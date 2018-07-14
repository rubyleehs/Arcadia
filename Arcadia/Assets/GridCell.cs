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


public class GridCell : MonoBehaviour {

    public Vector2Int arrayCoords;
    public Vector3Int cellCoords;
    public GridCell[] cellNeighbours = new GridCell[6];
    public float height;

    public bool Walkable = true;
    public Transform entity;

    public void SetNeighbour(HexDirection direction, GridCell cell)
    {
        if (cell == null) return;
        cellNeighbours[(int)direction] = cell;
        cell.cellNeighbours[(int)direction.Opposite()] = this;//notice the opposite(). this is possible due to HexDirExtensions;
    }

}
