using System;
using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    private float xSize;
    private float ySize;
    private float zSize;

    private void Start()
    {
        Renderer meshRenderer = GetComponent<MeshRenderer>();
        xSize = meshRenderer.bounds.size.x;
        ySize = meshRenderer.bounds.size.y;
        zSize = meshRenderer.bounds.size.z;
    }

    public void Place(float x, float y, float z)
    {
        
        float xPosition = x + xSize / 2;
        float yPosition = y + ySize / 2;
        float zPosition = z + zSize / 2;
        
        transform.position = new Vector3(xPosition, yPosition, zPosition);
    }
    
    // Constants
    public float GetX() { return transform.position.x; }
    public float GetY() { return transform.position.y; }
    public float GetZ() { return transform.position.z; }
    
    public float GetYSize() { return ySize; }
    public float GetXSize() { return xSize; }
    public float GetZSize() { return zSize; }
    
    // Calculated
    public float GetXEndEdge() { return GetX() - xSize / 2; }
    public float GetYEndEdge() { return GetY() - ySize / 2; }
    public float GetZEndEdge() { return GetZ() - zSize / 2; }
    
    public float GetXBeginningEdge() { return GetX() + xSize / 2; }
    public float GetYBeginningEdge() { return GetY() + ySize / 2; }
    public float GetZBeginningEdge() { return GetZ() + zSize / 2; }
}