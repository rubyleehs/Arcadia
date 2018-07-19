using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager: MonoBehaviour {
    public static int playerHP = 3;
    public static bool PlayerIsMoving = false;
    public static bool AllowInput = true;
    public float playerMoveSpeed;
    public LayerMask gridLayer;

    public static GridCell playerGridCell;

    public GridGen gridGen;
    public static int stage = 0;
    public static Transform player;
    public EnemyManager enemyManager;

	// Use this for initialization
	void Start () {
        player = this.transform;
        playerGridCell = GridGen.grid[0].gridCells[GridGen.gridRadius, 2];
        player.transform.position = playerGridCell.transform.position;
	}

    // Update is called once per frame
    void Update()
    {

        if (!AllowInput) return;

        if (playerGridCell == GridGen.grid[stage % 2].gridCells[GridGen.grid[stage % 2].exitLot.x, GridGen.grid[stage % 2].exitLot.y])
        {

            StartCoroutine(gridGen.MoveToNextGrid());
        }

        if (Input.GetButtonDown("Fire1"))
        {

            //Debug.Log("Click");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, 20f, gridLayer);

            if (hits.Length == 0) return;
            List<GridCell> clickedCells = new List<GridCell>();
            for (int i = 0; i < hits.Length; i++)
            {
                clickedCells.Add(hits[i].transform.GetComponent<GridCell>());
            }

            for (int i = 0; i < clickedCells.Count; i++)
            {
                if (clickedCells[i].gridNo != stage % 2)
                {
                    clickedCells.RemoveAt(i);
                    i--;
                }
            }

            if (clickedCells.Count == 0) return;
            if (!clickedCells[0].Walkable) return;

            if (EnemyManager.HasActiveEnemies)
            {       
                for (int i = 0; i < clickedCells.Count; i++)
                {
                    for (int d = 0; d < 6; d++)
                    {
                        if (playerGridCell.cellNeighbours[d] == clickedCells[i])
                        {
                            StartCoroutine(MovePlayer(d));
                            StartCoroutine(enemyManager.EnemyPhase());
                            
                            break;
                        }
                    }
                }
            }
            else
            {
                List<GridCell> pathToClick = gridGen.FindShortestPathToPlayer(clickedCells[0]);
                if (pathToClick.Count == 0)
                {
                    Debug.Log("No Path!");
                    return;
                }
                //Debug.Log(pathToClick.Count);
                pathToClick.Reverse();
                pathToClick.RemoveAt(0);
                pathToClick.Add(clickedCells[0]);
                Debug.Log("PathStart");
                StartCoroutine(MovePlayer(pathToClick));
            }
        }
    }

    IEnumerator MovePlayer(int dir)
    {
        Debug.Log("PlayerMoveStart");
        AllowInput = false;
        playerGridCell.Walkable = true;
        playerGridCell.entity = null;
        playerGridCell = playerGridCell.cellNeighbours[dir];
        //playerGridCell.Walkable = false;
        playerGridCell.entity = this.transform;
        gridGen.ResetDijkstra(playerGridCell.arrayCoords.x, playerGridCell.arrayCoords.y, GridGen.grid[stage % 2].heightMap, GridGen.grid[stage % 2].gridCells);
        while (Vector3.SqrMagnitude(player.transform.position - playerGridCell.transform.position) >= 0.01f)
        {
            player.transform.position = Vector3.MoveTowards(this.transform.position, playerGridCell.transform.position, playerMoveSpeed * Time.deltaTime);
            if (Vector3.SqrMagnitude(player.transform.position - playerGridCell.transform.position)<= 0.01f)
            {
                player.transform.position = playerGridCell.transform.position;
                PlayerIsMoving = false; ;
                yield return true;
            }
            else PlayerIsMoving = true;

            yield return null;
        }
    }

    IEnumerator MovePlayer(List<GridCell> path)
    {
        AllowInput = false;
        playerGridCell.Walkable = true;
        playerGridCell.entity = null;
        playerGridCell = path[0];
        PlayerIsMoving = true;
        for (int i = 0; i < path.Count; i++)
        {
            playerGridCell = path[i];
            while (Vector3.SqrMagnitude(player.transform.position - path[i].transform.position) >= 0.01f)
            {
                player.transform.position = Vector3.MoveTowards(this.transform.position, path[i].transform.position, playerMoveSpeed * Time.deltaTime);
                yield return null;
            }
        }
        playerGridCell = path[path.Count - 1];
        //playerGridCell.Walkable = false;
        playerGridCell.entity = this.transform;
        gridGen.ResetDijkstra(playerGridCell.arrayCoords.x, playerGridCell.arrayCoords.y, GridGen.grid[stage % 2].heightMap, GridGen.grid[stage % 2].gridCells);
        PlayerIsMoving = false;
        AllowInput = true;
        Debug.Log("Move End");
        yield return true;
    }
}
