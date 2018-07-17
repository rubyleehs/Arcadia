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
    public Color32 I_lavaColor;

    public static float camMoveSpeed;
    public static float entranceFallHeight;
    public static float exitFallHeight;
    public static float cellEntranceSpeed;
    public static float cellExitSpeed;
    public static float cellEntranceScale;
    public static float cellExitScale;
    public static float cellExitMaxDelay;
    public static Color32 defaultCellColor;
    public static Color32 lavaColor;

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
    }
}
