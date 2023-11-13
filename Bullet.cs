using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float timeToDelete = 0;
    private float timeBeforeDelete = Constants.TIME_BEFORE_BULLET_DESPAWNS;
    private Vector3 movementVector;
    private float speed = Constants.BULLET_SPEED;
    private Action onTriggerEnterAction;
    
    private void Start()
    {
        timeToDelete = Time.time + timeBeforeDelete;

    }

    private void Update()
    {
        transform.Translate(movementVector * Time.deltaTime * speed, Space.World);
        if (timeToDelete <= Time.time)
        {
            Destroy(gameObject);
        }
    }

    public void SetMovementVector(Vector3 vector)
    {
        movementVector = vector;

    }

    public void SetOnTriggerEnterAction(Action action)
    {
        onTriggerEnterAction = action;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            onTriggerEnterAction();
        }
    }
}