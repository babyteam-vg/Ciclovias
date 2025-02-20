using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public int x, y;
    private CellContent content;
    private bool buildable;
    private bool illuminated;

    // === Methods ===
    // Constructor
    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.content = CellContent.None;
        this.buildable = false;
        this.illuminated = true;
    }

    // Setters and Getters
    public void SetContent(CellContent content) { this.content = content; }
    public CellContent GetContent() { return content; }

    public void SetBuildable(bool free) { this.buildable = free; }
    public bool GetBuildable() { return buildable; }

    public void SetIlluminated(bool value) { illuminated = value; }
    public bool GetIlluminated() { return illuminated; }
}

public enum CellContent
{
    None,
    Sidewalk,
    Road,
    Traffic3,
    Traffic2,
    Traffic1,
    Dangerous,
    Green,
    Repulsive,
    Attraction,
}