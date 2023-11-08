using System;
using UnityEngine;

public class PhysicsEquation
{
    public double acceleration;
    public double velocity;
    public double initialPosition;
    public double lastTime;

    public bool isStarted = false;

    public PhysicsEquation(double vertex, double timeToVertex, double initialPosition)
    {
        this.initialPosition = initialPosition;

        // Gotten using math
        velocity = (-2.0 * initialPosition + 2.0 * vertex) / timeToVertex;
        acceleration = 2.0 * (initialPosition - vertex) / Math.Pow(timeToVertex, 2);
    }

    public double GetPosition(double time)
    {
        if (!isStarted) return 0;
        
        return acceleration * Math.Pow(time, 2) + velocity * time + initialPosition;
    }
    
    public double GetDeltaPosition(double deltaTime)
    {
        return GetPosition(lastTime + deltaTime) - GetPosition(lastTime);
    }

    public float GetCurentVelocity(double deltaTime)
    {
        if (!isStarted) return 0f;
        
        double currentTime = lastTime + deltaTime;
        double returnValue = velocity + (acceleration * currentTime);

        return (float) returnValue;
    }
    

    public void UpdateLastTime(double deltaTime)
    {
        lastTime += deltaTime;
    }

    public void Start(double initialPosition)
    {
        this.initialPosition = initialPosition;
        lastTime = 0f;
        isStarted = true;
    }

    public void Stop()
    {
        lastTime = 0f;
        isStarted = false;
    }
    
}