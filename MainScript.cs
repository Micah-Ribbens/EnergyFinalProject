using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

// TODO fix button mapping glitch!
public class MainScript : MonoBehaviour
{
    // Placeable Objects
    private InteractableObject enemyHouse;
    private PlaceableObject playerHouse;
    private PlaceableObject land;
    private InteractableObject bed;
    private InteractableObject geothermalNewspaper;
    private InteractableObject energyProviderNewspaper;
    private InteractableObject securityCamera;
    private InteractableObject greenNewspaper;

    // GameObject's gotten from Unity Editor
    public GameObject enemyHouseObject;
    public GameObject playerHouseObject;
    public GameObject landObject;
    public GameObject bedObject;
    public GameObject container;
    public GameObject geothermalNewspaperObject;
    public GameObject energyProviderNewspaperObject;
    public GameObject securityCameraObject;
    public GameObject greenNewspaperObject;

    // Constant Variables (Objects)
    public GameObject enemy;
    public GameObject enemyCannonLeft;
    public GameObject enemyCannonRight;
    public GameObject playerObject;
    public Player player;
    public TextMeshProUGUI popupText;
    public GameObject popup;
    public TextMeshProUGUI largeText;
    public GameObject largeTextCanvas;
    public GameObject plantPrefab;
    public GameObject bulletPrefab;
    public GameObject cowPrefab;
    
    // Data Management
    private Dictionary<string, string> tagOptions = new Dictionary<string, string>();
    private InteractableObject[] plants = new InteractableObject[1];
    private InteractableObject[] cows = new InteractableObject[1];
    private Action onButtonAction1;
    private Action onButtonAction2;
    private Action onGrabAction;
    private bool grabButtonWasPressed;
    private bool hasCollectedGreenNewspaper;
    
    // Money
    private int numberOfCowsLeft;
    private int numberOfPlantsLeft;
    private float playerMoney = Constants.PLAYER_START_MONEY;
    private float enemyMoney = Constants.ENEMY_START_MONEY;
    private float playerMoneyBuffer = 0f;

    // Other
    private bool hasCalledInitialization = false;
    private float timeWhenEnemyShoots = 0;
    private bool popupIsActive = false;
    private bool largeTextIsActive = false;
    private Vector3 enemyStartingPosition;
    private int greenOptionsChosen = 0;
    private int greenOptionsNeeded = 3;
    
    // Modifiable Values
    private float timeBeforeShooting = 5f;
    private float delayAfterSpawningForEnemyShooting = 10f;
    
    // Calculated Constant Factors Off By
    private float playerHouseYOffBy = 1.1f;
    private float enemyHouseYOffBy = -0.975f;
    private float plantYOffBy = -.1f;
    private float cowYOffBy = 0.05f;
    private float bedYOffBy = -.316f;
    
    // Grid Logic
    private int framesBeforeCallingGridMethod = 2;  // Whether the grid objects have beeen created and grid.turnIntoGrid() was never called
    
    // Energy Efficiencies
    private float houseEnergyEfficiency = 1;
    private float lightsEnergyEfficiency = 1;
    private float energyCostMultiplier = 1;
    private Vector3 playerStartPosition;

    private int dayNumber = 1;

    private bool buttonAWasPressed = false;
    private bool buttonBWasPressed = false;
    private bool canCallRestartGame = true;
    
    
    private void Start()
    {
        // Grabbing variables from Unity Hub
        enemyHouse = enemyHouseObject.GetComponent<InteractableObject>();
        playerHouse = playerHouseObject.GetComponent<PlaceableObject>();
        land = landObject.GetComponent<PlaceableObject>();
        bed = bedObject.GetComponent<InteractableObject>();
        geothermalNewspaper = geothermalNewspaperObject.GetComponent<InteractableObject>();
        energyProviderNewspaper = energyProviderNewspaperObject.GetComponent<InteractableObject>();
        player = playerObject.GetComponent<Player>();
        securityCamera = securityCameraObject.GetComponent<InteractableObject>();
        greenNewspaper = greenNewspaperObject.GetComponent<InteractableObject>();

        enemyStartingPosition = enemy.transform.position;
        playerStartPosition = player.transform.position;
        
        timeWhenEnemyShoots = Time.time + timeBeforeShooting;
        SetPopupActive(false);
        SetLargeTextCanvasActive(false);
        BeginNewDay(false);
        greenNewspaperObject.SetActive(false);
        
        // Tags
        tagOptions.Add("Cow", "Press 'B' to Steal Cow");    
        tagOptions.Add("Plant", "Press 'B' to Harvest Plant");    
        tagOptions.Add("GeothermalNewspaper", "Press 'B' to Discover Technology");
        tagOptions.Add("EnergyProviderNewspaper", "Press 'B' to Discover Technology");
        tagOptions.Add("SecurityCamera", "Press 'B' to Discover Technology");
        tagOptions.Add("Bed", "Press 'X' to Start New Day");
        tagOptions.Add("GreenNewspaper", "Press 'B' To Collect Tree Hugger Newsletter");
        tagOptions.Add("EnemyHouse", "Press 'B' To Place Tree Hugger Newsletter");
        
    }
    
