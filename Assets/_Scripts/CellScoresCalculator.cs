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
    public int CalculateSafety(Cell cell)
    {
        int safety = 0;

        CellContent content = cell.GetContent();
        switch (content)
        {
            case CellContent.Road:
                safety -= 8;
                break;
            case CellContent.Traffic3:
                safety -= 4;
                break;
            case CellContent.Traffic2:
                safety -= 2;
                break;
            case CellContent.Traffic1:
                safety -= 1;
                break;
            case CellContent.Dangerous:
                safety -= 4;
                break;
            default:
                safety += 1;
                break;
        }
        int illumination = cell.GetIlluminated() ? safety : safety--; // Dark: -Safety

        return safety;
    }
    public int CalculatePathSafety(List<Vector2Int> path)
    {
        int totalSafety = 0;

        for (int i = 0; i < path.Count; i++)
        {
            Cell currentCell = grid.GetCell(path[i].x, path[i].y);
            totalSafety += CalculateSafety(currentCell);
            // Stop
            //if (IsStopPoint(currentCell, i < path.Count - 1 ? grid.GetCell(path[i + 1].x, path[i + 1].y) : null))
            //    totalSafety += 1;
        }

        return totalSafety;
    }

    // Charm
    public int CalculateCharm(Cell cell)
    {
        int charm = 0;

        CellContent content = cell.GetContent();
        switch (content)
        {
            case CellContent.Green:
                charm += 1;
                break;
            case CellContent.Repulsive:
                charm -= 2;
                break;
            case CellContent.Attraction:
                charm += 3;
                break;
            default:
                break;
        }

        return charm;
    }
    public int CalculatePathCharm(List<Vector2Int> path)
    {
        int totalCharm = 0;

        foreach (var position in path)
        {
            Cell cell = grid.GetCell(position.x, position.y);
            totalCharm += CalculateCharm(cell);
        }

        return totalCharm;
    }

    // Flow
    //public float CalculateFlow(Cell cell)
    //{
    //    int crowded = cell.GetContent() == CellContent.Attraction ? -1 : 0; // Traffic: -Flow
    //    int illumination = cell.GetIlluminated() ? 0 : -1; // Dark: -Flow 

    //    return crowded + illumination;
    //}
    public float CalculatePathFlow(List<Vector2Int> path, Vector2Int destinationCell)
    {
        float totalFlow = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            Cell currentCell = grid.GetCell(path[i].x, path[i].y);
            //totalFlow += CalculateFlow(currentCell);
            // Stop
            //if (IsStopPoint(currentCell, i < path.Count - 1 ? grid.GetCell(path[i + 1].x, path[i + 1].y) : null))
            //    totalFlow -= 2; // Stop: --Flow
            // Detour
            if (IsGettingCloser(currentCell, i < path.Count - 1 ? grid.GetCell(path[i + 1].x, path[i + 1].y) : null, destinationCell))
                totalFlow += 1; // +Flow
            else totalFlow -= 1; // -Flow
        }

        return path.Count > 0 ? Mathf.Clamp(totalFlow / path.Count, 0f, 1f) : 0f;
    }

    //private bool IsStopPoint(Cell currentCell, Cell nextCell)
    //{
    //    return currentCell.GetContent() == CellContent.Stop &&
    //        nextCell.GetContent() == CellContent.Traffic &&
    //        nextCell != null;
    //}

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
