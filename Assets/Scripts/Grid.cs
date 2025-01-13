using UnityEngine;
using System.Collections.Generic;
using static Cell;

public class Grid : MonoBehaviour
{
    public int width, height;
    public float cellSize;

    private Cell[,] gridArray;

    // === Methods ===
    private void Awake()
    {
        gridArray = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridArray[x, y] = new Cell(x, y);
            }
        }
    }

    // Grid (x, y) to World (i, j, k)
    public Vector3 GetWorldPositionFromCell(int x, int y) { return new Vector3(x, 0, y) * cellSize; }

    // World (i, j, k) to Grid (x, y)
    public Vector2Int? GetCellFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.z / cellSize);

        if (IsWithinBounds(x, y))
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
    public void SetCellBuildable(int x, int y, bool buildable)
    {
        if (IsWithinBounds(x, y))
            gridArray[x, y].SetBuildable(buildable);
    }
    public bool GetCellBuildable(int x, int y)
    {
        if (IsWithinBounds(x, y))
            return gridArray[x, y].GetBuildable();
        return false;
    }

    // Set and Get Safety
    public void SetCellSafety(int x, int y, float safety)
    {
        if (IsWithinBounds(x, y))
            gridArray[x, y].SetSafety(safety);
    }
    public float GetCellSafety(int x, int y)
    {
        if (IsWithinBounds(x, y))
            return gridArray[x, y].GetSafety();
        return 0.0f;
    }

    // Set and Get Charm
    public void SetCellCharm(int x, int y, float charm)
    {
        if (IsWithinBounds(x, y))
            gridArray[x, y].SetCharm(charm);
    }
    public float GetCellCharm(int x, int y)
    {
        if (IsWithinBounds(x, y))
            return gridArray[x, y].GetCharm();
        return 0.0f;
    }

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

    // Drawing the Grid (Debug)
    private void OnDrawGizmos()
    {
        if (gridArray == null) return;

        Gizmos.color = Color.gray;
        for (int x = 0; x <= width; x++) Gizmos.DrawLine(GetWorldPositionFromCell(x, 0), GetWorldPositionFromCell(x, height));
        for (int y = 0; y <= height; y++) Gizmos.DrawLine(GetWorldPositionFromCell(0, y), GetWorldPositionFromCell(width, y));

        if (Application.isPlaying)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CellContent content = gridArray[x, y].GetContent();
                    string initial = content.ToString().Substring(0, 1).ToUpper();

                    Vector3 position = GetWorldPositionFromCell(x, y) + new Vector3(cellSize, 0, cellSize) * 0.5f;
                    DrawTextGizmo(initial, position, Color.white);
                }
            }
        }
    }

    private void DrawTextGizmo(string text, Vector3 position, Color color)
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = color;
        style.alignment = TextAnchor.MiddleCenter;

        UnityEditor.Handles.Label(position, text, style);
    }
}