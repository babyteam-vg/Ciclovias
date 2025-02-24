using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private MapData[] maskMaps;
    [SerializeField] private ColorToCell[] colorMappings;

    private Dictionary<Color, ColorToCell> colorMappingDict;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake() { GenerateGrid(); }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Generate a Portion of the Map
    public void GenerateGridPortion(MapData newMapData)
    {
        ClearGridPortion(newMapData.coordinates);
        InitializeColorMapping();

        int width = newMapData.texture.width;
        int height = newMapData.texture.height;
        Color[] pixels = newMapData.texture.GetPixels();

        int startX = newMapData.coordinates.x * width;
        int startY = newMapData.coordinates.y * height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = y * width + x;
                Color pixelColor = pixels[index];

                if (pixelColor.a == 0) continue;

                bool illuminated = pixelColor.a >= 0.45 && pixelColor.a <= 0.55 ? false : true;

                if (colorMappingDict.TryGetValue(pixelColor, out ColorToCell colorMapping))
                    grid.SetCell(startX + x, startY + y, CreateCell(startX + x, startY + y, colorMapping, illuminated));
            }
        }
    }

    // ::::: Clear a Portion of the Map
    public void ClearGridPortion(Vector2Int coordinates)
    {
        int width = maskMaps[0].texture.width; // Se asume que todos los mapas tienen el mismo tamaño.
        int height = maskMaps[0].texture.height;
        int startX = coordinates.x * width;
        int startY = coordinates.y * height;

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid.SetCell(startX + x, startY + y, null);
    }

    // ::::: Get Original Mask Map
    public MapData GetMapDataForCoordinates(Vector2Int coordinates)
    {
        foreach (var map in maskMaps)
        {
            if (map.coordinates == coordinates)
                return map;
        }
        return null; // Retorna null si no se encuentra un mapa con esas coordenadas
    }


    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Generate the Whole Map
    private void GenerateGrid()
    {
        InitializeColorMapping();

        int maxWidth = 0;
        int maxHeight = 0;

        foreach (var map in maskMaps)
        {
            maxWidth = Mathf.Max(maxWidth, map.texture.width);
            maxHeight = Mathf.Max(maxHeight, map.texture.height);
        }

        int gridWidth = 0;
        int gridHeight = 0;

        foreach (var map in maskMaps)
        {
            gridWidth = Mathf.Max(gridWidth, map.coordinates.x * maxWidth + maxWidth);
            gridHeight = Mathf.Max(gridHeight, map.coordinates.y * maxHeight + maxHeight);
        }

        Cell[,] cells = new Cell[gridWidth, gridHeight];

        foreach (var map in maskMaps)
        {
            int width = map.texture.width;
            int height = map.texture.height;
            Color[] pixels = map.texture.GetPixels();
            int startX = map.coordinates.x * maxWidth;
            int startY = map.coordinates.y * maxHeight;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = y * width + x;
                    Color pixelColor = pixels[index];

                    if (pixelColor.a == 0) continue;

                    bool illuminated = pixelColor.a >= 0.45 && pixelColor.a <= 0.55 ? false : true;

                    if (colorMappingDict.TryGetValue(pixelColor, out ColorToCell colorMapping))
                        cells[startX + x, startY + y] = CreateCell(startX + x, startY + y, colorMapping, illuminated);
                }
            }
        }

        grid.SetGridArray(cells);
    }

    // ::::: Array -> Dictionary
    private void InitializeColorMapping()
    {
        colorMappingDict = new Dictionary<Color, ColorToCell>();
        foreach (ColorToCell mapping in colorMappings)
            if (!colorMappingDict.ContainsKey(mapping.color))
                colorMappingDict[mapping.color] = mapping;
    }

    // ::::: Create a Cell 4 the Grid
    private Cell CreateCell(int x, int y, ColorToCell colorMapping, bool illuminated)
    {
        Cell cell = new Cell(x, y);
        cell.SetContent(colorMapping.content);
        cell.SetBuildable(colorMapping.buildable);
        cell.SetIlluminated(illuminated);

        return cell;
    }
}

[System.Serializable]
public class MapData
{
    public Texture2D texture;
    public Vector2Int coordinates;

    public MapData(Texture2D texture, Vector2Int coordinates)
    {
        this.texture = texture;
        this.coordinates = coordinates;
    }
}


[System.Serializable]
public class ColorToCell
{
    public Color color;
    public CellContent content;
    public bool buildable;
}