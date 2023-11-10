using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainScript : MonoBehaviour
{
    // Placeable Objects
    private InteractableObject enemyHouse;
    private PlaceableObject playerHouse;
    private PlaceableObject land;
    private PlaceableObject bed;
    public GameObject container;
    
    // Constant Variables (Objects)
    private GameObject enemy;
    public GameObject enemyCannonLeft;
    public GameObject enemyCannonRight;
    private GameObject player;
    public TextMeshProUGUI text;
    public GameObject popup;
    public GameObject plantPrefab;
    public GameObject bulletPrefab;
    public GameObject cowPrefab;
    
    // Data Management
    private Dictionary<string, string> tagOptions = new Dictionary<string, string>();
    private Dictionary<string, Action> tagToAction = new Dictionary<string, Action>();
    private InteractableObject[] plants;
    private InteractableObject[] cows;
    private float playerMoney = Constants.PLAYER_START_MONEY;
    private float enemyMoney = Constants.ENEMY_START_MONEY;
    private Action onHarvestAction;
    private Action onOptionBAction;
        
    // Other
    private bool hasCalledInitialization = false;
    private float timeWhenEnemyShoots = 0;
    
    // Modifiable Values
    private float timeBeforeShooting = 2f;
    
    // Calculated Constant Factors Off By
    private float playerHouseYOffBy = 1.1f;
    private float enemyHouseYOffBy = -0.975f;
    private float plantYOffBy = -0.284f;
    private float cowYOffBy = 0.05f;
    private float bedYOffBy = -.316f;
    
    // Grid Logic
    private bool needsToCallGridMethod = false;  // Whether the grid objects have beeen created and grid.turnIntoGrid() was never called
    
    
    private void Start()
    {
        // Grabbing variables from Unity Hub
        enemyHouse = GameObject.Find("EnemyHouse").GetComponent<InteractableObject>();
        playerHouse = GameObject.Find("PlayerHouse").GetComponent<PlaceableObject>();
        land = GameObject.Find("Land").GetComponent<PlaceableObject>();
        enemy = GameObject.Find("Enemy");
        player = GameObject.Find("Player");
        bed = GameObject.Find("Bed").GetComponent<PlaceableObject>();
        
        timeWhenEnemyShoots = Time.time + timeBeforeShooting;
        popup.SetActive(false);
        InstantiateGrids();
    }

    private void Update()
    {
        if (!hasCalledInitialization)
        {
            Initialize();
            hasCalledInitialization = true;
        }

        if (needsToCallGridMethod)
        {
            PutIntoGrids();
        }
        runEnemyLogic();
    }

    private void runEnemyLogic()
    {
        Vector3 playerPosition = player.transform.position;
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 movementVector = new Vector3(playerPosition.x - enemyPosition.x, 0, playerPosition.z - enemyPosition.z);

        if (movementVector.magnitude > 2)  // If the enemy is close to the player - do not move to the player
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
        enemyHouse.SetActions(() => TriggerEnterAction(enemyHouse), () => TriggerExitAction(enemyHouse.gameObject));
        enemyHouse.Place(enemyHouse.GetXBeginningEdge(), land.GetYBeginningEdge() + enemyHouseYOffBy, enemyHouse.GetZBeginningEdge());
        playerHouse.Place(playerHouse.GetXBeginningEdge(), land.GetYBeginningEdge() + playerHouseYOffBy, playerHouse.GetZBeginningEdge());
        bed.Place(bed.GetXBeginningEdge(), land.GetYBeginningEdge() + bedYOffBy, bed.GetZBeginningEdge());
    }

    private void TriggerEnterAction(InteractableObject interactableObject)
    {
        if (!tagOptions.ContainsKey("Cow"))
        {
            tagOptions.Add("Cow", "Press 'A' to Steal Cow");    
            tagOptions.Add("Plant", "Press 'A' to Harvest Plant");    
            tagOptions.Add("Discoverable Object", "Press 'A' to Discover Technology");
            
            // tagToAction.Add("Cow", () => StealCow(gameObject));    
            // tagToAction.Add("Plant", () => HarvestPlant(gameObject));    
            // tagToAction.Add("Discoverable Object", () => { });
        }
        
        foreach (var key in tagOptions.Keys)
        {
            if (interactableObject.gameObject.CompareTag(key))
            {
                text.text = tagOptions.GetValueOrDefault(key, "");
                popup.SetActive(true);

                if (key == "Cow") onHarvestAction = () => StealCow(interactableObject.gameObject);
                if (key == "Plant") onHarvestAction = () => HarvestPlant(interactableObject.gameObject);
                break;
            }
        }
    }

    private void TriggerExitAction(GameObject gameObject)
    {
        if (tagOptions.ContainsKey(gameObject.tag))
        {
            popup.SetActive(false);
        }
    }

    private void InstantiateGrids()
    {
        int numberOfPlants = (int)Math.Ceiling(playerMoney * Constants.PLAYER_PROPORTION_OF_MONEY_SPENT_ON_PLANTS * 1/Constants.COST_TO_PLANT_LENTIL);
        plants = new InteractableObject[numberOfPlants];
        InstantiateGrid(plants, plantPrefab);

        int numberOfCows = (int)Math.Ceiling(enemyMoney * Constants.PLAYER_PROPORTION_OF_MONEY_SPENT_ON_PLANTS * 1/Constants.COST_TO_PLANT_COW);
        cows = new InteractableObject[1];
        InstantiateGrid(cows, cowPrefab);

        needsToCallGridMethod = true;
    }

    private void InstantiateGrid(InteractableObject[] interactableObjects, GameObject prefab)
    {
        for (int i = 0; i < interactableObjects.Length; i++)
        {
            interactableObjects[i] = Instantiate(prefab, transform.position, Quaternion.identity).GetComponent<InteractableObject>();
            InteractableObject obj = interactableObjects[i];
            obj.transform.parent = container.transform;
            obj.SetActions(() => TriggerEnterAction(obj), 
                                            () => TriggerExitAction(obj.gameObject));
            
        }
        
    }

    private void PutIntoGrids()
    {
        PutIntoGrid(cows, 30f, 30f, cowYOffBy);
        PutIntoGrid(plants, 0f, 0f, plantYOffBy);
        
        needsToCallGridMethod = false;
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
    
    // Harvesting
    public void StealCow(GameObject cow)
    {
        Destroy(cow);
        playerMoney += Constants.PROFIT_FROM_HARVESTING_COW;
        popup.SetActive(false);
    }

    public void HarvestPlant(GameObject plant)
    {
        Destroy(plant);
        playerMoney += Constants.PROFIT_FROM_HARVESTING_LENTIL;
        popup.SetActive(false);
    }
    
    // On Actions
    public void OnHarvest(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            if (onHarvestAction != null)
            {
                onHarvestAction();
            }
        }
    }
    
    public void OnOptionB(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            if (onOptionBAction != null)
            {
                onOptionBAction();
            }
        }
    }
}