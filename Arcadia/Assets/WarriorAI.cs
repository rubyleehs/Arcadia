using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarriorAI : BasicEnemyAI {


    public override void StartTurn()
    {
        base.StartTurn();     
    }

    public override bool Attack(bool DealsDamage)
    {
        return base.Attack(DealsDamage);
    }

    public override bool CheckCellIsAttackable(GridCell checkCell)
    {
        return base.CheckCellIsAttackable(checkCell);
    }


    public override void Move(GridCell targetCell)
    {
        targetCell = gridCell.LowestAdjDijkstraCell();
        base.Move(targetCell);
    }

    public override GridCell FindNextPos()
    {
        return gridCell.LowestAdjDijkstraCell();
    }

    public override IEnumerator MoveAnim(GridCell targetCell)
    {
        StartCoroutine(base.MoveAnim(targetCell));
        yield return true;
    }

    public override IEnumerator AttackAnim()
    {
        while (Vector3.SqrMagnitude(this.transform.position - 0.5f * (gridCell.transform.position + InputManager.playerGridCell.transform.position)) >= 0.01f)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, 0.5f * (gridCell.transform.position + InputManager.playerGridCell.transform.position), EnemyManager.enemyMoveSpeed * Time.deltaTime);
            if (Vector3.SqrMagnitude(this.transform.position - 0.5f * (gridCell.transform.position + InputManager.playerGridCell.transform.position)) <= 0.01f)
            {
                this.transform.position = 0.5f * (gridCell.transform.position + InputManager.playerGridCell.transform.position);
                break;
            }
            yield return null;
        }
        while (Vector3.SqrMagnitude(this.transform.position - gridCell.transform.position) >= 0.01f)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, gridCell.transform.position, EnemyManager.enemyMoveSpeed * Time.deltaTime);
            if (Vector3.SqrMagnitude(this.transform.position - gridCell.transform.position) <= 0.01f)
            {
                this.transform.position = gridCell.transform.position;
                break;
            }
            yield return null;
        }
        EnemyManager.TryEndEnemyPhase();
    }
}
