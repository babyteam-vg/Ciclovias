using UnityEngine;
using System.Collections.Generic;
using static Cell;

public class Grid : MonoBehaviour
{
    private Cell[,] gridArray;
    private int width, height;
    private float cellSize = 1f;

    // === Methods ===
    public void SetGridArray(Cell[,] cells)
    {
        this.gridArray = cells;
        this.width = cells.GetLength(0);
        this.height = cells.GetLength(1);
    }

    public Vector2Int GetGridDimensions() { return new Vector2Int(width, height); }
    public float GetCellSize() { return cellSize; }

    // Get a Cell
    public Cell GetCell(int x, int y) { return gridArray[x, y]; }

    // Grid (x, y) to World (i, j, k)
    public Vector3 GetWorldPositionFromCell(int x, int y) { return new Vector3(x, 0, y) * cellSize; }

    // World (i, j, k) to Grid (x, y)
    public Vector2Int? GetCellFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.z / cellSize);

        if (IsWithinBounds(x, y) && GetCell(x, y) != null)
            return new Vector2Int(x, y);
        return null;
    }

    // Set and Get Content
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
    
    // Set and Get Buildable
    public void SetCellBuildable(int x, int y, bool buildable) { gridArray[x, y].SetBuildable(buildable); }
    public bool GetCellBuildable(int x, int y) { return gridArray[x, y].GetBuildable(); }

    // Set and Get Traffic
    public void SetCellTraffic(int x, int y, int traffic) { gridArray[x, y].SetTraffic(traffic); }
    public int GetCellTraffic(int x, int y) { return gridArray[x, y].GetTraffic(); }

    // Set and Get Greenery
    public void SetCellGreenery(int x, int y, int greenery) { gridArray[x, y].SetGreenery(greenery); }
    public int GetCellGreenery(int x, int y) { return gridArray[x, y].GetGreenery(); }

    // Set and Get Near Attraction
    public void SetCellNearAttraction(int x, int y, bool nearAttraction) { gridArray[x, y].SetNearAttraction(nearAttraction); }
    public bool GetCellNearAttraction(int x, int y) { return gridArray[x, y].GetNearAttraction(); }

    // Set and Get Waiting Point
    public void SetCellWaitingPoint(int x, int y, bool waitingPoint) { gridArray[x, y].SetWaitingPoint(waitingPoint); }
    public bool GetCellWaitingPoint(int x, int y) { return gridArray[x, y].GetWaitingPoint(); }

    // Is Cell Not Off Limits?
    public bool IsWithinBounds(int x, int y) { return x >= 0 && x < width && y >= 0 && y < height; }

    // Get Adjacent Cells to [x][y]
    public List<Cell> GetAdjacentCells(int x, int y)
    {
        List<Cell> adjacentCells = new List<Cell>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            { //      Skip Central Cell <¬
                if (dx == 0 && dy == 0) continue;

                int newX = x + dx;
                int newY = y + dy;

                if (IsWithinBounds(newX, newY))
                    adjacentCells.Add(gridArray[newX, newY]);
            }
        }
        return adjacentCells;
    }

    // Edge to Center of a Cell
    public Vector2 EdgeToMid(Vector2 edgePosition)
    {
        float centerX = edgePosition.x + cellSize / 2;
        float centerY = edgePosition.y + cellSize / 2;
        return new Vector2(centerX, centerY);
    }
}