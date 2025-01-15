using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellScoresCalculator : MonoBehaviour
{
    [SerializeField] private Grid grid;

    [Header("Weights")]
    [Range(0f, 1f)] public float trafficWeight = 0.5f;
    [Range(0f, 1f)] public float greeneryWeight = 0.5f;
    [Range(0f, 1f)] public float nearAttractionWeight = 0.5f;
    [Range(0f, 1f)] public float steepWeight = 0.5f;
    [Range(0f, 1f)] public float illuminationWeight = 0.5f;

    // === Methods ===
    // Safety
    public float CalculateSafety(Cell cell)
    {
        int traffic = cell.GetTraffic(); // +Traffic, -Safety
        int illuminated = cell.GetIlluminated() ? 1 : 0; // +Illuminated, +Safety

        float[] normalizedValues = new float[]
        {
            NormalizedValue(traffic, 0, 3),
            NormalizedValue(illuminated, 0, 1)
        };

        float[] weights = new float[]
        {
            trafficWeight,
            illuminationWeight
        };

        return MetricValue(normalizedValues, weights);
    }

    // Charm
    public float CalculateCharm(Cell cell)
    {
        int greenery = cell.GetGreenery(); // +Greenery, +Charm
        int nearAttraction = cell.GetNearAttraction() ? 1 : 0; // +Attraction, +Charm
        int illuminated = cell.GetIlluminated() ? 1 : 0; // +Illuminated, +Charm

        float[] normalizedValues = new float[]
        {
            NormalizedValue(greenery, 0, 3),
            NormalizedValue(nearAttraction, 0, 1),
            NormalizedValue(illuminated, 0, 1)
        };

        float[] weights = new float[]
        {
            greeneryWeight,
            nearAttractionWeight,
            illuminationWeight
        };

        return MetricValue(normalizedValues, weights);
    }

    // Flow
    public float CalculateFlow(Cell cell)
    {
        int traffic = cell.GetTraffic(); // +Traffic, -Flow
        int steep = cell.GetSteep() ? 1 : 0; // Steep, -Flow

        float[] normalizedValues = new float[]
        {
            NormalizedValue(traffic, 0, 3),
            NormalizedValue(steep, 0, 1)
        };

        float[] weights = new float[]
        {
            trafficWeight,
            steepWeight
        };

        return MetricValue(normalizedValues, weights);
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