    private void Update()
    {
        RunEveryFrame();
        
        if (playerMoney <= 0 && canCallRestartGame)
        {
            RestartGame();
            canCallRestartGame = false;
        }
        // RunButtonLogic();
        else if (!largeTextIsActive)
        {
            runEnemyLogic();
        }
        

        
    }

    private void RunEveryFrame()
    {
        framesBeforeCallingGridMethod--;
        
        if (!hasCalledInitialization)
        {
            Initialize();
            hasCalledInitialization = true;
        }
        
        if (framesBeforeCallingGridMethod == 0)
        {
            PutIntoGrids();
        }


        
        RunButtonLogic();
    }

    private void RunButtonLogic()
    {
        if (buttonAWasPressed && onButtonAction1 != null)
        {
            buttonAWasPressed = false;
            onButtonAction1();
        }

        if (buttonBWasPressed && onButtonAction2 != null)
        {
            buttonBWasPressed = false;
            onButtonAction2();
        }

        if (grabButtonWasPressed && onGrabAction != null)
        {
            grabButtonWasPressed = false;
            onGrabAction();
        }
        
    }

    private void runEnemyLogic()
    {
        Vector3 playerPosition = playerObject.transform.position;
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 movementVector = new Vector3(playerPosition.x - enemyPosition.x, 0, playerPosition.z - enemyPosition.z);

        if (movementVector.magnitude > 2)  // If the enemy is close to the playerObject - do not move to the playerObject
        {
            movementVector.Normalize();
            
            // Going from a direction that is in world terms to my "local space"
            enemy.transform.Translate(movementVector * Time.deltaTime * 5f, Space.World);
            
            double yRotation = Math.Atan2(movementVector.x, movementVector.z) * 180.0 / Math.PI;
            enemy.transform.eulerAngles = new Vector3(enemy.transform.eulerAngles.x,
                (float) yRotation + 90f, enemy.transform.eulerAngles.z);
        }

        if (timeWhenEnemyShoots <= Time.time)
        {
            movementVector.Normalize();

            Vector3 bulletRotation = enemy.transform.eulerAngles + bulletPrefab.transform.eulerAngles;
            GameObject bullet1 = Instantiate(bulletPrefab, enemyCannonLeft.transform.position, Quaternion.Euler(bulletRotation));
            GameObject bullet2 = Instantiate(bulletPrefab, enemyCannonRight.transform.position, Quaternion.Euler(bulletRotation));

            bullet1.transform.parent = container.transform;
            bullet2.transform.parent = container.transform;

            Bullet bullet1Script = bullet1.GetComponent<Bullet>();
            Bullet bullet2Script = bullet2.GetComponent<Bullet>();
            
            bullet1Script.SetMovementVector(movementVector);
            bullet2Script.SetMovementVector(movementVector);
            
            bullet1Script.SetOnTriggerEnterAction(() => RunBulletCollision(bullet1));
            bullet2Script.SetOnTriggerEnterAction(() => RunBulletCollision(bullet2));
            
            timeWhenEnemyShoots = Time.time + timeBeforeShooting;
        }
        
    }

    private void Initialize()
    {
        InteractableObject[] objects = { bed, securityCamera, energyProviderNewspaper, geothermalNewspaper};
        foreach (var obj in objects)
        {
            obj.SetActions(() => TriggerEnterAction(obj.gameObject), () => TriggerExitAction(obj.gameObject));
            
        }
        
        enemyHouse.Place(enemyHouse.GetXBeginningEdge(), land.GetYBeginningEdge() + enemyHouseYOffBy, enemyHouse.GetZBeginningEdge());
        playerHouse.Place(playerHouse.GetXBeginningEdge(), land.GetYBeginningEdge() + playerHouseYOffBy, playerHouse.GetZBeginningEdge());
        bed.Place(bed.GetXBeginningEdge(), land.GetYBeginningEdge() + bedYOffBy, bed.GetZBeginningEdge());
    }



