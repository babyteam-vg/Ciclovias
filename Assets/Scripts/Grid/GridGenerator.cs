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

    private void InitializeColorMapping()
    {
        colorMappingDict = new Dictionary<Color, ColorToCell>();
        foreach (ColorToCell mapping in colorMappings)
        {
            colorMappingDict[mapping.color] = mapping;
        }
    }

    private void GenerateGrid()
    {
        InitializeColorMapping();

        int width = map.width;
        int height = map.height;
        Cell[,] cells = new Cell[width, height];
        Color[] pixels = map.GetPixels();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = y * width + x;
                Color pixelColor = pixels[index];

                if (pixelColor.a == 0)
                    continue; // Ignore

                foreach (ColorToCell colorMapping in colorMappings)
                {
                    if (IsColorMatch(pixelColor, colorMapping.color))
                    {
                        {
                            cells[x, y] = CreateCell(x, y, colorMapping);
                            break; // Go to Next Pixel
                        }
                    }
                }
            }
        }

        grid.SetGridArray(cells);
    }

    private Cell CreateCell(int x, int y, ColorToCell colorMapping)
    {
        Cell cell = new Cell(x, y);
        cell.SetContent(colorMapping.content);
        cell.SetBuildable(colorMapping.buildable);
        cell.SetTraffic(colorMapping.traffic);
        cell.SetDanger(colorMapping.danger);
        cell.SetGreenery(colorMapping.greenery);
        cell.SetRevulsion(colorMapping.revulsion);
        cell.SetNearAttraction(colorMapping.nearAttraction);
        cell.SetWaitingPoint(colorMapping.waitingPoint);

        return cell;
    }

    private bool IsColorMatch(Color colorA, Color colorB)
    {
        float tolerance = 0.01f;
        return Mathf.Abs(colorA.r - colorB.r) < tolerance &&
            Mathf.Abs(colorA.g - colorB.g) < tolerance &&
            Mathf.Abs(colorA.b - colorB.b) < tolerance &&
            Mathf.Abs(colorA.a - colorB.a) < tolerance;
    }
}

[System.Serializable]
public class ColorToCell
{
    public Color color;
    public CellContent content;
    public bool buildable;
    public int traffic;
    public int danger;
    public int greenery;
    public int revulsion;
    public bool nearAttraction;
    public bool waitingPoint;
}