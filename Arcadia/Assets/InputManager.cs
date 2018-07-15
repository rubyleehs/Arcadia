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
	void Update () {

        if (playerGridPos == gridGen.grid[stage%2].gridCells[gridGen.grid[stage%2].exitLot.x, gridGen.grid[stage%2].exitLot.y])
        {
            
            StartCoroutine(gridGen.MoveToNextGrid());
        }

        if (!AllowInput) return;

        if (Input.GetButtonDown("Fire1"))
        {
            //Debug.Log("Click");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 20f,gridLayer);

            if (hit == false) return;
            //Debug.Log("Hit");
            GridCell gridCell = hit.transform.GetComponent<GridCell>();


            if (gridCell == null) return;
            if (!gridCell.Walkable) return;

            //Debug.Log("Empty Cell Detected");
            for (int i = 0; i < 6; i++)
            {
                if(playerGridPos.cellNeighbours[i] == gridCell)
                {
                    StartCoroutine(MovePlayer(i));
                    break;
                }
            }
        }

    }

    IEnumerator MovePlayer(int dir)
    {
        playerGridPos = playerGridPos.cellNeighbours[dir];
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
}
