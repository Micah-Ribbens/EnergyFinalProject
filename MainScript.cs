using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

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
    private float moneyInBank = 0f;

    // Other
    private bool hasCalledInitialization = false;
    private float timeWhenEnemyShoots = 0;
    private bool popupIsActive = false;
    private bool largeTextIsActive = false;
    private Vector3 enemyStartingPosition;
    private int greenOptionsChosen = 0;
    private int greenOptionsNeeded = 3;
    
    // Modifiable Values
    private float timeBeforeShooting = Constants.ENEMY_SHOOT_TIME;
    private float delayAfterSpawningForEnemyShooting = Constants.ENEMY_SHOOT_TIME_DELAY_AFTER_SPAWN;
    private float timeLeftUntilPlayerLosesMoney = Constants.TIME_UNTIL_PLAYER_LOSES_MONEY;
    
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
    
    // HUD Variables
    public Image piggyBankRedFill;
    public Canvas piggyBankCanvas;

    public Image moneyProportionRedFill;
    public GameObject moneyProportionCanvas;

    public Image enemyShootingUIFill;
    public Canvas enemyShootingUICanvas;

    private Image[] hudImages;
    private float totalEnemyShootTime = 0;
    private float energyUtilsCost = 0;

    private float enemySpeed = Constants.ENEMY_SPEED;
    private bool isPresenting = Constants.IS_PRESENTING;
    
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
        
        SetPopupActive(false);
        SetLargeTextCanvasActive(false);
        BeginNewDay(false, true);
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
        hudImages = new Image[] { moneyProportionRedFill, piggyBankRedFill, enemyShootingUIFill };
    }
    
    private void Update()
    {
        RunEveryFrame();
        
        if (playerMoney <= 0 && canCallRestartGame)
        {
            RestartGame();
            canCallRestartGame = false;
        } else if (!largeTextIsActive)
        {
            RunEnemyLogic();
            
            timeLeftUntilPlayerLosesMoney -= Time.deltaTime;

            if (timeLeftUntilPlayerLosesMoney <= 0)
            {
                float proportion = Constants.TIME_UNTIL_PLAYER_LOSES_MONEY / 60f;  // Proportion of a minute
                float hvacCost = Constants.HVAC_SYSTEM_COST_PER_MINUTE * proportion * houseEnergyEfficiency;
                float lightsCost = Constants.LIGHT_ENERGY_COST_PER_MINUTE * proportion * numberOfPlantsLeft * lightsEnergyEfficiency;
                float totalCost = (hvacCost + lightsCost) * energyCostMultiplier;
                
                playerMoney -= totalCost;
                timeLeftUntilPlayerLosesMoney = Constants.TIME_UNTIL_PLAYER_LOSES_MONEY;

                energyUtilsCost += totalCost;
            }

            RunHUDLogic();
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

    private void RunEnemyLogic()
    {
        Vector3 playerPosition = playerObject.transform.position;
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 movementVector = new Vector3(playerPosition.x - enemyPosition.x, 0, playerPosition.z - enemyPosition.z);

        if (movementVector.magnitude > Constants.ENEMY_FOLLOW_DISTANCE)  // If the enemy is close to the playerObject - do not move to the playerObject
        {
            movementVector.Normalize();
            
            // Going from a direction that is in world terms to my "local space"
            enemy.transform.Translate(movementVector * Time.deltaTime * enemySpeed, Space.World);
            
            double yRotation = Math.Atan2(movementVector.x, movementVector.z) * 180.0 / Math.PI;
            enemy.transform.eulerAngles = new Vector3(enemy.transform.eulerAngles.x,
                (float) yRotation + 90f, enemy.transform.eulerAngles.z);
        }

        if (timeWhenEnemyShoots <= Time.time)
        {
            SetEnemyShootTime(timeBeforeShooting);

            if (!isPresenting)
            {
                ShootBullets(movementVector);
            }
        }
        
    }

    private void ShootBullets(Vector3 movementVector)
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
    }

    private void SetObjectActions()
    {
        InteractableObject[] objects = { bed, securityCamera, energyProviderNewspaper, geothermalNewspaper};
        foreach (var obj in objects)
        {
            obj.SetActions(() => TriggerEnterAction(obj.gameObject), () => TriggerExitAction(obj.gameObject));
            
        }
    }

    private void Initialize()
    {
        SetObjectActions();
        
        enemyHouse.Place(enemyHouse.GetXBeginningEdge(), land.GetYBeginningEdge() + enemyHouseYOffBy, enemyHouse.GetZBeginningEdge());
        playerHouse.Place(playerHouse.GetXBeginningEdge(), land.GetYBeginningEdge() + playerHouseYOffBy, playerHouse.GetZBeginningEdge());
        bed.Place(bed.GetXBeginningEdge(), land.GetYBeginningEdge() + bedYOffBy, bed.GetZBeginningEdge());
        player.SetOnHitEnemyAction(() =>
        {
            SetEnemyShootTime(Constants.ENEMY_SHOOT_TIME_AFTER_PLAYER_HIT);
            // timeWhenEnemyShoots += Constants.ENEMY_SHOOT_TIME_AFTER_PLAYER_HIT;
            // totalEnemyShootTime += Constants.ENEMY_SHOOT_TIME_AFTER_PLAYER_HIT;
        });
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

    private void BeginNewDay(bool calculateMoney, bool shouldGiveText)
    {
        // I should increase the player's money here, but not reset the piggyBank until later
        if (calculateMoney)
        {
            enemyMoney += (Constants.PROFIT_FROM_HARVESTING_COW + Constants.COST_TO_PLANT_COW) * numberOfCowsLeft;
            enemyMoney += Constants.PROFIT_FROM_HARVESTING_LENTIL * numberOfPlantsLeft;
            playerMoney += moneyInBank;
        }
        
        // This must be here, so resetting the variables does not mess up the calculations
        UpdateGridsForNewDay();
        
        string dayStartText = GetDayStartText();
        energyUtilsCost = 0;
        moneyInBank = 0;
        

        if (GetPlayerMoneyProportion() >= Constants.MONEY_PROPORTION_NEEDED_TO_SEE_GREEN_NEWSPAPER 
            && greenOptionsChosen == greenOptionsNeeded)
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
            if (shouldGiveText)
            {
                // If the day number is 2 or 3 then the player should still see their profits!
                if (dayNumber <= 3 && dayNumber != 1) {
                    string text = "";
                    if (dayNumber == 2) text = Constants.DAY_2_TEXT;
                    if (dayNumber == 3) text = Constants.DAY_3_TEXT;
                    SetLargeTextCanvasActive(true);
                    largeText.text = text;
                    
                    SetOnButtonAction1(() =>
                    {
                        SetLargeTextCanvasActive(true);
                        largeText.text = dayStartText;
                        SetOnButtonAction1(NewDayButtonAction);
                    });
                }
                else
                {
                    SetLargeTextCanvasActive(true);
                    largeText.text = dayStartText;
                    SetOnButtonAction1(NewDayButtonAction);
                }


            } else
            {
                NewDayButtonAction();
            }
            
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
        SetEnemyShootTime(delayAfterSpawningForEnemyShooting + timeBeforeShooting);
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
        SetPopupActive(false);

        if (playerMoney >= Constants.GEOTHERMAL_ENERGY_COST)
        {
            largeText.text = Constants.GEOTHERMAL_NEWSPAPER_TEXT + "\n\nYou have $" + GetConverted(playerMoney) + " so you can afford it!" +
                             "\nPress 'X' to install Geothermal" + "\nPress 'Y' to Deny";
            SetButtonActionsForEnoughMoney();
        }

        else
        {
            largeText.text = Constants.GEOTHERMAL_NEWSPAPER_TEXT + "\n\nYou have $" + GetConverted(playerMoney) +
                             " so you sadly cannot afford it. Come back when you can." + "\n\nPress 'X' to continue";
            SetOnButtonAction1(() =>
            {
                SetLargeTextCanvasActive(false);
                SetPopupActive(popupIsActive);
                TriggerExitAction(geothermalNewspaperObject);
            });
        }
    }

    private void SetButtonActionsForEnoughMoney()
    {
        SetOnButtonAction1(() =>
        {
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            DisableInteractableObject(geothermalNewspaper);
            houseEnergyEfficiency -= Constants.GEOTHERMAL_EFFICIENCY_INCREASE;
            playerMoney -= Constants.GEOTHERMAL_ENERGY_COST;
            greenOptionsChosen++;
        });
        
        SetOnButtonAction2(() =>
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

        SetOnButtonAction1(() =>
        {
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            DisableInteractableObject(energyProviderNewspaper);
            greenOptionsChosen++;
            energyCostMultiplier = Constants.TREE_HUGGER_ENERGY_PROVIDER_COST_MULTIPLIER;
        });
        
        SetOnButtonAction2(() =>
        {
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            DisableInteractableObject(energyProviderNewspaper);
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
            SetLargeTextCanvasActive(false);
            SetPopupActive(popupIsActive);
            DisableInteractableObject(securityCamera);
            lightsEnergyEfficiency -= Constants.LED_LIGHT_EFFICIENCY_INCREASE;

        });
    }
    
    // Harvesting
    public void StealCow(GameObject cow)
    {
        DestroyInteractableObject(cow);
        moneyInBank += Constants.PROFIT_FROM_HARVESTING_COW;
        SetPopupActive(false);
        numberOfCowsLeft--;
        greenOptionsChosen--;  // Stealing a cow guarantees you can't win the game (not green)
    }

    public void HarvestPlant(GameObject plant)
    {
        DestroyInteractableObject(plant);
        moneyInBank += Constants.PROFIT_FROM_HARVESTING_LENTIL + Constants.COST_TO_PLANT_LENTIL;
        SetPopupActive(false);
        numberOfPlantsLeft--;
    }
    
    private void TriggerEnterAction(GameObject gameObject)
    {
        if (largeTextIsActive || gameObject == null) return;

        foreach (var key in tagOptions.Keys)
        {
            if (gameObject.CompareTag(key))
            {
                popupText.text = tagOptions.GetValueOrDefault(key, "");
                SetPopupActive(true);

                if (key == "Cow") SetOnGrabAction(() => StealCow(gameObject));
                else if (key == "Plant") SetOnGrabAction(() => HarvestPlant(gameObject));
                else if (key == "Bed") SetOnButtonAction1(() => BeginNewDay(true, true));
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
        moneyInBank = 0;
        dayNumber = 1;
        greenOptionsChosen = 0;
            
        canCallRestartGame = true;
        hasCollectedGreenNewspaper = false;
            
        geothermalNewspaperObject.SetActive(true);
        energyProviderNewspaperObject.SetActive(true);
        securityCameraObject.SetActive(true);
            
        DisableInteractableObject(greenNewspaper);
        SetObjectActions();
        greenNewspaper.SetActions(null, null);
        enemyHouse.SetActions(null, null);
            
        player.transform.position = playerStartPosition;
        BeginNewDay(false, true);
    }

    private void SetLargeTextCanvasActive(bool isActive)
    {
        if (!isActive && largeTextIsActive)
        {
            SetEnemyShootTime(timeBeforeShooting);
        }
        
        largeTextCanvas.SetActive(isActive);
        popup.SetActive(false);
        largeTextIsActive = isActive;
        player.SetIsActive(!isActive);
        SetHUDActive(!isActive);
    }

    private void SetPopupActive(bool isActive)
    {
        popup.SetActive(isActive);
        popupIsActive = isActive;
    }

    private string GetDayStartText()
    {
        if (dayNumber == 1) return Constants.DAY_1_TEXT;
        return "Day Number " + dayNumber + ". I have still not succeeded in stopping that cow farmer." +
               " I must continue trying. Day End Numbers:" + GetDayEndNumbersText(false);

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
            SetLargeTextCanvasActive(true);
            largeText.text = Constants.GOT_SHOT_TEXT + GetDayEndNumbersText(true);
            
            numberOfCowsLeft = cows.Length;
            numberOfPlantsLeft = plants.Length;

            SetOnButtonAction1(() =>
            {
                BeginNewDay(true, false);
                SetLargeTextCanvasActive(false);
            });
            
        }
        Destroy(bullet);
    }

    private float GetPlayerMoneyProportion()
    {
        if (enemyMoney <= 0)
        {
        }
        
        enemyMoney = Math.Max(0, enemyMoney);  // To prevent enemy money from being negative
        
        return playerMoney / (playerMoney + enemyMoney);
    }

    private void TakeGreenNewspaper()
    {
        DisableInteractableObject(greenNewspaper);
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

    private void DisableInteractableObject(InteractableObject interactableObject)
    {
        interactableObject.SetActions(null, null);
        interactableObject.gameObject.SetActive(false);
        TriggerExitAction(interactableObject.gameObject);
    }

    private void SetHUDActive(bool isActive)
    {
        piggyBankCanvas.gameObject.SetActive(isActive);
        enemyShootingUICanvas.gameObject.SetActive(isActive);
        moneyProportionCanvas.SetActive(isActive);
    }
    
    // UI Code
    private void RunHUDLogic()
    {
        float timeRemaining = Math.Max(timeWhenEnemyShoots - Time.time, 0f);
        float enemyShootTimeProportion = 1 - (timeRemaining / totalEnemyShootTime);

        float plantsTotalMoney = (Constants.PROFIT_FROM_HARVESTING_LENTIL + Constants.COST_TO_PLANT_LENTIL) * plants.Length;
        float cowTotalMoney = Constants.PROFIT_FROM_HARVESTING_COW * cows.Length;
        
        float piggyBankProportion = moneyInBank / (plantsTotalMoney + cowTotalMoney);

        float[] values = {1 - GetPlayerMoneyProportion(), 1 - piggyBankProportion, -enemyShootTimeProportion};
        SetHUDFills(values);
    }

    private void SetHUDFills(float[] values)
    {
        for (int i = 0; i < hudImages.Length; i++)
        {
            float value = values[i];
            Image image = hudImages[i];
            
            float constFactor = 20;
            float xPos =  (1 - Math.Abs(value)) * constFactor;

            if (value < 0) xPos = -xPos;  // So the negative is "kept"
                
            Vector2 pos = image.rectTransform.anchoredPosition;
            image.rectTransform.anchoredPosition = new Vector2(xPos, pos.y);

            Vector3 previousScale = image.rectTransform.localScale;
            image.rectTransform.localScale = new Vector3(Math.Abs(value), previousScale.y, previousScale.z);
        }
    }

    private void SetEnemyShootTime(float time)
    {
        timeWhenEnemyShoots = Time.time + time;
        totalEnemyShootTime = time;
    }

    private string GetDayEndNumbersText(bool hasBeenShot)
    {
        float cost = plants.Length * Constants.COST_TO_PLANT_LENTIL + energyUtilsCost;
        float profit = moneyInBank - cost;

        if (hasBeenShot) profit = -cost;
        
        int convertedUtilsCost = GetConverted(energyUtilsCost);
        int convertedProfit = GetConverted(profit);
        int convertedMoneyInBank = GetConverted(moneyInBank);
        int convertedPlayerMoney = GetConverted(playerMoney);
        int convertedEnemyMoney = GetConverted(enemyMoney);

        return "\n\nMoney In Piggy Bank: $" + convertedMoneyInBank + "\nLentils Harvested: " +
               (plants.Length - numberOfPlantsLeft) +
               "\nCows Stolen: " + (cows.Length - numberOfCowsLeft) + "\nEnergy Utils Cost: $" + convertedUtilsCost +
               "\nTotal Profit: $" + convertedProfit + "\nPlayer Money: $" + convertedPlayerMoney +
               "\nEnemy Money: $" + convertedEnemyMoney + "\n\nPress 'X' to continue";
    }

    private int GetConverted(float amount)
    {
        return (int)(amount / Constants.DIVIDE_FACTOR);
    }
}