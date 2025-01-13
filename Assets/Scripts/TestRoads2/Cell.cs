using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public int x, y;
    private CellContent content;
    private bool buildable;
    private float safety, charm;

    // === Methods ===
    // Constructor
    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.content = CellContent.None;
        this.buildable = false;
        this.safety = 0.0f;
        this.charm = 0.0f;
    }

    // Setters and Getters
    public void SetContent(CellContent content) { this.content = content; }
    public CellContent GetContent() { return content; }

    public void SetBuildable(bool free) { this.buildable = free; }
    public bool GetBuildable() { return buildable; }

    public void SetSafety(float value) { safety = Mathf.Clamp(value, 0.0f, 1.0f); }
    public float GetSafety() { return safety; }

    public void SetCharm(float value) { charm = Mathf.Clamp(value, 0.0f, 1.0f); }
    public float GetCharm() { return charm; }
}

public enum CellContent
{
    None,
    Road,
    Sidewalk,
    Compound,
    Attraction
}