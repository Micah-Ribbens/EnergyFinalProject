using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    private PlaceableObject[] plants;
    private PlaceableObject[] cows;
        
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
        
        // Instantiate Grids
        plants = new PlaceableObject[15];
        for (int i = 0; i < plants.Length; i++)
        {
            plants[i] = Instantiate(plantPrefab, transform.position, Quaternion.identity).GetComponent<PlaceableObject>();
            plants[i].transform.parent = container.transform;
        }

        
        cows = new PlaceableObject[15];
        for (int i = 0; i < cows.Length; i++)
        {
            cows[i] = Instantiate(cowPrefab, transform.position, Quaternion.identity).GetComponent<PlaceableObject>();
            cows[i].transform.parent = container.transform;
        }
    }

    private void Update()
    {
        if (!hasCalledInitialization)
        {
            Initialize();
            hasCalledInitialization = true;
        }
        
        // Moving enemy
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
        enemyHouse.SetActions(() => TriggerEnterAction(enemyHouse.gameObject), () => TriggerExitAction(enemyHouse.gameObject));
        enemyHouse.Place(enemyHouse.GetXBeginningEdge(), land.GetYBeginningEdge() + enemyHouseYOffBy, enemyHouse.GetZBeginningEdge());
        playerHouse.Place(playerHouse.GetXBeginningEdge(), land.GetYBeginningEdge() + playerHouseYOffBy, playerHouse.GetZBeginningEdge());
        bed.Place(bed.GetXBeginningEdge(), land.GetYBeginningEdge() + bedYOffBy, bed.GetZBeginningEdge());
        
        PlaceableObject plant = plants[0];
        
        int rows = 5;
        int columns = 3;
        Grid grid = new Grid(new Dimension(0f, 0f, plant.GetXSize() * columns * 2, plant.GetZSize() * rows * 2), 5, 3);
        grid.TurnIntoGrid(plants, plant.GetXSize(), plant.GetZSize(), land.GetYBeginningEdge() + plantYOffBy);

        PlaceableObject cow = cows[0];
        
        grid = new Grid(new Dimension(30f, 30f, cow.GetXSize() * columns * 2, cow.GetZSize() * rows * 2), 5, 3);
        grid.TurnIntoGrid(cows, cow.GetXSize(), cow.GetZSize(), land.GetYBeginningEdge() + cowYOffBy);
    }

    private void TriggerEnterAction(GameObject gameObject)
    {
        Debug.Log(gameObject);
        if (!tagOptions.ContainsKey("House"))
        {
            tagOptions.Add("House", "Press 'A' to Investigate House");
            tagOptions.Add("Cow", "Press 'A' to Steal Cow");    
            tagOptions.Add("Discoverable Object", "Press 'A' to Discover Technology");
        }
        
        foreach (var key in tagOptions.Keys)
        {
            if (gameObject.CompareTag(key))
            {
                text.text = tagOptions.GetValueOrDefault(key, "");
                popup.SetActive(true);
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
}