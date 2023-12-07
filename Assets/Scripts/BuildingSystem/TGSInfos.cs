using System.Collections;
using System.Collections.Generic;
using TGS;
using UnityEngine;

public static class TGSInfos
{
    public static Vector2 cellSize { get; private set; }

    public static void SetTGSInfos(TerrainGridSystem tgs)
    {
        cellSize = tgs.cellSize;
    }
}
