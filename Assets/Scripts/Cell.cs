using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public int x, y;
    private CellContent content;
    private bool buildable;
    // Safety, Charm and Flow
    private int traffic; // -Safety / -Flow
    private int greenery; // +Charm
    private bool nearAttraction; // +Charm
    private bool steep; // -Flow
    private bool illuminated; // +Safety / +Charm

    // === Methods ===
    // Constructor
    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.content = CellContent.None;
        this.buildable = false;
        this.traffic = 0;
        this.greenery = 0;
        this.nearAttraction = false;
        this.steep = false;
        this.illuminated = false;
    }

    // Setters and Getters
    public void SetContent(CellContent content) { this.content = content; }
    public CellContent GetContent() { return content; }

    public void SetBuildable(bool free) { this.buildable = free; }
    public bool GetBuildable() { return buildable; }

    // Map Editor
    public void SetTraffic(int value) { traffic = Mathf.Clamp(value, 0, 3); }
    public int GetTraffic() { return traffic; }

    public void SetGreenery(int value) { greenery = Mathf.Clamp(value, 0, 3); }
    public int GetGreenery() { return greenery; }

    public void SetNearAttraction(bool value) { nearAttraction = value; }
    public bool GetNearAttraction() { return nearAttraction; }

    public void SetSteep(bool value) { steep = value; }
    public bool GetSteep() { return steep; }

    public void SetIlluminated(bool value) { illuminated = value; }
    public bool GetIlluminated() { return illuminated; }
}

public enum CellContent
{
    None,
    Road,
    Sidewalk,
    Compound,
    Attraction
}