using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public int x, y;
    private CellContent content;
    private bool buildable;
    private int traffic; // -Safety / -Flow

    // === Methods ===
    // Constructor
    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.content = CellContent.None;
        this.buildable = false;
        this.traffic = 0;
    }

    // Setters and Getters
    public void SetContent(CellContent content) { this.content = content; }
    public CellContent GetContent() { return content; }

    public void SetBuildable(bool free) { this.buildable = free; }
    public bool GetBuildable() { return buildable; }

    // Map Editor
    public void SetTraffic(int value) { traffic = Mathf.Clamp(value, 0, 2); }
    public int GetTraffic() { return traffic; }
}

public enum CellContent
{
    None,
    Traffic3,
    Traffic2,
    Traffic1,
    Sidewalk,
    Dangerous,
    Green,
    Revulsive,
    Attraction,
}