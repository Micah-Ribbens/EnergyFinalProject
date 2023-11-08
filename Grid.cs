using System;

public class Grid
{
    private int rows;
    private int columns;

    private Dimension dimension;
    private float xBuffer = .1f;
    private float zBuffer = .1f;
    private bool goesTopToBottom = true;
    private bool goesLeftToRight = true;

    public Grid(Dimension dimension, int rows, int columns)
    {
        this.dimension = dimension;
        this.rows = rows;
        this.columns = columns;
    }

    public Grid(Dimension dimension, int rows, int columns, bool goesTopToBottom, bool goesLeftToRight)
    : this(dimension, rows, columns)
    {
        this.goesTopToBottom = goesTopToBottom;
        this.goesLeftToRight = goesLeftToRight;
    }

    public void TurnIntoGrid(PlaceableObject[] items, float itemMaxXSize, float itemMaxZSize, float placementY)
    {
        float tempRows = rows;
        float tempColumns = columns;
        int numberOfItems = items.Length;

        if (tempRows == -1)
        {
            tempRows = GetGridDimension(tempColumns, numberOfItems);
        }

        if (tempColumns == -1)
        {
            tempColumns = GetGridDimension(tempRows, numberOfItems);
        }

        float itemHeight = GetItemDimension(dimension.zSize, tempRows, itemMaxZSize, zBuffer);
        float itemWidth = GetItemDimension(dimension.xSize, tempColumns, itemMaxXSize, xBuffer);
        
        float baseLeftEdge = goesLeftToRight ? dimension.xStartEdge : dimension.xEndEdge;
        float baseTopEdge = goesTopToBottom ? dimension.xStartEdge : dimension.xEndEdge - itemWidth;

        for (int i = 0; i < numberOfItems; i++)
        {
            int columnNumber = i % (int) tempColumns;
            int rowNumber = (int) Math.Floor( (i / tempColumns));

            float xStartEdge = baseLeftEdge + GetDimensionChange(columnNumber, itemWidth, xBuffer);
            float zStartEdge = baseTopEdge + GetDimensionChange(rowNumber, itemHeight, zBuffer);
            items[i].Place(xStartEdge, placementY, zStartEdge);
        }
    }

    public int GetGridDimension(float otherDimension, int numberOfItems)
    {
        return (int) Math.Ceiling(otherDimension / numberOfItems);
    }

    public float GetItemDimension(float gridDimensionSize, float gridDimension, float itemDimensionMax,
        float bufferBetweenItems)
    {
        float remainingDimension = gridDimensionSize - bufferBetweenItems * (gridDimension - 1);

        float itemDimension = remainingDimension / gridDimension;

        if (itemDimensionMax != -1 && itemDimension > itemDimensionMax)
        {
            itemDimension = itemDimensionMax;

        }
        return itemDimension;
    }

    public float GetDimensionChange(float gridDimension, float itemDimension, float bufferBetweenItems)
    {
        float dimensionChangeAmount = gridDimension * (itemDimension + bufferBetweenItems);
        return goesTopToBottom ? dimensionChangeAmount : -dimensionChangeAmount;
    }
    


}