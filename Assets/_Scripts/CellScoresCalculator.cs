using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellScoresCalculator
{
    [SerializeField] private Grid grid;

    // :::::::::: PUBLIC METHODS ::::::::::
    public CellScoresCalculator(Grid grid) { this.grid = grid; }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Safety
    public int CalculateSafety(Cell cell)
    {
        int safety = 0;

        CellContent content = cell.GetContent();
        switch (content)
        {
            case CellContent.Road:
                safety -= 5;
                break;
            case CellContent.Traffic:
                safety -= 3;
                break;
            case CellContent.Zebra:
                safety -= 1;
                break;
            case CellContent.Dangerous:
                safety -= 3;
                break;
            default:
                safety += 1;
                break;
        }
        int illumination = cell.GetIlluminated() ? safety : safety--; // Dark: -Safety

        return safety;
    }
    public float CalculatePathSafety(List<Vector2Int> path)
    {
        if (path.Count == 0) return 0;

        int totalSafety = 0;
        for (int i = 0; i < path.Count; i++)
        {
            Cell currentCell = grid.GetCell(path[i].x, path[i].y);
            totalSafety += CalculateSafety(currentCell);
        }

        float normalizedSafety = (float)(totalSafety) / (path.Count);
        return normalizedSafety;
    }

    // ::::: Charm
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
    public float CalculatePathCharm(List<Vector2Int> path)
    {
        if (path.Count == 0) return 0;

        int totalCharm = 0;
        foreach (var position in path)
        {
            Cell cell = grid.GetCell(position.x, position.y);
            totalCharm += CalculateCharm(cell);
        }

        float normalizedCharm = (float)(totalCharm) / (path.Count);
        return normalizedCharm;
    }

    // ::::: Flow
    public float CalculateFlow(Cell cell)
    {
        int flow = 0;

        CellContent content = cell.GetContent();
        switch (content)
        {
            case CellContent.Crossing:
                flow -= 2;
                break;
            case CellContent.Attraction:
                flow -= 2;
                break;
            default:
                break;
        }

        return flow;
    }
    public float CalculatePathFlow(List<Vector2Int> path, Vector2Int destinationPos)
    {
        if (path.Count == 0) return 0;

        float totalFlow = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            Cell previousCell = i > 0 ? grid.GetCell(path[i - 1].x, path[i - 1].y) : null;
            Cell currentCell = grid.GetCell(path[i].x, path[i].y);
            totalFlow += CalculateFlow(currentCell);

            // Detour
            if (IsGettingCloser(previousCell, currentCell, destinationPos))
                totalFlow += 1; // +Flow
            else totalFlow -= 1; // -Flow
        }

        return totalFlow / path.Count;
    }

    private bool IsGettingCloser(Cell previousCell, Cell currentCell, Vector2Int destinationPos)
    {
        if (previousCell == null || currentCell == null || destinationPos == null)
            return false;

        float previousDistanceSq = Mathf.Pow(previousCell.x - destinationPos.x, 2) +
                                   Mathf.Pow(previousCell.y - destinationPos.y, 2);

        float currentDistanceSq = Mathf.Pow(currentCell.x - destinationPos.x, 2) +
                                  Mathf.Pow(currentCell.y - destinationPos.y, 2);

        Vector2Int previousToCurrent = new Vector2Int(
            currentCell.x - previousCell.x,
            currentCell.y - previousCell.y);
        Vector2Int currentToDestination = new Vector2Int(
            destinationPos.x - currentCell.x,
            destinationPos.y - currentCell.y);
        float angle = Vector2.Angle(previousToCurrent, currentToDestination);

        if (angle >= 90f)
            return false;

        if (currentDistanceSq < previousDistanceSq)
            return true;

        return false;
    }
}
