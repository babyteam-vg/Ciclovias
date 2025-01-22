using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public int x, y;
    private CellContent content;
    private bool buildable;
    // Safety, Charm and Flow
    private int traffic; // -Safety / -Flow
    private int danger; // -Safety
    private int greenery; // +Charm
    private int revulsion; // -Charm
    private bool nearAttraction; // +Charm
    private bool waitingPoint; // +Safey / -Flow
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
        this.danger = 0;
        this.greenery = 0;
        this.revulsion = 0;
        this.nearAttraction = false;
        this.waitingPoint = false;
        this.illuminated = false;
    }

    // Setters and Getters
    public void SetContent(CellContent content) { this.content = content; }
    public CellContent GetContent() { return content; }

    public void SetBuildable(bool free) { this.buildable = free; }
    public bool GetBuildable() { return buildable; }

    // Map Editor
    public void SetTraffic(int value) { traffic = Mathf.Clamp(value, 0, 2); }
    public int GetTraffic() { return traffic; }

    public void SetDanger(int value) { danger = Mathf.Clamp(value, 0, 2); }
    public int GetDanger() { return danger; }

    public void SetGreenery(int value) { greenery = Mathf.Clamp(value, 0, 2); }
    public int GetGreenery() { return greenery; }

    public void SetRevulsion(int value) { revulsion = Mathf.Clamp(value, 0, 2); }
    public int GetRevulsion() { return revulsion; }

    public void SetNearAttraction(bool value) { nearAttraction = value; }
    public bool GetNearAttraction() { return nearAttraction; }

    public void SetWaitingPoint(bool value) { waitingPoint = value; }
    public bool GetWaitingPoint() { return waitingPoint; }

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