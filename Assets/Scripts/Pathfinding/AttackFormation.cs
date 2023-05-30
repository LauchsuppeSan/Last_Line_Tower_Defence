using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGS;

using static Types;

public class AttackFormation : MonoBehaviour
{
    public PathFindingUnitsGroup group { private get; set; }
    static TerrainGridSystem tgs;

    private void Start()
    {
        tgs = FindObjectOfType<TerrainGridSystem>();
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.G))
        {
            group.SurroundCell(2196);
            group.SurroundCell(2907);
            group.SurroundCell(2934);
        }

        if (Input.GetKey(KeyCode.H))
        {
            group.ReturnToMovementFormation();
        }
    }

    public static List<int> GetCellsForFormation(int enemyCell, int currentPathUnitCell, int attackRange)
    {
        int[] possibleCells =
            tgs
            .cells
            .FindAll(cell => Vector3.Distance(tgs.CellGetPosition(cell), tgs.CellGetPosition(tgs.cells[enemyCell])) < attackRange * tgs.cellSize.y - 0.5
                             && cell.canCross 
                             && cell.index != enemyCell)
            .ConvertAll(cell => cell.index)
            .ToArray();

        tgs.CellSetColor(new List<int>(possibleCells), Color.red);

        return new List<int>();
    }
}
