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

[System.Serializable]
public class Grid
{
    public GridCell[,] gridCells;
    public float[,] heightMap;
    public Vector2Int exitLot;//2d array coords
}

public class GridGen : MonoBehaviour
{
    public float camMoveSpeed;
    public Color lavaColor;
    public Color normalColor;

    public GameObject cellPrefab;
    public int gridRadius;
    public float cellPadding;

    public Grid[] grid;
    public Transform[] cellParents;


    private int[,] dijkstraMap;
    public float lavaMinHeight = 43;
    private bool GridPossible = false;
    



    // Use this for initialization
    void Awake()
    {
        grid = new Grid[2];
        dijkstraMap = new int[2 * gridRadius + 1, 2 * gridRadius + 1];
        grid[0] = new Grid()
        {
            gridCells = new GridCell[2 * gridRadius + 1, 2 * gridRadius + 1],
            heightMap = new float[2 * gridRadius + 1, 2 * gridRadius + 1],
        };
        grid[1] = new Grid()
        {
            gridCells = new GridCell[2 * gridRadius + 1, 2 * gridRadius + 1],
            heightMap = new float[2 * gridRadius + 1, 2 * gridRadius + 1],
        };

        for (int y = 0; y <= 2 * gridRadius; y++)
        {
            for (int x = 0; x <= 2 * gridRadius; x++)
            {
                if (Mathf.Abs(x - (y + gridRadius) / 2) <= gridRadius && Mathf.Abs(y - gridRadius) <= gridRadius && Mathf.Abs(x - (y + gridRadius) / 2 + y - gridRadius) <= gridRadius)
                {
                    CreateCell(x, y, grid[0].gridCells, cellParents[0]);
                    CreateCell(x, y, grid[1].gridCells, cellParents[1]);
                    //oddGridCells[x, y].gameObject.transform.position += grid[0].gridCells[exitLot.x, exitLot.y].transform.position;
                }
            }
        }
        int attempts = 0;
        while (!GridPossible)
        {
            attempts++;
            CreateMap(ref grid[0].heightMap, grid[0].gridCells, ref grid[0].exitLot);

            CreateNextHeightMap(grid[0].exitLot.x - gridRadius, grid[0].exitLot.y - 2, grid[0].heightMap, ref grid[1].heightMap);
            CreateMap(ref grid[1].heightMap, grid[1].gridCells, ref grid[1].exitLot);
        }
        Debug.Log(attempts);


        cellParents[1].transform.position += grid[0].gridCells[grid[0].exitLot.x, grid[0].exitLot.y].transform.position - grid[0].gridCells[gridRadius, 2].transform.position;
        cellParents[1].gameObject.SetActive(false);
        UpdateVisuals(grid[0].heightMap, grid[0].gridCells, grid[0].exitLot);
        UpdateVisuals(grid[1].heightMap, grid[1].gridCells, grid[1].exitLot);
    }

    public void CreateMap(ref float[,] _heightMap, GridCell[,] _gridCells, ref Vector2Int _exitLot)
    {
        //Debug.Log(_gridCells[gridRadius,gridRadius].transform.position);
        //EXIT LOT IS AT 0,0 COZ CANNOT SPAWN EXIT AS top half no height map. should work when height map is completed.
        GridPossible = false;
        int tries = 0;
        float[,] tempHeightMap = new float[2 * gridRadius + 1, 2 * gridRadius + 1];
        for (int y = 0; y <= 2 * gridRadius; y++)
        {
            for (int x = 0; x <= 2 * gridRadius; x++)
            {
                tempHeightMap[x, y] = _heightMap[x, y];
            }
        }

        while (!GridPossible && tries < 7)
        {
            tries++;
            for (int y = 0; y <= 2 * gridRadius; y++)
            {
                for (int x = 0; x <= 2 * gridRadius; x++)
                {
                    _heightMap[x, y] = tempHeightMap[x, y];
                }
            }
            for (int y = 0; y <= 2 * gridRadius; y++)
            {
                for (int x = 0; x <= 2 * gridRadius; x++)
                {
                    if (_heightMap[x, y] == 0)
                    {
                        if (x == gridRadius && y == 2)
                        {
                            _heightMap[x, y] = 130;
                        }
                        else _heightMap[x, y] = (float)Random.Range(0, 100);

                        if (y >= gridRadius * 1.5f && _heightMap[x, y] < lavaMinHeight)
                        {
                            _heightMap[x, y] *= 1.2f;
                        }
                    }
                    else
                    {
                        _heightMap[x, y] = -_heightMap[x, y];
                        //Debug.Log(tries + " : " + _heightMap[x, y]);
                    }
                }
            }

            while (true)
            {
                _exitLot.x = Random.Range(1, 2 * gridRadius + 1);
                _exitLot.y = Random.Range(gridRadius + 3, 2 * gridRadius -1);

                if (_gridCells[_exitLot.x, _exitLot.y] != null && (_exitLot.x <= gridRadius-2 || _exitLot.x >= gridRadius +2))
                {
                    //Debug.Log(_gridCells[_exitLot.x, _exitLot.y].transform.position);
                    _heightMap[_exitLot.x, _exitLot.y] = 130;
                    break;
                }
            }

            //Debug.Log("s: " + grid[0].heightMap[gridRadius, 2]);

            EvenOutHeightMap(2, ref _heightMap);
            for (int y = 0; y <= 2 * gridRadius; y++)
            {
                for (int x = 0; x <= 2 * gridRadius; x++)
                {
                    if (_heightMap[x, y] < 0)
                    {
                        _heightMap[x, y] *= -1;
                    }
                }
            }
            //Debug.Log("e: " + grid[0].heightMap[gridRadius, 2]);
            ResetDijkstra(gridRadius, 2, _heightMap, _gridCells);
            if (dijkstraMap[_exitLot.x, _exitLot.y] < 1000 && dijkstraMap[_exitLot.x, _exitLot.y] > 0)
            {
                //Debug.Log(dijkstraMap[endX, endY]);
                GridPossible = true;
            }
        }
        

        //Debug.Log("tries : " + tries);
        //UpdateVisuals(_heightMap, _gridCells, _exitLot);
    }