    private void InstantiateGrids()
    {
        int numberOfPlants = (int)Math.Ceiling(Constants.PLAYER_PROPORTION_OF_MONEY_SPENT_ON_PLANTS * playerMoney/Constants.COST_TO_PLANT_LENTIL);
        plants = new InteractableObject[numberOfPlants];
        InstantiateGrid(plants, plantPrefab);

        int numberOfCows = (int)Math.Ceiling(Constants.ENEMY_PROPORTION_OF_MONEY_SPENT_ON_COWS * enemyMoney/Constants.COST_TO_PLANT_COW);
        cows = new InteractableObject[numberOfCows];
        InstantiateGrid(cows, cowPrefab);


        framesBeforeCallingGridMethod = 2;
        enemyMoney -= Constants.COST_TO_PLANT_COW * numberOfCows;
        playerMoney -= Constants.COST_TO_PLANT_LENTIL * numberOfPlants;

        numberOfCowsLeft = numberOfCows;
        numberOfPlantsLeft = numberOfPlants;

    }

    private void InstantiateGrid(InteractableObject[] interactableObjects, GameObject prefab)
    {
        for (int i = 0; i < interactableObjects.Length; i++)
        {
            interactableObjects[i] = Instantiate(prefab).GetComponent<InteractableObject>();
            InteractableObject obj = interactableObjects[i];
            obj.transform.parent = container.transform;
            obj.SetActions(() => TriggerEnterAction(obj.gameObject), 
                            () => TriggerExitAction(obj.gameObject));
            
        }
    }

    private void BeginNewDay(bool calculateMoney)
    {
        if (calculateMoney)
        {
            enemyMoney += (Constants.PROFIT_FROM_HARVESTING_COW + Constants.COST_TO_PLANT_COW) * numberOfCowsLeft;
            enemyMoney += Constants.PROFIT_FROM_HARVESTING_LENTIL * numberOfPlantsLeft;
            playerMoney += playerMoneyBuffer;
            playerMoneyBuffer = 0;
        }
        
        UpdateGridsForNewDay();
        Debug.Log("money good: " + (GetPlayerMoneyProportion() >= .5f));
        Debug.Log("Green options good: " + (greenOptionsChosen == greenOptionsNeeded));

        if (GetPlayerMoneyProportion() >= .5f && greenOptionsChosen == greenOptionsNeeded)
        {
            greenNewspaper.SetActions(() => TriggerEnterAction(greenNewspaperObject),
                                        () => TriggerExitAction(greenNewspaperObject));
            
            greenNewspaperObject.SetActive(true);
        } else if (enemyMoney <= 0)
        {
            SetLargeTextCanvasActive(true);
            largeText.text = Constants.ALL_MONEY_TEXT;
            SetOnButtonAction1(ResetGameVariables);
        }

        if (enemyMoney > 0)
        {
            SetLargeTextCanvasActive(true);
            largeText.text = GetDayStartText();

            SetOnButtonAction1(NewDayButtonAction);
            dayNumber++;
        }
        

    }

    private void UpdateGridsForNewDay()
    {
        foreach (InteractableObject interactableObject in cows)
        {
            if (interactableObject != null) Destroy(interactableObject.gameObject);
        }
        
        foreach (InteractableObject interactableObject in plants)
        {
            if (interactableObject != null) Destroy(interactableObject.gameObject);
        }
        
        InstantiateGrids();
    }

    private void NewDayButtonAction()
    {
        SetLargeTextCanvasActive(false);
        SetPopupActive(popupIsActive);
        timeWhenEnemyShoots = delayAfterSpawningForEnemyShooting + timeBeforeShooting + Time.time;
        enemy.transform.position = enemyStartingPosition;
        player.SetPosition(playerStartPosition);
    }
    
    
    private void PutIntoGrids()
    {
        PutIntoGrid(cows, 30f, 30f, cowYOffBy);
        PutIntoGrid(plants, -5f, -5f, plantYOffBy);
    }

