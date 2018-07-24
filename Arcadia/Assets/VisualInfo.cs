using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualInfo : MonoBehaviour
{
    public float I_camMoveSpeed;
    public float I_entranceFallHeight;
    public float I_exitFallHeight;
    public float I_cellEntranceSpeed;
    public float I_cellExitSpeed;
    public float I_cellEntranceScale;
    public float I_cellExitScale;
    public float I_cellExitMaxDelay;
    public Color32 I_defaultCellColor;
    public Color32 I_highlightCellColor;
    public Color32 I_playerJumpCellColor;
    public Color32 I_lavaColor;
    public float I_deathExpansionRatio;
    public float I_playerJumpHeight;

    public static float camMoveSpeed;
    public static float entranceFallHeight;
    public static float exitFallHeight;
    public static float cellEntranceSpeed;
    public static float cellExitSpeed;
    public static float cellEntranceScale;
    public static float cellExitScale;
    public static float cellExitMaxDelay;
    public static Color32 defaultCellColor;
    public static Color32 highlightCellColor;
    public static Color32 playerJumpCellColor;
    public static Color32 lavaColor;
    public static float deathExpansionRatio;
    public static float playerJumpHeight;

    private void Awake()
    {
        camMoveSpeed = I_camMoveSpeed;
        cellEntranceSpeed = I_cellEntranceSpeed;
        cellExitSpeed = I_cellExitSpeed;
        defaultCellColor = I_defaultCellColor;
        lavaColor = I_lavaColor;
        cellEntranceScale = I_cellEntranceScale;
        cellExitScale = I_cellExitScale;
        entranceFallHeight = I_entranceFallHeight;
        exitFallHeight = I_exitFallHeight;
        cellExitMaxDelay = I_cellExitMaxDelay;
        deathExpansionRatio = I_deathExpansionRatio;
        highlightCellColor = I_highlightCellColor;
        playerJumpHeight = I_playerJumpHeight;
        playerJumpCellColor = I_playerJumpCellColor;
    }

    public static Vector3 GetBezierCurvePoint(List<Vector3> bezierNodes, float t)
    {
        if (bezierNodes.Count == 0)
        {
            Debug.Log("Cannot Form Bezier Curve With No Point, returning Vector3.Zero");
            return Vector3.zero;
        }
        if (bezierNodes.Count == 1)
        {
            Debug.Log("Cannot Form Bezier Curve With Single Point, Returning Original Point");
            return bezierNodes[0];
        }
        if (t > 1 || t < 0)
        {
            //Debug.Log("Start/End Points of Bezier Outside Range. t value should be between 0 and 1. May have error");
        }

        List<Vector3> _finalPoints = new List<Vector3>();
        _finalPoints.AddRange(bezierNodes);
        List<Vector3> _nodes = new List<Vector3>();

        while (_finalPoints.Count > 1)
        {
            _nodes.Clear();
            _nodes.AddRange(_finalPoints);
            _finalPoints.Clear();
            for (int i = 0; i < _nodes.Count - 1; i++)
            {
                _finalPoints.Add(Vector3.Lerp(_nodes[i], _nodes[i + 1], t));
            }

        }
        return _finalPoints[0];
    }

    public static Vector3 GetBezierCurvePoint(Vector3[] bezierNodes, float t)
    {
        if (bezierNodes.Length == 0)
        {
            Debug.Log("Cannot Form Bezier Curve With No Point, returning Vector3.Zero");
            return Vector3.zero;
        }
        if (bezierNodes.Length == 1)
        {
            Debug.Log("Cannot Form Bezier Curve With Single Point, Returning Original Point");
            return bezierNodes[0];
        }
        if (t > 1 || t < 0)
        {
            //Debug.Log("Start/End Points of Bezier Outside Range. t value should be between 0 and 1. May have error");
        }

        List<Vector3> _finalPoints = new List<Vector3>();
        _finalPoints.AddRange(bezierNodes);
        List<Vector3> _nodes = new List<Vector3>();

        while (_finalPoints.Count > 1)
        {
            _nodes.Clear();
            _nodes.AddRange(_finalPoints);
            _finalPoints.Clear();
            for (int i = 0; i < _nodes.Count - 1; i++)
            {
                _finalPoints.Add(Vector3.Lerp(_nodes[i], _nodes[i + 1], t));
            }

        }
        return _finalPoints[0];
    }
}
