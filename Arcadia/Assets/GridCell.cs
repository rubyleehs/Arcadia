using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HexDirection
{
    NW, NE, E, SE, SW, W
}

public static class HexDirectionExtensions
{
    public static HexDirection Opposite(this HexDirection direction)//This only aplies if its a new thing you made, for ints and stuff you need to overide it; i think.
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }
}


public class GridCell : MonoBehaviour
{
    public bool IsCopied;
    public Vector2Int arrayCoords;
    public Vector3Int cellCoords;
    public GridCell[] cellNeighbours = new GridCell[6];
    public float height;

    public bool Walkable = true;
    public Transform entity;

    private SpriteRenderer spriteRenderer;
    private Vector3 startScale;
    public int dijkstraValue;
    public bool IsNowAnim = false;

    public int gridNo;


    public void Awake()
    {
        startScale = Vector3.one * HexMetrics.outerRadius * 2;
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    public void SetNeighbour(HexDirection direction, GridCell cell)
    {
        if (cell == null) return;
        cellNeighbours[(int)direction] = cell;
        cell.cellNeighbours[(int)direction.Opposite()] = this;//notice the opposite(). this is possible due to HexDirExtensions;
    }

    public IEnumerator EntranceAnim()
    {
        IsNowAnim = true;
        Color endColor = spriteRenderer.color;
        Color tempColor = spriteRenderer.color;
        Vector3 endPos = this.transform.localPosition;
        tempColor.a = 0;
        float progress = 0;
        float smoothenProgress = 0;
        spriteRenderer.enabled = true;

        if (entity != null)
        {
            entity.GetComponent<SpriteRenderer>().enabled = true;
        }

        while (progress < 1)
        {
            progress += VisualInfo.cellEntranceSpeed * Time.deltaTime;
            smoothenProgress = Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, progress));
            tempColor = new Color(endColor.r, endColor.g, endColor.b, Mathf.Lerp(0, 1, progress));
            this.transform.localPosition = endPos + new Vector3(0, Mathf.Lerp(VisualInfo.entranceFallHeight, 0, smoothenProgress), 0);
            spriteRenderer.color = tempColor;
            this.transform.localScale = Vector3.Lerp(startScale * VisualInfo.cellEntranceScale, startScale, progress);

            if (progress >= 1)
            {
                spriteRenderer.color = endColor;
                this.transform.localScale = startScale;
                this.transform.localPosition = endPos;
                IsNowAnim = false;
                yield return true;
            }
            yield return null;
        }
    }

    public IEnumerator ExitAnim()//FIX ME!!!
    {
        IsNowAnim = true;
        Color startColor = spriteRenderer.color;
        Color tempColor = spriteRenderer.color;
        Vector3 endPos = this.transform.localPosition;
        tempColor.a = 1;
        float progress = 0;
        float smoothenProgress = 0;

        while (progress < VisualInfo.cellExitMaxDelay)
        {
            progress += Time.deltaTime;
            if (Random.Range(0, VisualInfo.cellExitMaxDelay) <= progress)
            {
                break;
            }
            yield return null;
        }
        progress = 0;
        while (progress < 1)
        {
            progress += VisualInfo.cellExitSpeed * Time.deltaTime;
            smoothenProgress = Mathf.SmoothStep(0, 1, Mathf.SmoothStep(0, 1, progress));
            tempColor = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1, 0, progress));
            spriteRenderer.color = tempColor;
            this.transform.localPosition = endPos + new Vector3(0, Mathf.Lerp(0, VisualInfo.exitFallHeight, smoothenProgress), 0);
            this.transform.localScale = Vector3.Lerp(startScale, startScale * VisualInfo.cellExitScale, progress);

            if (progress >= 1)
            {
                //this.transform.GetComponent<PolygonCollider2D>().enabled = false;

                for (int i = 0; i < this.transform.childCount; i++)
                {
                    this.transform.GetChild(i).gameObject.SetActive(false);
                }
                spriteRenderer.color = startColor;
                this.transform.localScale = startScale;
                spriteRenderer.enabled = false;
                this.transform.localPosition = endPos;
                IsNowAnim = false;
                yield return true;
            }
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "GridAnimBorder" && spriteRenderer.enabled == false && !IsNowAnim)
        {
            if (GridGen.IsMoving)
            {
                StartCoroutine(EntranceAnim());
            }
            else
            {
                this.transform.localScale = startScale;
                spriteRenderer.enabled = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "GridAnimBorder" && spriteRenderer.enabled == true && !IsNowAnim)    
        {
            if (GridGen.IsMoving)
            {
                StartCoroutine(ExitAnim());
            }
            else
            {
                this.transform.localScale = startScale;
                spriteRenderer.enabled = false;
            }
        }
    }

    public GridCell LowestAdjDijkstraCell()
    {
        GridCell nextCell = null;
        for (int i = 0; i < 6; i++)
        {
            if(cellNeighbours[i] != null && cellNeighbours[i].dijkstraValue >0 && cellNeighbours[i].Walkable) //DOnt put Walkable orcannot make/find last step as playable is on non walkable cell lol;
            {
                
                if (nextCell == null) nextCell = cellNeighbours[i];
                else if (nextCell.dijkstraValue > cellNeighbours[i].dijkstraValue) nextCell = cellNeighbours[i];
            }
        }
        return nextCell;
    }
}