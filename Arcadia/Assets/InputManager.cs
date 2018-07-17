using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager: MonoBehaviour {
    public static bool AllowInput = true;
    public float playerMoveSpeed;
    public LayerMask gridLayer;

    public static GridCell playerGridPos;

    public GridGen gridGen;
    public static int stage = 0;
    public static Transform player;

	// Use this for initialization
	void Start () {
        player = this.transform;
        playerGridPos = gridGen.grid[0].gridCells[gridGen.gridRadius, 2];
        player.transform.position = playerGridPos.transform.position;
	}

    // Update is called once per frame
    void Update()
    {

        if (!AllowInput) return;

        if (playerGridPos == gridGen.grid[stage % 2].gridCells[gridGen.grid[stage % 2].exitLot.x, gridGen.grid[stage % 2].exitLot.y])
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
            //Debug.Log("Hit");
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
                        if (playerGridPos.cellNeighbours[d] == clickedCells[i])
                        {
                            StartCoroutine(MovePlayer(d));
                            break;
                        }
                    }
                }
            }
            else
            {
                List<GridCell> pathToClick = gridGen.FindShortestPathToPlayer(clickedCells[0]);
                if (pathToClick.Count == 0) return;
                //Debug.Log(pathToClick.Count);
                pathToClick.Reverse();
                pathToClick.RemoveAt(0);
                pathToClick.Add(clickedCells[0]);

                StartCoroutine(MovePlayer(pathToClick));
            }
        }
    }

    IEnumerator MovePlayer(int dir)
    {
        playerGridPos.Walkable = true;
        playerGridPos.entity = null;
        playerGridPos = playerGridPos.cellNeighbours[dir];
        playerGridPos.Walkable = false;
        playerGridPos.entity = this.transform;
        gridGen.ResetDijkstra(playerGridPos.arrayCoords.x, playerGridPos.arrayCoords.y, gridGen.grid[stage % 2].heightMap, gridGen.grid[stage % 2].gridCells);
        while (player.transform.position != playerGridPos.transform.position)
        {
            player.transform.position = Vector3.MoveTowards(this.transform.position, playerGridPos.transform.position, playerMoveSpeed * Time.deltaTime);
            if (player.transform.position == playerGridPos.transform.position)
            {
                AllowInput = true;
                yield return true;
            }
            else AllowInput = false;

            yield return null;
        }
    }

    IEnumerator MovePlayer(List<GridCell> path)
    {
        playerGridPos.Walkable = true;
        playerGridPos.entity = null;
        playerGridPos = path[0];
        AllowInput = false;
        Debug.Log(path.Count);

        for (int i = 0; i < path.Count; i++)
        {
            Debug.Log( i + ": " + path[i].transform.position);//returns correct stuff.
        }
        for (int i = 0; i < path.Count; i++)
        {
            playerGridPos = path[i];
            Debug.Log(i + ": " + path[i].transform.position);//i = 0 is correct. but i = 1 from above is missing and 2,3,4.... all get shifted back a number
            while (player.transform.position != path[i].transform.position)
            {
                player.transform.position = Vector3.MoveTowards(this.transform.position, path[i].transform.position, playerMoveSpeed * Time.deltaTime);
                yield return null;
            }
        }
        playerGridPos = path[path.Count - 1];
        playerGridPos.Walkable = false;
        playerGridPos.entity = this.transform;
        AllowInput = true;
        gridGen.ResetDijkstra(playerGridPos.arrayCoords.x, playerGridPos.arrayCoords.y, gridGen.grid[stage % 2].heightMap, gridGen.grid[stage % 2].gridCells);
        yield return true;
    }
}
