using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class MainScript : MonoBehaviour
{
    // Placeable Objects
    private InteractableObject enemyHouse;
    private PlaceableObject playerHouse;
    private PlaceableObject land;
    private InteractableObject bed;
    private InteractableObject geothermalNewspaper;
    private InteractableObject energyProviderNewspaper;
    
    // GameObject's gotten from Unity Editor
    public GameObject enemyHouseObject;
    public GameObject playerHouseObject;
    public GameObject landObject;
    public GameObject bedObject;
    public GameObject container;
    public GameObject geothermalNewspaperObject;
    public GameObject energyProviderNewspaperObject;
    
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
    private Dictionary<string, Action> tagToAction = new Dictionary<string, Action>();
    private InteractableObject[] plants = new InteractableObject[1];
    private InteractableObject[] cows = new InteractableObject[1];
    private Action onButtonPressA;
    private Action onButtonPressB;
    
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
    
    // Modifiable Values
    private float timeBeforeShooting = 2f;
    
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
    private Vector3 playerOriginalPosition;

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

        playerOriginalPosition = player.transform.position;
        
        timeWhenEnemyShoots = Time.time + timeBeforeShooting;
        SetPopupActive(false);
        SetLargeTextCanvasActive(false);
        BeginNewDay(false);
        
        // Tags
        tagOptions.Add("Cow", "Press 'A' to Steal Cow");    
        tagOptions.Add("Plant", "Press 'A' to Harvest Plant");    
        tagOptions.Add("DiscoverableObject", "Press 'A' to Discover Technology");
        tagOptions.Add("Bed", "Press 'A' to Start New Day");
    }
    
    private void Update()
    {
        framesBeforeCallingGridMethod--;
        
        if (framesBeforeCallingGridMethod == 0)
        {
            PutIntoGrids();
        }
        
        if (playerMoney <= 0 && canCallRestartGame)
        {
            RestartGame();
            canCallRestartGame = false;
            return;
        }

        if (buttonAWasPressed && onButtonPressA != null)
        {
            buttonAWasPressed = false;
            onButtonPressA();
        }

        if (buttonBWasPressed && onButtonPressB != null)
        {
            buttonBWasPressed = false;
            onButtonPressB();
        }
        
        if (largeTextIsActive)
        {
            return;
        }
        
        if (!hasCalledInitialization)
        {
            Initialize();
            hasCalledInitialization = true;
        }


        
        runEnemyLogic();
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
            
            bullet1.GetComponent<Bullet>().SetMovementVector(movementVector);
            bullet2.GetComponent<Bullet>().SetMovementVector(movementVector);
            
            timeWhenEnemyShoots = Time.time + timeBeforeShooting;
        }
        
    }

    private void Initialize()
    {
        bed.SetActions(() => TriggerEnterAction(bed.gameObject), () => TriggerExitAction(bed.gameObject));
        enemyHouse.Place(enemyHouse.GetXBeginningEdge(), land.GetYBeginningEdge() + enemyHouseYOffBy, enemyHouse.GetZBeginningEdge());
        playerHouse.Place(playerHouse.GetXBeginningEdge(), land.GetYBeginningEdge() + playerHouseYOffBy, playerHouse.GetZBeginningEdge());
        bed.Place(bed.GetXBeginningEdge(), land.GetYBeginningEdge() + bedYOffBy, bed.GetZBeginningEdge());
    }



    private void InstantiateGrids()
    {
        int numberOfPlants = (int)Math.Ceiling(playerMoney * Constants.PLAYER_PROPORTION_OF_MONEY_SPENT_ON_PLANTS * 1/Constants.COST_TO_PLANT_LENTIL);
        plants = new InteractableObject[numberOfPlants];
        InstantiateGrid(plants, plantPrefab);

        int numberOfCows = (int)Math.Ceiling(enemyMoney * Constants.PLAYER_PROPORTION_OF_MONEY_SPENT_ON_PLANTS * 1/Constants.COST_TO_PLANT_COW);
        cows = new InteractableObject[1];
        InstantiateGrid(cows, cowPrefab);


        framesBeforeCallingGridMethod = 2;
        enemyMoney -= Constants.COST_TO_PLANT_COW * numberOfPlants;
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
        foreach (InteractableObject interactableObject in cows)
        {
            if (interactableObject != null) Destroy(interactableObject.gameObject);
        }
        
        foreach (InteractableObject interactableObject in plants)
        {
            if (interactableObject != null) Destroy(interactableObject.gameObject);
        }

        if (calculateMoney)
        {
            enemyMoney += (Constants.PROFIT_FROM_HARVESTING_COW + Constants.COST_TO_PLANT_COW) * numberOfCowsLeft;
            enemyMoney += Constants.PROFIT_FROM_HARVESTING_LENTIL * numberOfPlantsLeft;
            playerMoney += playerMoneyBuffer;
            playerMoneyBuffer = 0;
        }

        if (playerMoney == 0) return;
        
        SetLargeTextCanvasActive(true);
        largeText.text = GetDayStartText();

        SetOnButtonPressA(() =>
        {
            Debug.Log("New Day Stuff");
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
        });

        InstantiateGrids();
        dayNumber++;
    }

    private void PutIntoGrids()
    {
        PutIntoGrid(cows, 30f, 30f, cowYOffBy);
        PutIntoGrid(plants, 0f, 0f, plantYOffBy);
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

        SetOnButtonPressA(() =>
        {
            Debug.Log("Geothermal Stuff");
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            houseEnergyEfficiency -= Constants.GEOTHERMAL_EFFICIENCY_INCREASE;
        });
        
        SetOnButtonPressB(() =>
        {
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
        });

    }
    
    private void DiscoverEnergyProvider()
    {
        SetLargeTextCanvasActive(true);
        largeText.text = Constants.ENERGY_PROVIDER_NEWSPAPER_TEXT;
        SetPopupActive(false);

        SetOnButtonPressA(() =>
        {
            Debug.Log("Energy Provider");
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            energyCostMultiplier = Constants.LIGHTNING_ENERGY_PROVIDER_COST_MULTIPLIER;
        });
        
        SetOnButtonPressB(() =>
        {
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            energyCostMultiplier = Constants.TREE_HUGGER_ENERGY_PROVIDER_COST_MULTIPLIER;
        });

    }

    private void DiscoverLEDTechnology()
    {
        SetLargeTextCanvasActive(true);
        largeText.text = Constants.LED_CAMERA_TEXT;
        SetPopupActive(false);

        SetOnButtonPressA(() =>
        {
            Debug.Log("LED Stuff");
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            lightsEnergyEfficiency -= Constants.LED_LIGHT_EFFICIENCY_INCREASE;

        });
    }
    
    // Harvesting
    public void StealCow(GameObject cow)
    {
        Destroy(cow);
        playerMoneyBuffer += Constants.PROFIT_FROM_HARVESTING_COW;
        SetPopupActive(false);
        numberOfCowsLeft--;
    }

    public void HarvestPlant(GameObject plant)
    {
        Destroy(plant);
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

                if (key == "Cow") SetOnButtonPressA(() => StealCow(gameObject));
                else if (key == "Plant") SetOnButtonPressA(() => HarvestPlant(gameObject));
                else if (key == "Bed") SetOnButtonPressA(() => BeginNewDay(true));
                else if (key == "EnergyProviderNewspaper") SetOnButtonPressA(DiscoverEnergyProvider);
                else if (key == "GeothermalNewspaper") SetOnButtonPressA(DiscoverGeothermal);
                break;
            }
        }
    }


    private void TriggerExitAction(GameObject gameObject)
    {
        if (tagOptions.ContainsKey(gameObject.tag))
        {
            SetPopupActive(false);
        }
    }
    
    // On Actions
    public void OnButtonPressA(InputAction.CallbackContext context)
    {
        buttonAWasPressed = context.action.triggered;
    }
    
    public void OnButtonPressB(InputAction.CallbackContext context)
    {
        buttonBWasPressed = context.action.triggered;
    }

    private void RestartGame()
    {
        largeText.text = Constants.GAME_OVER_TEXT;
        SetLargeTextCanvasActive(true);
        SetPopupActive(false);
        
        SetOnButtonPressA(() =>
        {
            Debug.Log("Restart Stuff");
            SetPopupActive(false);
            timeWhenEnemyShoots = Time.time + timeBeforeShooting;
            SetLargeTextCanvasActive(false);
            playerMoney = Constants.PLAYER_START_MONEY;
            enemyMoney = Constants.ENEMY_START_MONEY;
            energyCostMultiplier = 1;
            houseEnergyEfficiency = 1;
            lightsEnergyEfficiency = 1;
            playerMoneyBuffer = 0;
            BeginNewDay(false);
            playerObject.SetActive(true);
            playerObject.transform.position = playerOriginalPosition;
            canCallRestartGame = true;
            dayNumber = 1;
        });
    }

    private void SetLargeTextCanvasActive(bool isActive)
    {
        largeTextCanvas.SetActive(isActive);
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

Press 'A' to continue";

    }

    private void SetOnButtonPressA(Action action)
    {
        onButtonPressA = action;
    }
    
    private void SetOnButtonPressB(Action action)
    {
        onButtonPressB = action;
    }
}