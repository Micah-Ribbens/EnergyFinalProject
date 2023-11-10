using System;
using Unity.VisualScripting;
using UnityEngine;

public class InteractableObject : PlaceableObject
{
    private bool hasStarted = false;
    private const float playerDistanceBeforeInteractable = 20f;
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
        // BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        // boxCollider.isTrigger = true;
        // PlaceableObject player = GameObject.Find("Player").GetComponent<PlaceableObject>();
        // distanceNeeded = playerDistanceBeforeInteractable * player.GetXSize();
        
        // TODO fixing the box collider code. How do I set the center and make it work correctly?
        // boxCollider.size = new Vector3(boxCollider.size.x + distanceNeeded, boxCollider.size.y + distanceNeeded, boxCollider.size.z + distanceNeeded);
        // Vector3 position = boxCollider.transform.position;
        // boxCollider.transform.position = new Vector3(position.x + distanceNeeded / 2, position.y + distanceNeeded / 2, position.z + distanceNeeded / 2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerEnterAction != null) triggerEnterAction();
        
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (triggerEnterAction != null) triggerExitAction();;
    }

    public void SetActions(Action triggerEnterAction, Action triggerExitAction)
    {
        this.triggerEnterAction = triggerEnterAction;
        this.triggerExitAction = triggerExitAction;
    }
}