    private void PutIntoGrid(InteractableObject[] interactableObjects, float xStartEdge, float zStartEdge, float yOffBy)
    {
        InteractableObject interactableObject = interactableObjects[0];
        int rows = (int) Math.Sqrt(interactableObjects.Length);
        int columns = (int) Math.Ceiling(interactableObjects.Length / (double) rows);
        Grid grid = new Grid(new Dimension(xStartEdge, zStartEdge, interactableObject.GetXSize() * columns * 2, 
                        interactableObject.GetZSize() * rows * 2), rows, columns);
        grid.TurnIntoGrid(interactableObjects, interactableObject.GetXSize(), interactableObject.GetZSize(), land.GetYBeginningEdge() + yOffBy);
    }
    
    // Technology
    private void DiscoverGeothermal()
    {
        SetLargeTextCanvasActive(true);
        largeText.text = Constants.GEOTHERMAL_NEWSPAPER_TEXT;
        SetPopupActive(false);

        SetOnButtonAction1(() =>
        {
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            DisableInteractableObject(geothermalNewspaperObject);
            houseEnergyEfficiency -= Constants.GEOTHERMAL_EFFICIENCY_INCREASE;
            greenOptionsChosen++;
        });
        
        SetOnButtonAction2(() =>
        {
            DisableInteractableObject(geothermalNewspaperObject);
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
        });

    }
    
    private void DiscoverEnergyProvider()
    {
        SetLargeTextCanvasActive(true);
        largeText.text = Constants.ENERGY_PROVIDER_NEWSPAPER_TEXT;
        SetPopupActive(false);

        SetOnButtonAction1(() =>
        {
            DisableInteractableObject(energyProviderNewspaperObject);
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            greenOptionsChosen++;
            energyCostMultiplier = Constants.TREE_HUGGER_ENERGY_PROVIDER_COST_MULTIPLIER;
        });
        
        SetOnButtonAction2(() =>
        {
            DisableInteractableObject(energyProviderNewspaperObject);
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            energyCostMultiplier = Constants.LIGHTNING_ENERGY_PROVIDER_COST_MULTIPLIER;
        });

    }

    private void DiscoverLEDTechnology()
    {
        SetLargeTextCanvasActive(true);
        largeText.text = Constants.LED_CAMERA_TEXT;
        SetPopupActive(false);

        SetOnButtonAction1(() =>
        {
            greenOptionsChosen++;
            DisableInteractableObject(securityCameraObject);
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            lightsEnergyEfficiency -= Constants.LED_LIGHT_EFFICIENCY_INCREASE;

        });
    }
    
    // Harvesting
    public void StealCow(GameObject cow)
    {
        DestroyInteractableObject(cow);
        playerMoneyBuffer += Constants.PROFIT_FROM_HARVESTING_COW;
        SetPopupActive(false);
        numberOfCowsLeft--;
        greenOptionsChosen--;  // Stealing a cow guarantees you can't win the game (not green)
    }

    public void HarvestPlant(GameObject plant)
    {
        DestroyInteractableObject(plant);
        playerMoneyBuffer += Constants.PROFIT_FROM_HARVESTING_LENTIL;
        SetPopupActive(false);
        numberOfPlantsLeft--;
    }
    
    private void TriggerEnterAction(GameObject gameObject)
    {
        if (largeTextIsActive) return;
        
        foreach (var key in tagOptions.Keys)
        {
            if (gameObject.CompareTag(key))
            {
                popupText.text = tagOptions.GetValueOrDefault(key, "");
                SetPopupActive(true);

                if (key == "Cow") SetOnGrabAction(() => StealCow(gameObject));
                else if (key == "Plant") SetOnGrabAction(() => HarvestPlant(gameObject));
                else if (key == "Bed") SetOnButtonAction1(() => BeginNewDay(true));
                else if (key == "EnergyProviderNewspaper") SetOnGrabAction(DiscoverEnergyProvider);
                else if (key == "GeothermalNewspaper") SetOnGrabAction(DiscoverGeothermal);
                else if (key == "SecurityCamera") SetOnGrabAction(DiscoverLEDTechnology);
                else if (key == "GreenNewspaper") SetOnGrabAction(TakeGreenNewspaper);
                else if (key == "EnemyHouse") SetOnGrabAction(PlaceGreenNewspaper);
                break;
            }
        }
    }


    private void TriggerExitAction(GameObject gameObject)
    {
        if (tagOptions.ContainsKey(gameObject.tag) && !largeTextIsActive)
        {
            SetPopupActive(false);
            onButtonAction1 = null;
            onButtonAction2 = null;
            onGrabAction = null;
        }
    }

    private void DestroyInteractableObject(GameObject gameObject)
    {
        Destroy(gameObject);
        TriggerExitAction(gameObject);
    }
    