    public void UpdateVisuals(float[,] _heightMap, GridCell[,] _gridCells, Vector2Int _exitLot)
    {
        for (int y = 0; y <= 2 * gridRadius; y++)
        {
            for (int x = 0; x <= 2 * gridRadius; x++)
            {
                if (_gridCells[x, y] != null)
                {
                    _gridCells[x, y].height = _heightMap[x, y];
                    if (_heightMap[x, y] < lavaMinHeight)
                    {
                        _gridCells[x, y].Walkable = false;
                        _gridCells[x, y].transform.GetComponent<SpriteRenderer>().color = lavaColor;
                    }
                    else
                    {
                        _gridCells[x, y].Walkable = true;
                        _gridCells[x, y].transform.GetComponent<SpriteRenderer>().color = normalColor;
                    }
                }
            }
        }
        _gridCells[gridRadius, 2].transform.GetComponent<SpriteRenderer>().color = Color.blue;
       // Debug.Log(_exitLot.x + " , " + _exitLot.y);
        _gridCells[_exitLot.x, _exitLot.y].transform.GetComponent<SpriteRenderer>().color = Color.cyan;
    }


    public void CreateCell(int x, int y, GridCell[,] _gridCells, Transform _cellParent)
    {
        Vector3 cellPos = new Vector3((x - gridRadius) * (HexMetrics.innerRadius + cellPadding) * 2f, (y - gridRadius) * (HexMetrics.outerRadius + cellPadding) * 1.5f, 0);
        cellPos.x += (HexMetrics.innerRadius + cellPadding) * Mathf.Abs((y - gridRadius) % 2);


        GridCell cell = _gridCells[x, y] = Instantiate(cellPrefab).GetComponent<GridCell>();
        cell.cellCoords = new Vector3Int(x - (y + gridRadius) / 2, y - gridRadius, x - (y + gridRadius) / 2 + y - gridRadius);
        cell.arrayCoords = new Vector2Int(x, y);


        cell.cellNeighbours = new GridCell[6];


        if (x > 0)
        {
            cell.SetNeighbour(HexDirection.W, _gridCells[x - 1, y]);
        }
        if (y > 0)
        {
            if ((y % 2 == 0 && gridRadius % 2 == 1) || (y % 2 == 1 && gridRadius % 2 == 0))
            {
                cell.SetNeighbour(HexDirection.SW, _gridCells[x, y - 1]);
                if (x + 1 < 2 * gridRadius)
                {
                    cell.SetNeighbour(HexDirection.SE, _gridCells[x + 1, y - 1]);
                }
            }
            else
            {
                if (x > 0)
                {
                    cell.SetNeighbour(HexDirection.SW, _gridCells[x - 1, y - 1]);
                }
                cell.SetNeighbour(HexDirection.SE, _gridCells[x, y - 1]);
            }
        }


        cell.transform.SetParent(_cellParent, false);
        cell.transform.localPosition = cellPos;
        //cell.transform.localPosition = new Vector3(x * 15, y * 15);
        cell.transform.localScale = Vector3.one * HexMetrics.outerRadius * 2;
        
    }

