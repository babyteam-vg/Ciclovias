using UnityEngine;
using System.Collections.Generic;

public class GridConstructor : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private List<CustomCells> customCells;

    // === Methods ===
    void Start()
    {
        var gridModifier = FindObjectOfType<GridConstructor>();
        gridModifier.ApplyCustomCells();
    }

    // Apply Changes to the Grid
    public void ApplyCustomCells()
    {
        if (grid == null || customCells == null) return;

        foreach (var customCell in customCells)
        {
            if (grid.IsWithinBounds(customCell.x, customCell.y))
            {
                grid.SetCellContent(customCell.x, customCell.y, customCell.content);
                grid.SetCellBuildable(customCell.x, customCell.y, customCell.buildable);
                grid.SetCellTraffic(customCell.x, customCell.y, customCell.traffic);
                grid.SetCellGreenery(customCell.x, customCell.y, customCell.greenery);
                grid.SetCellNearAttraction(customCell.x, customCell.y, customCell.nearAttraction);
                grid.SetCellSteep(customCell.x, customCell.y, customCell.steep);
                grid.SetCellIlluminated(customCell.x, customCell.y, customCell.illuminated);
            }
        }
    }

    public void ClearSidewalks() { foreach (Transform child in grid.transform) Destroy(child.gameObject); }

    public void ResetGrid()
    {
        ClearSidewalks();
        ApplyCustomCells();
    }
}

[System.Serializable]
public class CustomCells
{
    public int x;
    public int y;
    public CellContent content;
    public bool buildable;
    public int traffic;
    public int greenery;
    public bool nearAttraction;
    public bool steep;
    public bool illuminated;
}
