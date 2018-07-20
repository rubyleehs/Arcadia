using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BasicEnemyAI : MonoBehaviour
{
    public int range;
    public GridCell gridCell;

    public virtual void StartTurn()
    {
        if (!Attack())
        {
            StartCoroutine(Move(FindNextPos()));
        }
    }

    public virtual IEnumerator Move(GridCell targetCell)
    {     
        if (targetCell != null && targetCell.Walkable)
        {
            gridCell.Walkable = true;
            gridCell.entity = null;
            targetCell.Walkable = false;
            targetCell.entity = this.transform;
            gridCell = targetCell;

            StartCoroutine(MoveAnim(targetCell));
        }
        else
        {
            StartCoroutine(EnemyManager.TryEndEnemyPhase());
            yield return true;
        }
    }

    public virtual GridCell FindNextPos()
    {
        Debug.Log("Error! Pls override this in parent");
        return null;
    }

    public virtual IEnumerator MoveAnim(GridCell targetCell)
    {
        while (Vector3.SqrMagnitude(this.transform.position - targetCell.transform.position) >= 0.01f)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetCell.transform.position, EnemyManager.enemyMoveSpeed * Time.deltaTime);
            if (Vector3.SqrMagnitude(this.transform.position - targetCell.transform.position) <= 0.01f)
            {
                this.transform.position = targetCell.transform.position;
                StartCoroutine(EnemyManager.TryEndEnemyPhase());
                yield return true;
            }
            else InputManager.AllowInput = false;

            yield return null;
        }
        yield return true;
    }


    public virtual bool Attack()//inherited script should call its own animation;
    {
        if (gridCell.dijkstraValue> 0 && gridCell.dijkstraValue <= range + 1 && (gridCell.cellCoords.x == InputManager.playerGridCell.cellCoords.x || gridCell.cellCoords.y == InputManager.playerGridCell.cellCoords.y || gridCell.cellCoords.z == InputManager.playerGridCell.cellCoords.z))
        {
            InputManager.playerHP--;
            Debug.Log("Player Damaged!");
            StartCoroutine(AttackAnim());
            return true;
        }
        else return false; ;
    }

    public virtual IEnumerator AttackAnim()
    {
        Debug.Log("Error! Pls override this in parent");
        StartCoroutine(EnemyManager.TryEndEnemyPhase());
        yield return true;
    }

    public bool MoveToNextGrid()
    {
        if (gridCell.transform.parent == GridGen.cellParents[InputManager.stage % 2]) return true;
        Vector2Int newPos = gridCell.arrayCoords - (GridGen.grid[(InputManager.stage +1) % 2].exitLot - new Vector2Int(GridGen.gridRadius, 2));
        if(newPos.y %2 == 1 && GridGen.grid[(InputManager.stage + 1) % 2].exitLot.y % 2 == 1)
        {
            newPos.x++;
        }
        if (newPos.x > 0 && newPos.x <= 2 * GridGen.gridRadius && newPos.y >= 0 && newPos.y <= 2 * GridGen.gridRadius && GridGen.grid[(InputManager.stage) % 2].gridCells[newPos.x, newPos.y] != null)
        {
            gridCell.Walkable = true;
            gridCell.entity = null;
            gridCell = GridGen.grid[(InputManager.stage) % 2].gridCells[newPos.x, newPos.y];
            gridCell.entity = this.transform;
            gridCell.Walkable = false;
            this.transform.position = gridCell.transform.position;
            return true;
        }
        else
        {
            gridCell.Walkable = true;
            gridCell.entity = null;
            return false;
        }

    }
}
