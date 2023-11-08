public class Dimension
{
    public float xStartEdge;
    public float zStartEdge;
    public float xSize;
    public float zSize;
    public float xEndEdge;
    public float zEndEdge;
    
    public Dimension(float xStartEdge, float zStartEdge, float xSize, float zSize)
    {
        this.xStartEdge = xStartEdge;
        this.zStartEdge = zStartEdge;
        this.xSize = xSize;
        this.zSize = zSize;
        xEndEdge = xStartEdge + xSize;
        zEndEdge = zStartEdge + zSize;
    }
    
}