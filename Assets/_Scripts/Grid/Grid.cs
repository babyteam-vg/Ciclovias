using UnityEngine;
using System.Collections.Generic;
using static Cell;

public class Grid : MonoBehaviour
{
    private Cell[,] gridArray;
    private int width, height;
    private float cellSize = 1f;

    // :::::::::: PUBLIC METHODS ::::::::::
    public void SetGridArray(Cell[,] cells)
    {
        this.gridArray = cells;
        this.width = cells.GetLength(0);
        this.height = cells.GetLength(1);
    }

    public Vector2Int GetGridDimensions() { return new Vector2Int(width, height); }
    public float GetCellSize() { return cellSize; }

    // ::::: Get a Cell
    public Cell GetCell(int x, int y) { return gridArray[x, y]; }

    // ::::: Grid (x, y) to World (i, j, k)
    public Vector3 GetWorldPositionFromCell(int x, int y) { return new Vector3(x, 0, y) * cellSize; }
    public Vector3 GetWorldPositionFromCellCentered(float x, float y) { return new Vector3((x + 0.5f) * cellSize, 0, (y + 0.5f) * cellSize); }


    // ::::: World (i, j, k) to Grid (x, y)
    public Vector2Int? GetCellFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.z / cellSize);

        if (IsWithinBounds(x, y) && GetCell(x, y) != null)
            return new Vector2Int(x, y);
        return null;
    }

    // ::::: Set and Get Content
    public void SetCellContent(int x, int y, CellContent content)
    {
        if (IsWithinBounds(x, y))
            gridArray[x, y].SetContent(content);
    }
    public CellContent GetCellContent(int x, int y)
    {
        if (IsWithinBounds(x, y))
            return gridArray[x, y].GetContent();
        return CellContent.None;
    }

    // ::::: Is [x][y] Not Off Limits?
    public bool IsWithinBounds(int x, int y) { return x >= 0 && x < width && y >= 0 && y < height; }

    // ::::: Get Adjacent Cells to [x][y]
    public List<Cell> GetAdjacentCells(int x, int y)
    {
        List<Cell> adjacentCells = new List<Cell>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            { 
                if (dx == 0 && dy == 0) continue; // Omitir la celda central

                int newX = x + dx;
                int newY = y + dy;

                if (IsWithinBounds(newX, newY) && gridArray[newX, newY] != null) // Evitar agregar celdas nulas
                    adjacentCells.Add(gridArray[newX, newY]);
            }
        }
        return adjacentCells;
    }

    // ::::: Check Adjacency in 8 Directions
    public bool IsAdjacent(Vector2Int current, Vector2Int target)
    {
        int dx = Mathf.Abs(current.x - target.x);
        int dy = Mathf.Abs(current.y - target.y);
        return (dx <= 1 && dy <= 1);
    }

    // ::::: Edge to Center of a Cell
    public Vector2 EdgeToMid(Vector2Int edgePosition)
    {
        float centerX = edgePosition.x + cellSize / 2;
        float centerY = edgePosition.y + cellSize / 2;
        return new Vector2(centerX, centerY);
    }
}