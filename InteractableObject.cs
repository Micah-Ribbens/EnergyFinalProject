using System;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableObject : PlaceableObject
{
    private bool hasStarted = false;
    private const float playerDistanceBeforeInteractable = 5f;
    private float distanceNeeded;
    private Action triggerEnterAction;
    private Action triggerExitAction;
    
    public void Update()
    {
        if (!hasStarted)
        {
            Instantiate();
            hasStarted = true;
        }
    }

    private void Instantiate()
    {
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        PlaceableObject player = GameObject.Find("Player").GetComponent<PlaceableObject>();
        distanceNeeded = player.GetXSize() * playerDistanceBeforeInteractable;
        
        Debug.Log(boxCollider.size);
        boxCollider.size = new Vector3(boxCollider.size.x + distanceNeeded, boxCollider.size.y + distanceNeeded, boxCollider.size.z + distanceNeeded);
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerEnterAction();
    }
    
    private void OnTriggerExit(Collider other)
    {
        triggerExitAction();
    }

    public void setActions(Action triggerEnterAction, Action triggerExitAction)
    {
        this.triggerEnterAction = triggerEnterAction;
        this.triggerExitAction = triggerExitAction;
    }
}