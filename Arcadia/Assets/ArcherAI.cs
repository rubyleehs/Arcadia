using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherAI : BasicEnemyAI {

    public GameObject arrow;
    public  float arrowBezHeight;
    public float arrowFlySpeed;

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


    public override IEnumerator Move(GridCell targetCell)
    {
        if (gridCell.dijkstraValue > range.y - 1)
        {
            targetCell = gridCell.LowestAdjDijkstraCell();
        }
        else
        {
            List<GridCell> possibleTargetCells = gridCell.NeighboursWithDijkstraValue(gridCell.dijkstraValue + 1);

            if (possibleTargetCells.Count == 0 || gridCell.dijkstraValue > range.x)
            {
                possibleTargetCells = gridCell.NeighboursWithDijkstraValue(gridCell.dijkstraValue);
            }
            if (possibleTargetCells.Count > 0)
            {
                targetCell = possibleTargetCells[Random.Range(0, possibleTargetCells.Count)];
            }
            else targetCell = gridCell;
        }
        StartCoroutine(base.Move(targetCell));
        yield return true;
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
        Transform shotArrow = Instantiate(arrow, this.transform.position, Quaternion.identity).transform;
        List<Vector3> bezierNodes = new List<Vector3>() { this.transform.position, 0.5f * (this.transform.position + InputManager.playerGridCell.transform.position) + new Vector3(0,arrowBezHeight,0), InputManager.playerGridCell.transform.position};

        float progress = 0;

        while(Vector3.Magnitude(shotArrow.position - InputManager.playerGridCell.transform.position) >= 0.01f)
        {
            progress += Time.deltaTime * arrowFlySpeed * (HexMetrics.innerRadius / (Vector3.Magnitude(this.transform.position-InputManager.playerGridCell.transform.position)));
            Vector3 nextPos = VisualInfo.GetBezierCurvePoint(bezierNodes, progress);
            shotArrow.position = nextPos;
            if (Vector3.SqrMagnitude(shotArrow.position - InputManager.playerGridCell.transform.position) <= 0.01f)
            {
                shotArrow.position = InputManager.playerGridCell.transform.position;
                Destroy(shotArrow.gameObject);
                EnemyManager.TryEndEnemyPhase();
                yield return true;
                break;
            }
            else InputManager.AllowInput = false;

            yield return null;     
        }
        yield return null;
    }
}
