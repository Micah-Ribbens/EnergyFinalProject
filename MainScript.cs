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
    
    // Constant Variables (Objects)
    private GameObject enemy;
    public GameObject enemyCannonLeft;
    public GameObject enemyCannonRight;
    private GameObject player;
    public TextMeshProUGUI text;
    public GameObject popup;
    public GameObject plantPrefab;
    public GameObject bulletPrefab;
    
    // Data Management
    private Dictionary<string, string> tagOptions = new Dictionary<string, string>();
    private PlaceableObject[] plants;
    private PlaceableObject enemyPlacableObject;
        
    // Other
    private bool hasCalledInitialization = false;
    private float timeWhenEnemyShoots = 0;
    
    // Modifiable Values
    private float timeBeforeShooting = 2f;
    
    private void Start()
    {
        // Grabbing variables from Unity Hub
        enemyHouse = GameObject.Find("EnemyHouse").GetComponent<InteractableObject>();
        playerHouse = GameObject.Find("PlayerHouse").GetComponent<PlaceableObject>();
        land = GameObject.Find("Land").GetComponent<PlaceableObject>();
        enemy = GameObject.Find("Enemy");
        enemyPlacableObject = enemy.GetComponent<PlaceableObject>();
        player = GameObject.Find("Player");
        
        Debug.Log("x " + enemyHouse.GetX() + " y " + enemyHouse.GetY() + " z " + enemyHouse.GetZ());

        popup.SetActive(false);
        
        plants = new PlaceableObject[15];
        for (int i = 0; i < plants.Length; i++)
        {
            plants[i] = Instantiate(plantPrefab, transform.position, Quaternion.identity).GetComponent<PlaceableObject>();
        }

        timeWhenEnemyShoots = Time.time + timeBeforeShooting;

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
            
            bullet1.GetComponent<Bullet>().SetMovementVector(movementVector);
            bullet2.GetComponent<Bullet>().SetMovementVector(movementVector);
            
            timeWhenEnemyShoots = Time.time + timeBeforeShooting;
        }
        
    }

    private void Initialize()
    {
        // enemyHouse.SetActions(() => TriggerEnterAction(enemyHouse.gameObject), () => TriggerExitAction(enemyHouse.gameObject));
        enemyHouse.Place(enemyHouse.GetXBeginningEdge(), land.GetYBeginningEdge(), enemyHouse.GetZBeginningEdge());
        playerHouse.Place(enemyHouse.GetXBeginningEdge(), land.GetYBeginningEdge(), enemyHouse.GetZBeginningEdge());
        
        PlaceableObject plant = plants[0];
        
        int rows = 5;
        int columns = 3;
        Grid grid = new Grid(new Dimension(0f, 0f, plant.GetXSize() * columns * 2, plant.GetZSize() * rows * 2), 5, 3);
        grid.TurnIntoGrid(plants, plant.GetXSize(), plant.GetZSize(), land.GetYBeginningEdge());
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
                // text.text = tagOptions.GetValueOrDefault(key, "");
                // popup.SetActive(true);
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