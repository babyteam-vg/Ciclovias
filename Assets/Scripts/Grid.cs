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

    // Get a Cell
    public Cell GetCell(int x, int y) { return gridArray[x, y]; }

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

    // Set and Get Illuminated
    public void SetCellIlluminated(int x, int y, bool illuminated) { gridArray[x, y].SetIlluminated(illuminated); }
    public bool GetCellIlluminated(int x, int y) { return gridArray[x, y].GetIlluminated(); }

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

        CellScoresCalculator cellScoresCalculator = FindObjectOfType<CellScoresCalculator>();

        if (Application.isPlaying && cellScoresCalculator!= null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 position = GetWorldPositionFromCell(x, y) + new Vector3(cellSize, 0, cellSize) * 0.5f;
                    Cell cell = gridArray[x, y];

                    string initial = cell.GetContent().ToString().Substring(0, 1).ToUpper();
                    string buildable = cell.GetBuildable() ? "(B)" : "";

                    float safety = cellScoresCalculator.CalculateSafety(cell);
                    float charm = cellScoresCalculator.CalculateCharm(cell);
                    float flow = cellScoresCalculator.CalculateFlow(cell);

                    //string displayText = $"{initial} {buildable}\nS: {safety:F2}\nC: {charm:F2}\nF: {flow:F2}";
                    string displayText = $"{initial}\n{x},{y}";
                    DrawTextGizmo(displayText, position, Color.white);
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