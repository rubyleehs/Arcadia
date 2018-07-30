using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BasicEnemyAI : MonoBehaviour
{
    public Vector2Int range;
    public GridCell gridCell;
    public bool AttackObstructable;
    public bool IsStunned;
    public bool IsMoving;
    public bool WillBeDead = false;


    public virtual void StartTurn()
    {
        if (IsStunned)
        {    
            IsStunned = false;
            //EnemyManager.TryEndEnemyPhase();
            return;
        }
        else if (!Attack(true))
        {
            Move(FindNextPos());
        }
    }

    public void Push(GridCell targetCell)
    {
        IsStunned = true;
        Move(gridCell);
    }

    public virtual void Move(GridCell targetCell)
    {
        if (IsStunned)
        {
            //Debug.Log("archer stunned");
            targetCell = gridCell;
           
            if (!targetCell.Walkable)
            {
                WillBeDead = true;
            }
            targetCell.Walkable = false;
            targetCell.entity = this.transform;
            StartCoroutine(MoveAnim(targetCell));
            return;
        }
        if ((targetCell != null && targetCell.Walkable && targetCell != gridCell))
        {
            gridCell.Walkable = true;
            gridCell.entity = null;
            targetCell.Walkable = false;
            targetCell.entity = this.transform;
            gridCell = targetCell;

            StartCoroutine(MoveAnim(targetCell));
        }
        else if(targetCell != null)
        {
            //gridCell.Walkable = true;
            //gridCell.entity = null;
            //WillBeDead = true;//
            StartCoroutine(MoveAnim(targetCell));
           // StartCoroutine(Die(VisualInfo.deathExpansionRatio));
        }
    }

    public virtual GridCell FindNextPos()
    {
        Debug.Log("Error! Pls override this in parent");
        return null;
    }

    public virtual IEnumerator MoveAnim(GridCell targetCell)
    {
        if(Vector3.SqrMagnitude(this.transform.position - targetCell.transform.position) <= 0.01f)
        {
            this.transform.position = targetCell.transform.position;
            IsMoving = false;
            EnemyManager.TryEndEnemyPhase();
            if (WillBeDead)
            {
                StartCoroutine(Die(VisualInfo.deathExpansionRatio));
            }
            yield return true;
        }
        

        while (Vector3.SqrMagnitude(this.transform.position - targetCell.transform.position) >= 0.01f)
        {
            IsMoving = true;
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetCell.transform.position, EnemyManager.enemyMoveSpeed * Time.deltaTime);
            if (Vector3.SqrMagnitude(this.transform.position - targetCell.transform.position) <= 0.01f)
            {
                this.transform.position = targetCell.transform.position;
                IsMoving = false;
                EnemyManager.TryEndEnemyPhase();

                if (WillBeDead)
                {
                    StartCoroutine(Die(VisualInfo.deathExpansionRatio));
                }
                yield return true;
            }
            else InputManager.AllowInput = false;

            yield return null;
        }
        yield return true;
    }


    public virtual bool Attack(bool DealsDamage)//inherited script should call its own animation;
    {
        if (CheckCellIsAttackable(InputManager.playerGridCell)) 
        {
            if (DealsDamage)
            {
                InputManager.playerHP--;
                Debug.Log("Player Damaged!");
                StartCoroutine(AttackAnim());
            }
            return true;
        }
        else return false; ;
    }

    public virtual bool CheckCellIsAttackable(GridCell checkCell)//inherited script should call its own animation;
    {
        if (checkCell == gridCell) return false; 
        List<int> dif = new List<int>() { Mathf.Abs(gridCell.cellCoords.x - checkCell.cellCoords.x), Mathf.Abs(gridCell.cellCoords.y - checkCell.cellCoords.y), Mathf.Abs(gridCell.cellCoords.z - checkCell.cellCoords.z) };
        dif.Sort();
        if (gridCell.dijkstraValue > 0 && dif[1] <= range.y && dif[1] >= range.x && dif[0] == 0)
        {
            return true;
        }
        else return false; ;
    }

    public virtual IEnumerator AttackAnim()
    {
        Debug.Log("Error! Pls override this in parent");
        EnemyManager.TryEndEnemyPhase();
        yield return true;
    }

    public bool MoveToNextGrid()
    {
        if (!this.isActiveAndEnabled) return false;
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

    public IEnumerator Die(float deathExpansionRatio)
    {
        while (IsMoving)
        {
            Debug.Log("DeathMove");//
            yield return null;
        }
        gridCell.Walkable = true;
        gridCell.entity = null;
        gridCell = null;
        SpriteRenderer spriteRenderer = this.transform.GetComponent<SpriteRenderer>();
        Color startColor = spriteRenderer.color;
        Color endColor = spriteRenderer.color;
        Vector3 endScale = this.transform.localScale * deathExpansionRatio;
        endColor.a = 0;
        float progress = 0;

        while(progress < 1)
        {
            progress += VisualInfo.cellExitSpeed * Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, progress);
            this.transform.localScale = Vector3.Lerp(this.transform.localScale, endScale, progress);
            
            if(progress >= 1)
            {
                Debug.Log("Die");
                Destroy(this.gameObject);
                yield return true;
            }
            yield return null;

        }
    }
}
