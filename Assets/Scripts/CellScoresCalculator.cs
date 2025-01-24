using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellScoresCalculator
{
    [SerializeField] private Grid grid;

    [Header("Weights")]
    [Range(0f, 1f)] public float trafficWeight = 0.5f;
    [Range(0f, 1f)] public float dangerWeight = 0.5f;
    [Range(0f, 1f)] public float greeneryWeight = 0.5f;
    [Range(0f, 1f)] public float revulsionWeight = 0.5f;

    // === Methods ===
    public CellScoresCalculator(Grid grid) { this.grid = grid; }

    // Safety
    public float CalculateSafety(Cell cell)
    {
        int traffic = cell.GetTraffic(); // Traffic: -Safety
        int danger = cell.GetContent() == CellContent.Dangerous ? 1 : 0;

        float[] normalizedValues = new float[]
        {
            NormalizedValue(traffic, 0, 3),
            NormalizedValue(danger, 0, 1),
        };

        float[] weights = new float[]
        {
            trafficWeight,
            dangerWeight,
        };

        return MetricValue(normalizedValues, weights);
    }
    public float CalculatePathSafety(List<Vector2Int> path)
    {
        float totalSafety = 0f;

        foreach (var position in path)
        {
            Cell cell = grid.GetCell(position.x, position.y);
            if (cell != null)
                totalSafety += CalculateSafety(cell);
        }

        return path.Count > 0 ? totalSafety / path.Count : 0f;
    }

    // Charm
    public float CalculateCharm(Cell cell)
    {
        int greenery = cell.GetContent() == CellContent.Green || cell.GetContent() == CellContent.Attraction ? 1: 0; // Green or Attraction: +Charm
        int revulsion = cell.GetContent() == CellContent.Revulsive ? 1 : 0; // Revulsive: -Charm

        float[] normalizedValues = new float[]
        {
            NormalizedValue(greenery, 0, 1),
            NormalizedValue(revulsion, 0, 1),
        };

        float[] weights = new float[]
        {
            greeneryWeight,
            revulsionWeight,
        };

        return MetricValue(normalizedValues, weights);
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

        return path.Count > 0 ? totalCharm / path.Count : 0f;
    }

    // Flow
    public float CalculateFlow(Cell cell)
    {
        int traffic = cell.GetTraffic(); // Traffic: -Flow

        float[] normalizedValues = new float[]
        {
            NormalizedValue(traffic, 0, 3),
        };

        float[] weights = new float[]
        {
            trafficWeight,
        };

        return MetricValue(normalizedValues, weights);
    }
    public float CalculatePathFlow(List<Vector2Int> path)
    {
        float totalFlow = 0f;

        foreach (var position in path)
        {
            Cell cell = grid.GetCell(position.x, position.y);
            if (cell != null)
                totalFlow += CalculateFlow(cell);
        }

        return path.Count > 0 ? totalFlow / path.Count : 0f;
    }

    // [0, 3] -> [0.0, 1.0]
    public static float NormalizedValue(int value, int minValue, int maxValue)
    {
        if (maxValue == minValue)
            return 0f;
        return Mathf.Clamp((float)(value - minValue) / (maxValue - minValue), 0f, 1f);
    }

    // Final Result [0.0, 1.0]
    public static float MetricValue(float[] normalizedValues, float[] weights)
    {
        float weightedSum = 0f;
        float weightTotal = 0f;

        for (int i = 0; i < normalizedValues.Length; i++)
        {
            weightedSum += normalizedValues[i] * weights[i];
            weightTotal += Mathf.Abs(weights[i]);
        }

        return weightTotal > 0f ? Mathf.Clamp01(weightedSum / weightTotal) : 0f;
    }
}
