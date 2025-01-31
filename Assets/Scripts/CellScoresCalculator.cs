using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellScoresCalculator
{
    [SerializeField] private Grid grid;

    // === Methods ===
    public CellScoresCalculator(Grid grid) { this.grid = grid; }

    // Safety
    public float CalculateSafety(Cell cell)
    {
        int traffic = cell.GetTraffic(); // Traffic: -Safety
        int danger = cell.GetContent() == CellContent.Dangerous ? -2 : 0; // Dangerous: --Safety
        int illumination = cell.GetIlluminated() ? 0 : -1; // Dark: -Safety 

        return -traffic + danger + illumination;
    }
    public float CalculatePathSafety(List<Vector2Int> path)
    {
        float totalSafety = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            Cell currentCell = grid.GetCell(path[i].x, path[i].y);
            totalSafety += CalculateSafety(currentCell);
            // Stop
            if (currentCell != null) //                         Next Cell <¬
                if (IsStopPoint(currentCell, i < path.Count - 1 ? grid.GetCell(path[i + 1].x, path[i + 1].y) : null))
                    totalSafety += 1;
        }

        return totalSafety;
    }

    // Charm
    public float CalculateCharm(Cell cell)
    {
        int greenery = cell.GetContent() == CellContent.Green ? 1 : 0; // Green: +Charm
        int repulsion = cell.GetContent() == CellContent.Repulsive ? -1 : 0; // Repulsive: -Charm
        int attractiveness = cell.GetContent() == CellContent.Attraction ? 2 : 0; // Attraction: ++Charm

        return greenery + repulsion + attractiveness;
    }
    public float CalculatePathCharm(List<Vector2Int> path)
    {
        float totalCharm = 0f;

        foreach (var position in path)
        {
            Cell cell = grid.GetCell(position.x, position.y);
            if (cell != null)
                totalCharm += CalculateCharm(cell);
        }

        return totalCharm;
    }

    // Flow
    public float CalculateFlow(Cell cell)
    {
        int crowded = cell.GetContent() == CellContent.Attraction ? -1 : 0; // Traffic: -Flow
        // Detour Logic
        int illumination = cell.GetIlluminated() ? 0 : -1; // Dark: -Flow 

        return crowded + illumination;
    }
    public float CalculatePathFlow(List<Vector2Int> path, Vector2Int destinationCell)
    {
        float totalFlow = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            Cell currentCell = grid.GetCell(path[i].x, path[i].y);
            if (currentCell != null)
                totalFlow += CalculateFlow(currentCell);

            if (currentCell != null)
            { //                                                Next Cell <¬
                if (IsStopPoint(currentCell, i < path.Count - 1 ? grid.GetCell(path[i + 1].x, path[i + 1].y) : null))
                    totalFlow -= 2;

                if (IsGettingCloser(currentCell, i < path.Count - 1 ? grid.GetCell(path[i + 1].x, path[i + 1].y) : null, destinationCell))
                    totalFlow += 1;
                else totalFlow -= 1;
            }

        }

        return path.Count > 0 ? totalFlow / path.Count : 0f;
    }

    private bool IsStopPoint(Cell currentCell, Cell nextCell)
    {
        return currentCell.GetContent() == CellContent.Stop &&
            nextCell.GetContent() == CellContent.Traffic &&
            nextCell != null;
    }

    private bool IsGettingCloser(Cell currentCell, Cell nextCell, Vector2Int destinationCell)
    {
        if (currentCell == null || nextCell == null || destinationCell == null)
            return false;

        float currentDistanceSq = Mathf.Pow(currentCell.x - destinationCell.x, 2) +
                                  Mathf.Pow(currentCell.y - destinationCell.y, 2);

        float nextDistanceSq = Mathf.Pow(nextCell.x - destinationCell.x, 2) +
                               Mathf.Pow(nextCell.y - destinationCell.y, 2);

        return nextDistanceSq < currentDistanceSq;
    }


}
