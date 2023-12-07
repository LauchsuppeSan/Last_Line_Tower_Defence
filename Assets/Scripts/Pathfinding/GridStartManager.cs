using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TGS;

public class GridStartManager : MonoBehaviour
{
    public LayerMask ignoredLayerAtBridgeSearch;
    public Texture2D cellTextureMap;
    public bool test;

    private void Start()
    {
        // Get existing TGS
        TerrainGridSystem tgs = FindObjectOfType<TerrainGridSystem>();

        SetCellsUnderBridgesAsWallkable(tgs);

        if(test)
        {
            SetMovementPercentage(tgs);
        }
    }

    /// <summary>
    /// Defines walkable and not walkable cells by detecting bridges
    /// </summary>
    private void SetCellsUnderBridgesAsWallkable(TerrainGridSystem tgs)
    {
        // Get all cells invisible by TGS settings in inpector
        List<Cell> allInvisibleCells =
            tgs.cells
            .FindAll(c => c.visible == false);

        // Delete inspector settings so that all cells are walkable
        tgs.cellsMinimumAltitude = 0;
        tgs.cellsMaxSlope = 1;

        // Check if some of the at the beginning not walkable cells have a bridge obove them and collect them in the keepWalkable list
        List<Cell> keepCellWalkable = new List<Cell>();
        foreach (Cell cell in allInvisibleCells)
        {
            RaycastHit hit;

            if (Physics.Raycast(tgs.CellGetPosition(cell), Vector3.up, out hit, Mathf.Infinity, ~ignoredLayerAtBridgeSearch))
            {
                if (hit.collider.gameObject.tag == "Walkable")
                {
                    Debug.DrawRay(tgs.CellGetPosition(cell), Vector3.up, Color.red, 100);
                    cell.visible = false;
                    cell.canCross = true;
                    cell.isUnderBridge = true;
                    keepCellWalkable.Add(cell);
                }
                else
                { Debug.Log(hit.collider.gameObject.name); }
            }
        }

        // Remove all cells frome the invisible list, that are in the keep walking list
        allInvisibleCells.RemoveAll(c => keepCellWalkable.Contains(c));

        // Set all remaining cells in the invisible list as not walkable
        foreach (Cell item in allInvisibleCells)
        {
            item.visible = false;
            item.canCross = false;
        }
    }

    private void SetMovementPercentage(TerrainGridSystem tgs)
    {
        int cellIndex = 0;
        // Check if some of the at the beginning not walkable cells have a bridge obove them and collect them in the keepWalkable list
        for (int columnIndex = 0; columnIndex < tgs.columnCount; columnIndex++)
        {
            for (int rowIndex = 0; rowIndex < tgs.rowCount; rowIndex++)
            {
                Color mapPixelColor = cellTextureMap.GetPixel(rowIndex, columnIndex);
                tgs.CellSetColor(cellIndex, mapPixelColor);

                cellIndex++;
            }
        }
    }
}
