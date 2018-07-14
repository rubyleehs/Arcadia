using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager: MonoBehaviour {
    public bool AllowInput = true;
    public float playerMoveSpeed;
    public LayerMask gridLayer;

    public GridCell playerGridPos;

    public GridGen grid;

	// Use this for initialization
	void Start () {
        playerGridPos = grid.gridCells[grid.gridRadius, 2];
        this.transform.position = playerGridPos.transform.position;
	}
	
	// Update is called once per frame
	void Update () {

        this.transform.position = Vector3.MoveTowards(this.transform.position, playerGridPos.transform.position, playerMoveSpeed * Time.deltaTime);
        if(this.transform.position == playerGridPos.transform.position)
        {
            AllowInput = true;
        }
        if (!AllowInput) return;

        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("Click");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, 20f,gridLayer);

            if (hit == false) return;
            Debug.Log("Hit");
            GridCell gridCell = hit.transform.GetComponent<GridCell>();


            if (gridCell == null) return;
            if (!gridCell.Walkable) return;

            Debug.Log("Empty Cell Detected");
            for (int i = 0; i < 6; i++)
            {
                if(playerGridPos.cellNeighbours[i] == gridCell)
                {
                    MovePlayer(i);
                    AllowInput = false;
                    break;
                }
            }
        }

    }

    void MovePlayer(int dir)
    {
        playerGridPos = playerGridPos.cellNeighbours[dir];
    }
}
