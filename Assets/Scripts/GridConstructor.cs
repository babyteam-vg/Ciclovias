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

        foreach (var cell in customCells)
        {
            if (grid.IsWithinBounds(cell.x, cell.y))
            {
                grid.SetCellContent(cell.x, cell.y, cell.content);

                if (cell.content == CellContent.Sidewalk)
                    grid.SetCellBuildable(cell.x, cell.y, true);

                if (cell.content == CellContent.Road)
                    grid.SetCellBuildable(cell.x, cell.y, true);
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
    public float safety;
    public float charm;
}
