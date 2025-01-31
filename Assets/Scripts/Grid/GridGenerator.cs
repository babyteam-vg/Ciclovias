using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Texture2D map;
    [SerializeField] private ColorToCell[] colorMappings;

    private Dictionary<Color, ColorToCell> colorMappingDict;

    // === Methods ===
    private void Awake() { GenerateGrid(); }

    private void GenerateGrid()
    {
        InitializeColorMapping();

        int width = map.width;
        int height = map.height;
        Cell[,] cells = new Cell[width, height];
        Color[] pixels = map.GetPixels(); // Get All Pixels Instead of Reading 1 by 1

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = y * width + x;
                Color pixelColor = pixels[index];
                //    Transparent <¬
                if (pixelColor.a == 0)
                    continue; // Ignore

                if (colorMappingDict.TryGetValue(pixelColor, out ColorToCell colorMapping))
                    cells[x, y] = CreateCell(x, y, colorMapping);
            }
        }

        grid.SetGridArray(cells);
    }

    // Array -> Dictionary
    private void InitializeColorMapping()
    {
        colorMappingDict = new Dictionary<Color, ColorToCell>();
        foreach (ColorToCell mapping in colorMappings)
            if (!colorMappingDict.ContainsKey(mapping.color))
                colorMappingDict[mapping.color] = mapping;
    }

    // Create a Cell 4 the Grid
    private Cell CreateCell(int x, int y, ColorToCell colorMapping)
    {
        Cell cell = new Cell(x, y);
        cell.SetContent(colorMapping.content);
        cell.SetBuildable(colorMapping.buildable);
        cell.SetTraffic(colorMapping.traffic);
        cell.SetIlluminated(colorMapping.illuminated);

        return cell;
    }
}

[System.Serializable]
public class ColorToCell
{
    public Color color;
    public CellContent content;
    public bool buildable;
    [Range(0, 3)] public int traffic;
    public bool illuminated;
}