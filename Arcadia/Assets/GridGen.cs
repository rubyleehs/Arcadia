using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexMetrics
{
    public const float outerRadius = 5f;

    public const float innerRadius = outerRadius * 0.866025404f;

    public static Vector2[] corners =
    {
        new Vector2(-innerRadius, -0.5f * outerRadius),
        new Vector2(-innerRadius, 0.5f * outerRadius),
        new Vector2(0f, outerRadius),
        new Vector2(innerRadius, 0.5f * outerRadius),
        new Vector2(innerRadius, -0.5f * outerRadius),
        new Vector2(0f, -outerRadius),
    };
}

public class GridGen : MonoBehaviour
{

    public Color lavaColor;

    public GameObject cellPrefab;
    public int gridRadius;
    public float cellPadding;
    public Transform cellParent;
    public GridCell[,] gridCells;
    private float[,] heightMap;
    private int[,] floodMap;
    public float lavaMinHeight = 43;
    private bool GridPossible = false;

    // Use this for initialization
    void Start()
    {
        floodMap = new int[2 * gridRadius + 1, 2 * gridRadius + 1];
        gridCells = new GridCell[2 * gridRadius + 1, 2 * gridRadius + 1];
        heightMap = new float[2 * gridRadius + 1, 2 * gridRadius + 1];
        for (int y = 0; y <= 2 * gridRadius; y++)
        {
            for (int x = 0; x <= 2 * gridRadius; x++)
            {
                if (Mathf.Abs(x - (y + gridRadius) / 2) <= gridRadius && Mathf.Abs(y - gridRadius) <= gridRadius && Mathf.Abs(x - (y + gridRadius) / 2 + y - gridRadius) <= gridRadius)
                {
                    CreateCell(x, y);
                }
            }
        }
        int endX = 0;
        int endY = 0;
        int w = 0;
        while (!GridPossible)
        {
            for (int y = 0; y <= 2 * gridRadius; y++)
            {
                for (int x = 0; x <= 2 * gridRadius; x++)
                {
                    heightMap[x, y] = (float)Random.Range(0, 100);
                }
            }
            heightMap[gridRadius, 2] = 130;
            while (true)
            {
                endX = Random.Range(1, 2 * gridRadius + 1);
                endY = Random.Range(gridRadius + 3, 2 * gridRadius + 1);

                if (gridCells[endX, endY] != null)
                {
                    heightMap[endX, endY] = 130;
                    break;
                }
            }


            EvenOutHeightMap(2);
            EnsureConnection(new Vector2Int(gridRadius, 2), new Vector2Int(endX,endY));
        }
        for (int y = 0; y <= 2 * gridRadius; y++)
        {
            for (int x = 0; x <= 2 * gridRadius; x++)
            {
                if (gridCells[x, y] != null)
                {
                    gridCells[x, y].height = heightMap[x, y];
                    if (heightMap[x, y] < lavaMinHeight)
                    {
                        gridCells[x, y].transform.GetComponent<SpriteRenderer>().color = lavaColor;
                    }
                }
            }
        }
        gridCells[gridRadius, 2].transform.GetComponent<SpriteRenderer>().color = Color.blue;
        gridCells[endX, endY].transform.GetComponent<SpriteRenderer>().color = Color.cyan;
    }

    public void CreateCell(int x, int y)
    {
        Vector3 cellPos = new Vector3((x - gridRadius) * (HexMetrics.innerRadius + cellPadding) * 2f, (y - gridRadius) * (HexMetrics.outerRadius + cellPadding) * 1.5f, 0);
        cellPos.x += (HexMetrics.innerRadius + cellPadding) * Mathf.Abs((y - gridRadius) % 2);


        GridCell cell = gridCells[x, y] = Instantiate(cellPrefab).GetComponent<GridCell>();
        cell.cellCoords = new Vector3Int(x - (y + gridRadius) / 2, y - gridRadius, x - (y + gridRadius) / 2 + y - gridRadius);
        cell.arrayCoords = new Vector2Int(x, y);


        cell.cellNeighbours = new GridCell[6];


        if (x > 0)
        {
            cell.SetNeighbour(HexDirection.W, gridCells[x - 1, y]);
        }
        if (y > 0)
        {
            if (y % 2 == 1)
            {
                cell.SetNeighbour(HexDirection.SW, gridCells[x, y - 1]);
                if (x + 1 < 2 * gridRadius)
                {
                    cell.SetNeighbour(HexDirection.SE, gridCells[x + 1, y - 1]);
                }
            }
            else
            {
                if (x > 0)
                {
                    cell.SetNeighbour(HexDirection.SW, gridCells[x - 1, y - 1]);
                }
                cell.SetNeighbour(HexDirection.SE, gridCells[x, y - 1]);
            }
        }


        cell.transform.SetParent(cellParent, false);
        cell.transform.localPosition = cellPos;
        //cell.transform.localPosition = new Vector3(x * 15, y * 15);
        cell.transform.localScale = Vector3.one * HexMetrics.outerRadius * 2;
    }

    public void EvenOutHeightMap(int iteration)
    {
        for (int i = 0; i < iteration; i++)
        {
            float[,] cal_heightMap = new float[2 * gridRadius + 1, 2 * gridRadius + 1];

            for (int y = 0; y <= 2 * gridRadius; y++)
            {
                for (int x = 0; x <= 2 * gridRadius; x++)
                {
                    int n = 0;
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (y + dy > 0 && y + dy <= 2 * gridRadius && x + dx > 0 && x + dx <= 2 * gridRadius)
                            {
                                n++;
                                cal_heightMap[x, y] += heightMap[x + dx, y + dy];
                            }
                        }
                    }
                    cal_heightMap[x, y] /= n;
                }
            }
            heightMap = cal_heightMap;
        }
    }
    
    public void EnsureConnection(Vector2Int start, Vector2Int end)
    {
        for (int y = 0; y <= 2 * gridRadius; y++)
        {
            for (int x = 0; x <= 2 * gridRadius; x++)
            {
                if (heightMap[x, y] >= lavaMinHeight && gridCells[x, y] != null)
                {

                    floodMap[x, y] = 1;
                }
                else floodMap[x, y] = -1;
            }
        }
        FloodFill(start.x, start.y, end);
    }

    private void FloodFill(int x, int y, Vector2Int target)
    {
        for (int dy = 1; dy >= -1; dy--)
        {
            for (int dx = 1; dx >= -1; dx--)
            {
                if (!((y%2 ==1 && (dx == -1 && dy != 0)) ||(y % 2 == 0 && (dx == 1 && dy != 0))))
                {
                    if ((x + dx == target.x && y + dy == target.y) || GridPossible)
                    {
                        GridPossible = true;
                        return;
                    }
                    if (!(x + dx > 2 * gridRadius || y + dy > 2 * gridRadius || y + dy < 0 || x + dx < 0))
                    {
                        if (floodMap[x + dx, y + dy] == 1)
                        {
                            floodMap[x + dx, y + dy] = 2;
                            FloodFill(x + dx, y + dy, target);
                            gridCells[x + dx, y + dy].transform.GetComponent<SpriteRenderer>().color = Color.green;
                        }
                    }
                }
            }
        }
    }
}
