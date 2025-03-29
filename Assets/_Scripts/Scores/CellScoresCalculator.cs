using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellScoresCalculator
{
    private Grid grid;
    private Graph graph;

    // :::::::::: PUBLIC METHODS ::::::::::
    public CellScoresCalculator(Grid grid, Graph graph)
    {
        this.grid = grid;
        this.graph = graph;
    }

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
                charm -= 3;
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
                flow += 1;
                break;
        }

        return flow;
    }
    public float CalculatePathFlow(List<Vector2Int> path)
    {
        if (path.Count == 0) return 0;

        float totalFlow = 0f;

        for (int i = 0; i < path.Count; i++)
        {
            Cell currentCell = grid.GetCell(path[i].x, path[i].y);
            totalFlow += CalculateFlow(currentCell);

            if (graph.GetNeighborsCount(path[i]) > 2)
                totalFlow -= 3;

            if (i > 1)
            {
                Vector2Int prevDir = path[i - 1] - path[i - 2];
                Vector2Int currentDir = path[i] - path[i - 1];

                float angle = Vector2.Angle(prevDir, currentDir);
                if (angle > 90)
                    totalFlow -= 3;
            }
        }

        return totalFlow / path.Count;
    }
}
