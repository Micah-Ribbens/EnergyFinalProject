using System;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    private PlaceableObject enemyHouse;
    private PlaceableObject enemyHouseWithCameras;
    private PlaceableObject playerHouse;
    private GameObject playerHouseParent;
    private PlaceableObject land;
    private bool hasCalledInitialization = false;
    
    private void Start()
    {
        enemyHouse = GameObject.Find("EnemyHouse").GetComponent<PlaceableObject>();
        enemyHouseWithCameras = GameObject.Find("EnemyHouseWithCameras").GetComponent<PlaceableObject>();
        playerHouse = GameObject.Find("PlayerHouse").GetComponent<PlaceableObject>();
        playerHouseParent = GameObject.Find("PlayerHouseParent");
        land = GameObject.Find("Land").GetComponent<PlaceableObject>();
    }

    private void Update()
    {
        if (!hasCalledInitialization)
        {
            Initialize();
            hasCalledInitialization = true;
        }
        
    }

    private void Initialize()
    {
        enemyHouse.Place(enemyHouse.GetXBeginningEdge(), land.GetYBeginningEdge(), enemyHouse.GetZBeginningEdge());
        enemyHouseWithCameras.Place(enemyHouseWithCameras.GetXBeginningEdge(), land.GetYBeginningEdge(), enemyHouseWithCameras.GetZBeginningEdge());
        playerHouse.Place(enemyHouse.GetXBeginningEdge(), land.GetYBeginningEdge(), enemyHouse.GetZBeginningEdge());
    }
}