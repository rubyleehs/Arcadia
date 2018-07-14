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
    public GridCell[,] nextGridCells;


    private float[,] heightMap;
    private float[,] nextHeightMap;

    private int[,] dijkstraMap;
    public float lavaMinHeight = 43;
    private bool GridPossible = false;
    public Vector2Int exitLot;
    public Vector2Int nextExitLot;



    // Use this for initialization
    void Awake()
    {
        dijkstraMap = new int[2 * gridRadius + 1, 2 * gridRadius + 1];
        gridCells = new GridCell[2 * gridRadius + 1, 2 * gridRadius + 1];
        nextGridCells = new GridCell[2 * gridRadius + 1, 2 * gridRadius + 1];
        heightMap = new float[2 * gridRadius + 1, 2 * gridRadius + 1];
        nextHeightMap = new float[2 * gridRadius + 1, 2 * gridRadius + 1];


        for (int y = 0; y <= 2 * gridRadius; y++)
        {
            for (int x = 0; x <= 2 * gridRadius; x++)
            {
                if (Mathf.Abs(x - (y + gridRadius) / 2) <= gridRadius && Mathf.Abs(y - gridRadius) <= gridRadius && Mathf.Abs(x - (y + gridRadius) / 2 + y - gridRadius) <= gridRadius)
                {
                    CreateCell(x, y, gridCells);
                    CreateCell(x, y, nextGridCells);
                    nextGridCells[x, y].gameObject.transform.position += new Vector3(200, 0, 0);
                }
            }
        }
        int attempts = 0;
        while (!GridPossible)
        {
            attempts++;
            CreateMap(ref heightMap, gridCells, ref exitLot);

            CreateNextHeightMap(exitLot.x - gridRadius, exitLot.y - 2, heightMap, ref nextHeightMap, nextGridCells);
            CreateMap(ref nextHeightMap, nextGridCells, ref nextExitLot);
        }
        Debug.Log(attempts);
        UpdateVisuals(heightMap, gridCells, exitLot);
        UpdateVisuals(nextHeightMap, nextGridCells, nextExitLot);
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
                    _heightMap[exitLot.x, exitLot.y] = 130;
                    break;
                }
            }

            //Debug.Log("s: " + heightMap[gridRadius, 2]);

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
            //Debug.Log("e: " + heightMap[gridRadius, 2]);
            ResetDijkstra(gridRadius, 2, _heightMap, _gridCells);
            if (dijkstraMap[exitLot.x, exitLot.y] < 1000 && dijkstraMap[exitLot.x, exitLot.y] > 0)
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
                    else _gridCells[x, y].Walkable = true;
                }
            }
        }
        _gridCells[gridRadius, 2].transform.GetComponent<SpriteRenderer>().color = Color.blue;
       // Debug.Log(_exitLot.x + " , " + _exitLot.y);
        _gridCells[_exitLot.x, _exitLot.y].transform.GetComponent<SpriteRenderer>().color = Color.cyan;
    }


    public void CreateCell(int x, int y, GridCell[,] _gridCells)
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


        cell.transform.SetParent(cellParent, false);
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

    private void CreateNextHeightMap(int xDisplacement, int yDisplacement, float[,] mapToCopy, ref float[,] mapToPaste, GridCell[,] nextCells)
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
        //EvenOutHeightMap(2, ref nextHeightMap);
        //CreateMap(ref mapToPaste , nextCells);
        //UpdateVisuals(mapToPaste, nextCells);
    }
}
    