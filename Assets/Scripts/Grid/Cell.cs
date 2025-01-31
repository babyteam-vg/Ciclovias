using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public int x, y;
    private CellContent content;
    private bool buildable;
    private int traffic; // -Safety / -Flow
    private bool illuminated;

    // === Methods ===
    // Constructor
    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.content = CellContent.None;
        this.buildable = false;
        this.traffic = 0;
        this.illuminated = true;
    }

    // Setters and Getters
    public void SetContent(CellContent content) { this.content = content; }
    public CellContent GetContent() { return content; }

    public void SetBuildable(bool free) { this.buildable = free; }
    public bool GetBuildable() { return buildable; }

    public void SetTraffic(int value) { traffic = Mathf.Clamp(value, 0, 3); }
    public int GetTraffic() { return traffic; }

    public void SetIlluminated(bool value) { illuminated = value; }
    public bool GetIlluminated() { return illuminated; }
}

public enum CellContent
{
    None,
    Traffic,
    Sidewalk,
    Dangerous,
    Green,
    Repulsive,
    Attraction,
    Stop,
}