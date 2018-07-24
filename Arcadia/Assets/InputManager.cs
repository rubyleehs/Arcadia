using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static int playerHP = 3;
    public static bool PlayerIsMoving = false;
    public static bool AllowInput = true;
    public float playerMoveSpeed;
    public LayerMask gridLayer;
    public int moveRange;
    public int jumpRange;
    public bool AllowTurnSkip;
    public int jumpCoolDown;
    public bool PlayerSelected;

    public GameObject attackAnimObj;

    public static GridCell playerGridCell;

    public GridGen gridGen;
    public static int stage = 0;
    public static Transform player;
    public EnemyManager enemyManager;
    private int jumpCoolDownLeft = 0;

    // Use this for initialization
    void Start()
    {
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

            if (hits.Length == 0) {
                gridGen.DehighlightCells();
                return;
            }
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
            
            if (clickedCells[0] == playerGridCell)
            {
                gridGen.DehighlightCells();
                if (!PlayerSelected)
                {
                    PlayerSelected = true;
                    for (int y = 0; y <= 2 * GridGen.gridRadius; y++)
                    {
                        for (int x = 0; x <= 2 * GridGen.gridRadius; x++)
                        {

                            if (GridGen.grid[stage % 2].gridCells[x, y] != null && CanReachToCell(GridGen.grid[stage % 2].gridCells[x, y], moveRange,false)) gridGen.HighlightCell(GridGen.grid[stage % 2].gridCells[x, y],VisualInfo.highlightCellColor);
                            else if (jumpCoolDownLeft <= 0 && GridGen.grid[stage % 2].gridCells[x, y] != null && CanReachToCell(GridGen.grid[stage % 2].gridCells[x, y], jumpRange,true)) gridGen.HighlightCell(GridGen.grid[stage % 2].gridCells[x, y],VisualInfo.playerJumpCellColor);
                        }
                    }
                }
                else
                {
                    PlayerSelected = false;
                    if (AllowTurnSkip) EndPlayerTurn();
                }
                return;
            }
            else if (!clickedCells[0].Walkable)
           {
                gridGen.DehighlightCells();
                PlayerSelected = false;
                Transform clickedEntity = clickedCells[0].entity;
                if (clickedEntity == null) return;

                if (clickedEntity.tag == "Enemy")
                {
                    for (int y = 0; y <= 2 * GridGen.gridRadius; y++)
                    {
                        for (int x = 0; x <= 2 * GridGen.gridRadius; x++)
                        {
                            if (GridGen.grid[stage % 2].gridCells[x, y] != null && clickedEntity.GetComponent<BasicEnemyAI>().CheckCellIsAttackable(GridGen.grid[stage % 2].gridCells[x, y]))
                            {
                                gridGen.HighlightCell(GridGen.grid[stage % 2].gridCells[x, y],VisualInfo.highlightCellColor);
                            }
                        }
                    }
                }
                return;
            }
            else if (jumpCoolDownLeft <= 0 && PlayerSelected && CanReachToCell(clickedCells[0],jumpRange,true) && !CanReachToCell(clickedCells[0], moveRange,false))
            {
                gridGen.DehighlightCells();
                PlayerSelected = false;
                StartCoroutine(PlayerJump(clickedCells[0]));
                return;
            }

            if (gridGen.DehighlightCells() && !PlayerSelected) return;
            PlayerSelected = false;
            if (EnemyManager.HasActiveEnemies)
            {
                for (int i = 0; i < clickedCells.Count; i++)
                {

                    for (int d = 0; d < 6; d++)
                    {
                        if (clickedCells[i] == playerGridCell.cellNeighbours[d])
                        {
                            StartCoroutine(MovePlayer(d));
                            EndPlayerTurn();

                            return;
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

    public bool CanReachToCell(GridCell checkCell, int range, bool UnhinderedSteps)
    {
        if (!checkCell.Walkable) return false;
        if(checkCell == playerGridCell && !AllowTurnSkip)
        {
            return false;
        }
        if (!UnhinderedSteps)
        {
            if (checkCell.dijkstraValue == 1 && AllowTurnSkip) return true;
            if (checkCell.dijkstraValue <= range + 1 && checkCell.dijkstraValue != 1) return true;
            else return false;
        }
        else
        {
            return (GridGen.UnhinderedMinStepsBetween2Cells(playerGridCell, checkCell) <= range);
        }
    }


    IEnumerator MovePlayer(int dir)
    {
        AllowInput = false;
        playerGridCell.Walkable = true;
        playerGridCell.entity = null;
        playerGridCell = playerGridCell.cellNeighbours[dir];
        //playerGridCell.Walkable = false;
        playerGridCell.entity = this.transform;
        gridGen.ResetDijkstra(playerGridCell.arrayCoords.x, playerGridCell.arrayCoords.y, GridGen.grid[stage % 2].heightMap, GridGen.grid[stage % 2].gridCells);
        StartCoroutine(PlayerAttack(dir));
        while (Vector3.SqrMagnitude(player.transform.position - playerGridCell.transform.position) >= 0.01f)
        {
            player.transform.position = Vector3.MoveTowards(this.transform.position, playerGridCell.transform.position, playerMoveSpeed * Time.deltaTime);
            if (Vector3.SqrMagnitude(player.transform.position - playerGridCell.transform.position) <= 0.01f)
            {
                player.transform.position = playerGridCell.transform.position;
                StartCoroutine(PlayerAttack(dir));
                PlayerIsMoving = false;
                yield return true;
            }
            else PlayerIsMoving = true;

            yield return null;
        }
    }

    IEnumerator PlayerJump(GridCell gridCellToJumpTo)
    {
        AllowInput = false;
        jumpCoolDownLeft = jumpCoolDown + 1;
        playerGridCell.Walkable = true;
        playerGridCell.entity = null;
        playerGridCell = gridCellToJumpTo;
        playerGridCell.entity = this.transform;
        gridGen.ResetDijkstra(playerGridCell.arrayCoords.x, playerGridCell.arrayCoords.y, GridGen.grid[stage % 2].heightMap, GridGen.grid[stage % 2].gridCells);
        while (Vector3.SqrMagnitude(player.transform.position - playerGridCell.transform.position) >= 0.01f)
        {
            player.transform.position = Vector3.MoveTowards(this.transform.position, playerGridCell.transform.position, playerMoveSpeed * Time.deltaTime);
            if (Vector3.SqrMagnitude(player.transform.position - playerGridCell.transform.position) <= 0.01f)
            {
                player.transform.position = playerGridCell.transform.position;
                PlayerIsMoving = false;
                EndPlayerTurn();
                yield return true;
            }
            else PlayerIsMoving = true;

            yield return null;
        }
    }

    void EndPlayerTurn()
    {
        jumpCoolDownLeft--;
        AllowInput = false;
        StartCoroutine(enemyManager.EnemyPhase());
    }

    IEnumerator PlayerAttack(int moveDir)
    {
        List<BasicEnemyAI> enemiesToKill = new List<BasicEnemyAI>();
        List<Transform> attackAnimObjs = new List<Transform>();
        List<Vector3[]> dijkPoints = new List<Vector3[]>();
        if (playerGridCell.cellNeighbours[(moveDir + 2) % 6] != null && playerGridCell.cellNeighbours[(moveDir + 2) % 6].entity != null && playerGridCell.cellNeighbours[(moveDir + 2) % 6].entity.tag == "Enemy") enemiesToKill.Add(playerGridCell.cellNeighbours[(moveDir + 2) % 6].entity.GetComponent<BasicEnemyAI>());

        if (playerGridCell.cellNeighbours[(moveDir + 4) % 6] != null && playerGridCell.cellNeighbours[(moveDir + 4) % 6].entity != null && playerGridCell.cellNeighbours[(moveDir +4) % 6].entity.tag == "Enemy") enemiesToKill.Add(playerGridCell.cellNeighbours[(moveDir +4) % 6].entity.GetComponent<BasicEnemyAI>());

        for (int i = 0; i < enemiesToKill.Count; i++)
        {
            dijkPoints.Add(new Vector3[4] { (player.transform.position), enemiesToKill[i].transform.position, playerGridCell.transform.position, playerGridCell.transform.position});
            attackAnimObjs.Add(Instantiate(attackAnimObj, dijkPoints[i][0], Quaternion.identity).transform);
        }
        if (playerGridCell.cellNeighbours[moveDir] != null && playerGridCell.cellNeighbours[moveDir].entity != null && playerGridCell.cellNeighbours[moveDir].entity.tag == "Enemy")
        {
            if (enemiesToKill.Count != 0)
            {
                for (int i = 0; i < enemiesToKill.Count; i++)
                {
                    dijkPoints[i][3] = playerGridCell.cellNeighbours[moveDir].transform.position;
                }
            }
            else
            {
                dijkPoints.Add(new Vector3[2] { player.transform.position, playerGridCell.cellNeighbours[moveDir].transform.position });
                attackAnimObjs.Add(Instantiate(attackAnimObj, dijkPoints[0][0], Quaternion.identity).transform);
            }
            enemiesToKill.Add(playerGridCell.cellNeighbours[moveDir].entity.GetComponent<BasicEnemyAI>());            
        }
        for (int i = 0; i < enemiesToKill.Count; i++)
        {
            StartCoroutine(enemiesToKill[i].Die());
        }

        float progress = 0;
        while (true)
        {
            progress += (playerMoveSpeed * Time.deltaTime) / (2 * HexMetrics.innerRadius);
            for (int i = 0; i < attackAnimObjs.Count; i++)
            {
                attackAnimObjs[i].position = VisualInfo.GetBezierCurvePoint(dijkPoints[i], progress);
            }
            if (progress >= 1)
            {
                for (int i = 0; i < attackAnimObjs.Count; i++)
                {
                    Destroy(attackAnimObjs[i].gameObject);
                    //enemiesToKill[i].Die();
                }
                yield return true;
                break;
            }
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
        