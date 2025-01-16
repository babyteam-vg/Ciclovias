using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CellScoresCalculator : MonoBehaviour
{
    [SerializeField] private Grid grid;

    [Header("Weights")]
    [Range(0f, 1f)] public float trafficWeight = 0.5f;
    [Range(0f, 1f)] public float dangerWeight = 0.5f;
    [Range(0f, 1f)] public float greeneryWeight = 0.5f;
    [Range(0f, 1f)] public float revulsionWeight = 0.5f;
    [Range(0f, 1f)] public float nearAttractionWeight = 0.5f;
    [Range(0f, 1f)] public float waitingPointWeight = 0.5f;
    [Range(0f, 1f)] public float illuminationWeight = 0.5f;

    // === Methods ===
    // Safety
    public float CalculateSafety(Cell cell)
    {
        int traffic = cell.GetTraffic(); // +Traffic / -Safety
        int danger = cell.GetDanger(); // +Danger / -Safety
        int waitingPoint = cell.GetWaitingPoint() ? 1 : 0; // +Waiting Point / +Safety
        int illuminated = cell.GetIlluminated() ? 1 : 0; // +Illuminated / +Safety

        float[] normalizedValues = new float[]
        {
            NormalizedValue(traffic, 0, 2),
            NormalizedValue(danger, 0, 2),
            NormalizedValue(waitingPoint, 0, 1),
            NormalizedValue(illuminated, 0, 1)
        };

        float[] weights = new float[]
        {
            trafficWeight,
            dangerWeight,
            waitingPointWeight,
            illuminationWeight
        };

        return MetricValue(normalizedValues, weights);
    }

    // Charm
    public float CalculateCharm(Cell cell)
    {
        int greenery = cell.GetGreenery(); // +Greenery / +Charm
        int revulsion = cell.GetRevulsion(); // +Revulsion / -Charm
        int nearAttraction = cell.GetNearAttraction() ? 1 : 0; // +Near Attraction / +Charm
        int illuminated = cell.GetIlluminated() ? 1 : 0; // +Illuminated / +Charm

        float[] normalizedValues = new float[]
        {
            NormalizedValue(greenery, 0, 2),
            NormalizedValue(revulsion, 0, 2),
            NormalizedValue(nearAttraction, 0, 1),
            NormalizedValue(illuminated, 0, 1)
        };

        float[] weights = new float[]
        {
            greeneryWeight,
            revulsionWeight,
            nearAttractionWeight,
            illuminationWeight
        };

        return MetricValue(normalizedValues, weights);
    }

    // Flow
    public float CalculateFlow(Cell cell)
    {
        int traffic = cell.GetTraffic(); // +Traffic / -Flow
        int waitingPoint = cell.GetWaitingPoint() ? 1 : 0; // +Waiting Point / -Flow

        float[] normalizedValues = new float[]
        {
            NormalizedValue(traffic, 0, 2),
            NormalizedValue(waitingPoint, 0, 1)
        };

        float[] weights = new float[]
        {
            trafficWeight,
            waitingPointWeight
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
