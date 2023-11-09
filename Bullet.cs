using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float timeToDelete = 0;
    private float timeBeforeDelete = 5f;
    private Vector3 movementVector;
    private float speed = 20f;
    
    private void Start()
    {
        timeToDelete = Time.time + timeBeforeDelete;

    }

    private void Update()
    {
        transform.Translate(movementVector * Time.deltaTime * speed);
        if (timeToDelete <= Time.time)
        {
            Destroy(gameObject);
        }
    }

    public void SetMovementVector(Vector3 vector)
    {
        movementVector = vector;

    }
}