    // On Actions
    public void OnButtonAction1(InputAction.CallbackContext context)
    {
        buttonAWasPressed = context.action.triggered;
    }

    public void OnGrabAction(InputAction.CallbackContext context)
    {
        grabButtonWasPressed = context.action.triggered;
    }
    
    public void OnButtonAction2(InputAction.CallbackContext context)
    {
        buttonBWasPressed = context.action.triggered;
    }

    private void RestartGame()
    {
        largeText.text = Constants.GAME_OVER_TEXT;
        SetLargeTextCanvasActive(true);
        SetPopupActive(false);

        SetOnButtonAction1(ResetGameVariables);
    }

    private void ResetGameVariables()
    {
        SetPopupActive(false);
        SetLargeTextCanvasActive(false);
            
        playerMoney = Constants.PLAYER_START_MONEY;
        enemyMoney = Constants.ENEMY_START_MONEY;
            
        energyCostMultiplier = 1;
        houseEnergyEfficiency = 1;
        lightsEnergyEfficiency = 1;
        playerMoneyBuffer = 0;
        dayNumber = 1;
        greenOptionsChosen = 0;
            
        canCallRestartGame = true;
        hasCollectedGreenNewspaper = false;
            
        geothermalNewspaperObject.SetActive(true);
        energyProviderNewspaperObject.SetActive(true);
        securityCameraObject.SetActive(true);
            
        DisableInteractableObject(greenNewspaperObject);
        greenNewspaper.SetActions(null, null);
        enemyHouse.SetActions(null, null);
            
        BeginNewDay(false);
    }

    private void SetLargeTextCanvasActive(bool isActive)
    {
        if (!isActive && largeTextIsActive)
        {
            timeWhenEnemyShoots = Time.time + timeBeforeShooting;
        }
        
        largeTextCanvas.SetActive(isActive);
        popup.SetActive(false);
        largeTextIsActive = isActive;
        player.SetIsActive(!isActive);
    }

    private void SetPopupActive(bool isActive)
    {
        popup.SetActive(isActive);
        popupIsActive = isActive;
    }

    private string GetDayStartText()
    {
        if (dayNumber == 1) return Constants.DAY_1_TEXT;

        return "Day Number " + dayNumber + @". I have still not succeeded in stopping that cow farmer. I must continue trying.

Press 'X' to continue";

    }

    private void SetOnButtonAction1(Action action)
    {
        onButtonAction1 = action;
    }
    
    private void SetOnButtonAction2(Action action)
    {
        onButtonAction2 = action;
    }

    private void SetOnGrabAction(Action action)
    {
        onGrabAction = action;
    }

    private void RunBulletCollision(GameObject bullet)
    {
        if (!largeTextIsActive)
        {
            numberOfCowsLeft = cows.Length;
            numberOfPlantsLeft = plants.Length;
            SetLargeTextCanvasActive(true);
            largeText.text = Constants.GOT_SHOT_TEXT;

            SetOnButtonAction1(() =>
            {
                BeginNewDay(true);
            });
        }
        Destroy(bullet);
    }

    private float GetPlayerMoneyProportion()
    {
        if (enemyMoney <= 0)
        {
            Debug.Log("ERROR: ENEMY MONEY SHOULD ALWAYS BE POSITIVE, BUT IT IS NEGATIVE!");
        }
        
        enemyMoney = Math.Max(0, enemyMoney);  // To prevent enemy money from being negative
        return playerMoney / (playerMoney + enemyMoney);
    }

    private void TakeGreenNewspaper()
    {
        DisableInteractableObject(greenNewspaperObject);
        hasCollectedGreenNewspaper = true;
        SetPopupActive(false);
        largeText.text = Constants.GREEN_NEWSPAPER_TEXT;
        SetLargeTextCanvasActive(true);
        
        SetOnButtonAction1(() =>
        {
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            enemyHouse.SetActions(() => TriggerEnterAction(enemyHouseObject), 
                () => TriggerExitAction(enemyHouseObject));
        });
    }

    private void PlaceGreenNewspaper()
    {
        SetLargeTextCanvasActive(true);
        largeText.text = Constants.GAME_WON_TEXT;
        SetOnButtonAction1(ResetGameVariables);
        TriggerExitAction(enemyHouseObject);
    }

    private void DisableInteractableObject(GameObject gameObject)
    {
        gameObject.SetActive(false);
        TriggerExitAction(gameObject);
    }
}