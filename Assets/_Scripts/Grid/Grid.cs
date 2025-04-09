using UnityEngine;
using System.Collections.Generic;
using static Cell;
using System.Linq;

public class Grid : MonoBehaviour
{
    private Cell[,] gridArray;
    private int width, height;
    private float cellSize = 1f;

    // :::::::::: PUBLIC METHODS ::::::::::
    public void SetGridArray(Cell[,] cells)
    {
        this.gridArray = cells;
        this.width = cells.GetLength(0);
        this.height = cells.GetLength(1);
    }

    public Vector2Int GetGridDimensions() { return new Vector2Int(width, height); }
    public float GetCellSize() { return cellSize; }

    // ::::: Get a Cell
    public Cell GetCell(int x, int y)
    {
        return IsWithinBounds(x, y) ? gridArray[x, y] : null;
    }

    // ::::: Set a Cell (NEW)
    public void SetCell(int x, int y, Cell cell)
    {
        if (IsWithinBounds(x, y))
            gridArray[x, y] = cell;
    }

    // ::::: Grid (x, y) to World (i, j, k)
    public Vector3 GetWorldPositionFromCell(int x, int y) { return new Vector3(x, 0, y) * cellSize; }
    public Vector3 GetWorldPositionFromCellCentered(float x, float y) { return new Vector3((x + 0.5f) * cellSize, 0, (y + 0.5f) * cellSize); }

    // ::::: World (i, j, k) to Grid (x, y)
    public Vector2Int? GetCellFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / cellSize);
        int y = Mathf.FloorToInt(worldPosition.z / cellSize);

        if (IsWithinBounds(x, y) && GetCell(x, y) != null)
            return new Vector2Int(x, y);
        return null;
    }

    // ::::: Set and Get Content
    public void SetCellContent(int x, int y, CellContent content)
    {
        if (IsWithinBounds(x, y) && gridArray[x, y] != null)
            gridArray[x, y].SetContent(content);
    }

    public CellContent GetCellContent(int x, int y)
    {
        if (IsWithinBounds(x, y) && gridArray[x, y] != null)
            return gridArray[x, y].GetContent();
        return CellContent.None;
    }

    // ::::: Is [x][y] Not Off Limits?
    public bool IsWithinBounds(int x, int y) { return x >= 0 && x < width && y >= 0 && y < height; }

    // ::::: Get Adjacent Cells to [x][y]
    public HashSet<Cell> GetAdjacentCells(int x, int y, bool considerCentral = false)
    {
        HashSet<Cell> adjacentCells = new HashSet<Cell>();

        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (!considerCentral && (dx == 0 && dy == 0))
                    continue;

                int newX = x + dx;
                int newY = y + dy;

                if (IsWithinBounds(newX, newY) && gridArray[newX, newY] != null)
                    adjacentCells.Add(gridArray[newX, newY]);
            }

        return adjacentCells;
    }

    // ::::: Get Diagonal Cells to [x][y]
    public List<Cell> GetDiagonalCells(int centerX, int centerY)
    {
        List<Cell> diagonalCells = new List<Cell>();

        int[] dx = { -1, -1, 1, 1 };
        int[] dy = { -1, 1, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int newX = centerX + dx[i];
            int newY = centerY + dy[i];

            if (IsWithinBounds(newX, newY))
            {
                Cell diagonalCell = GetCell(newX, newY);
                if (diagonalCell != null)
                {
                    diagonalCells.Add(diagonalCell);
                }
            }
        }

        return diagonalCells;
    }

    // ::::: Check Adjacency in 8 Directions
    public bool IsAdjacent(Vector2Int current, Vector2Int target)
    {
        int dx = Mathf.Abs(current.x - target.x);
        int dy = Mathf.Abs(current.y - target.y);
        return (dx <= 1 && dy <= 1);
    }

    // ::::: Edge to Center of a Cell (2D)
    public Vector2 EdgeToMid(Vector2Int edgePosition)
    {
        float centerX = edgePosition.x + cellSize / 2;
        float centerY = edgePosition.y + cellSize / 2;
        return new Vector2(centerX, centerY);
    }

    // ::::: Edge to Center of a Cell (3D)
    public Vector3 EdgeToMid(Vector3 edgePosition)
    {
        float centerX = edgePosition.x + cellSize / 2;
        float centerZ = edgePosition.y + cellSize / 2;
        return new Vector3(centerX, edgePosition.y, centerZ);
    }

    // ::::: Get a Group of Adjacent Cells w/the Same Content
    public List<Cell> GetAdjacentCellsOfContent(Vector2Int gridPosition, CellContent targetContent)
    {
        List<Cell> result = new List<Cell>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(gridPosition);
        visited.Add(gridPosition);

        while (queue.Count > 0)
        {
            Vector2Int currentPos = queue.Dequeue();
            Cell currentCell = GetCell(currentPos.x, currentPos.y);

            if (currentCell == null || currentCell.GetContent() != targetContent)
                continue;

            result.Add(currentCell);

            HashSet<Cell> adjacentCells = GetAdjacentCells(currentPos.x, currentPos.y);
            foreach (Cell adjacentCell in adjacentCells)
            {
                if (adjacentCell == null) continue;

                Vector2Int adjacentPos = new Vector2Int(adjacentCell.x, adjacentCell.y);

                if (!visited.Contains(adjacentPos))
                {
                    visited.Add(adjacentPos);
                    queue.Enqueue(adjacentPos);
                }
            }
        }

        return result;
    }
}