    public void EvenOutHeightMap(int iteration, ref float[,] _heightMap)
    {
        for (int i = 0; i < iteration; i++)
        {
            float[,] cal_heightMap = new float[2 * gridRadius + 1, 2 * gridRadius + 1];

            for (int y = 0; y <= 2 * gridRadius; y++)
            {
                for (int x = 0; x <= 2 * gridRadius; x++)
                {
                    if (_heightMap[x, y] >= 0)
                    {
                        int n = 0;
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (y + dy > 0 && y + dy <= 2 * gridRadius && x + dx > 0 && x + dx <= 2 * gridRadius)
                                {
                                    n++;
                                    cal_heightMap[x, y] += Mathf.Abs(_heightMap[x + dx, y + dy]);
                                }
                            }
                        }
                        cal_heightMap[x, y] /= n;
                    }
                    else cal_heightMap[x, y] = _heightMap[x, y];


                }
            }
            _heightMap = cal_heightMap;
        }
    }

    public void ResetDijkstra(int xPos, int yPos, float[,] _heightMap, GridCell[,] _gridCells)
    {
        for (int y = 0; y <= 2 * gridRadius; y++)
        {
            for (int x = 0; x <= 2 * gridRadius; x++)
            {
                if (_heightMap[x, y] >= lavaMinHeight && _gridCells[x, y] != null)
                {
                    dijkstraMap[x, y] = 1000;
                }
                else dijkstraMap[x, y] = -1;
            }
        }
        dijkstraMap[xPos, yPos] = 1;
        UpdateDijkstra(xPos, yPos, _heightMap, _gridCells);
    }

    private void UpdateDijkstra(int x, int y, float[,] _heightMap, GridCell[,] _gridCells)
    {
        for (int dy = 1; dy >= -1; dy--)
        {
            for (int dx = 1; dx >= -1; dx--)
            {
                if (!((y % 2 == 1 && (dx == -1 && dy != 0)) || (y % 2 == 0 && (dx == 1 && dy != 0))))
                {
                    if (!(x + dx > 2 * gridRadius || y + dy > 2 * gridRadius || y + dy < 0 || x + dx < 0))
                    {
                        if (dijkstraMap[x + dx, y + dy] > dijkstraMap[x, y] + 1)
                        {
                            dijkstraMap[x + dx, y + dy] = dijkstraMap[x, y] + 1;
                            UpdateDijkstra(x + dx, y + dy, _heightMap, _gridCells);
                            //_gridCells[x + dx, y + dy].transform.GetComponent<SpriteRenderer>().color = Color.green;
                        }
                    }
                }
            }
        }
    }

    private void CreateNextHeightMap(int xDisplacement, int yDisplacement, float[,] mapToCopy, ref float[,] mapToPaste)
    {
        for (int y = 0; y <= 2 * gridRadius; y++)
        {
            for (int x = 0; x <= 2 * gridRadius; x++)
            {
                if (x + xDisplacement <= 2 * gridRadius && y + yDisplacement <= 2 * gridRadius && x + xDisplacement > 0 && y + yDisplacement > 0)
                {
                    if (yDisplacement % 2 == 1 && y % 2 == 1 && x + xDisplacement +1<= 2 * gridRadius)
                    {
                        mapToPaste[x, y] = mapToCopy[x + xDisplacement +1, y + yDisplacement];//THE ISSUE IS DUE TO ODD/EVEN CANNOT BLINDLY COPY
                        //Debug.Log(mapToCopy[x + xDisplacement, y + yDisplacement] + " vs " + mapToPaste[x, y]);
                    }
                    else
                    {
                        mapToPaste[x, y] = mapToCopy[x + xDisplacement, y + yDisplacement];
                    }
                }
                else mapToPaste[x, y] = 0;
                //Debug.Log(mapToCopy[x + xDisplacement, y + yDisplacement] + " vs " + mapToPaste[x, y]);
            }
        }
    }

    public IEnumerator MoveToNextGrid()
    {
        InputManager.stage++;
        cellParents[InputManager.stage % 2].gameObject.SetActive(true);
        while (Camera.main.transform.position != cellParents[InputManager.stage % 2].transform.position + new Vector3(0, 0, -10))
        {
            InputManager.AllowInput = false;
            Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, cellParents[InputManager.stage % 2].transform.position + new Vector3(0, 0, -10), camMoveSpeed * Time.deltaTime);
            if (Camera.main.transform.position == cellParents[InputManager.stage % 2].transform.position + new Vector3(0, 0, -10))
            {
                SetupNextGrid();
                yield return true;
            }
            yield return null;
        }
    }

    void SetupNextGrid()
    {
        InputManager.AllowInput = true;
        cellParents[(InputManager.stage + 1) % 2].gameObject.SetActive(false);
        Camera.main.transform.position = new Vector3(0, 0, -10);
        cellParents[InputManager.stage % 2].transform.position = Vector3.zero;
        cellParents[(InputManager.stage + 1) % 2].transform.position = grid[InputManager.stage % 2].gridCells[grid[InputManager.stage % 2].exitLot.x, grid[InputManager.stage % 2].exitLot.y].transform.position - grid[InputManager.stage % 2].gridCells[gridRadius, 2].transform.position;
        InputManager.playerGridPos = grid[InputManager.stage % 2].gridCells[gridRadius, 2];
        InputManager.player.position = InputManager.playerGridPos.transform.position;

        CreateNextHeightMap(grid[InputManager.stage % 2].exitLot.x - gridRadius, grid[InputManager.stage % 2].exitLot.y - 2, grid[InputManager.stage % 2].heightMap, ref grid[(InputManager.stage + 1) % 2].heightMap);
        CreateMap(ref grid[(InputManager.stage + 1) % 2].heightMap, grid[(InputManager.stage + 1) % 2].gridCells, ref grid[(InputManager.stage + 1) % 2].exitLot);
        UpdateVisuals(grid[(InputManager.stage + 1) % 2].heightMap, grid[(InputManager.stage + 1) % 2].gridCells, grid[(InputManager.stage + 1) % 2].exitLot);
    }

}
    