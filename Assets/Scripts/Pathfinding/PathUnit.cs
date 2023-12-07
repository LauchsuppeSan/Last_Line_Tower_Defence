using System.Collections;
using System.Collections.Generic;
using TGS;
using UnityEngine;
using static Types;

public class PathUnit : MonoBehaviour
{
    [SerializeField]
    Transform target;

    [SerializeField]
    public PathUnitSettings movementSettings = new PathUnitSettings();

    // Group informations
    [HideInInspector] public PathFindingUnitsGroup group;
    [HideInInspector] public int memberIndex;

    private TerrainGridSystem tgs;
    private bool canCreatePath = false;
    private List<int> path = new List<int>();


    private void Start()
    {
        // Set TerrainGridSystem
        tgs = FindObjectOfType<TerrainGridSystem>();

        if (target == null)
        {
            target = GameObject.Find("HQ").transform;
        }

        // Start pathfinding
        StartCoroutine(CreateFirstPathAfterSpawn());
    }

    void LateUpdate()
    {
        if (path.Count == 0 || !path.TrueForAll(i => tgs.cells[i].visible = true))
        {
            path = FindNewPath();
        }
        else
        {
            FollowPath();
        }
    }

    private List<int> FindNewPath()
    {
        int startIndex = tgs.CellGetAtPosition(transform.position, worldSpace: true).index;
        int targetIndex = tgs.CellGetAtPosition(target.position, worldSpace: true).index;
        return tgs.FindPath(startIndex, targetIndex);
    }

    bool needNextCell = true;
    Vector3 currentTargetCellPosition;

    private void FollowPath()
    {
        if (!group.UnitHasStatus(Status.Moving, memberIndex))
        {
            return;
        }

        float distToCurrentTargetCell = Vector3.Distance(transform.position, currentTargetCellPosition);

        // Stop if the member infront is to close (if no member is infront it will be -1)
        double distanceToMemberInfront = group.GetHorizontalDistToMemberForward(memberIndex);
        if (distanceToMemberInfront > 0 && distanceToMemberInfront <= group.distanceBetweenMembers)
        {
            return;
        }

        // Slow down if the member behind is to to far away (if no member is behind it will be -1)
        double distanceToMemberInBack = group.GetHorizontalDistToMemberBackward(memberIndex);
        if (distanceToMemberInBack > group.distanceBetweenMembers)
        {
            group.AddStatusForUnit(Status.LetMemberBehindCatchUp, memberIndex);
        }
        else
        {
            // Removes the LetMemberBehindCatchUp status if it is activ
            group.RemoveStatusFromUnit(Status.LetMemberBehindCatchUp, memberIndex);
        }

        if (needNextCell)
        {
            Cell nextTagetCell = tgs.cells[path[0]];
            Vector3 cellPosition = tgs.CellGetPosition(nextTagetCell);
            cellPosition = cellPosition + (Vector3.up * tgs.gridNormalOffset);

            currentTargetCellPosition =
                nextTagetCell.isUnderBridge
                ? cellPosition.PojectOnY(transform.position.y)
                : cellPosition;

            needNextCell = false;
        }

        Utils.SmoothLookAt(transform.position, currentTargetCellPosition, transform, movementSettings.rotationSpeed);

        /******************************************************************************************
         * Man kann hier (evtl mit einer neuen variable und lerp oder so) bestimmt dafür sporgen, *
         * dass die Einheiten nicht aprupt langsamer und schneller werden, sonder                 *
         * Beschleunigen und abbremsen um so ein natürlicheres fahrverhalten zu erzeugen          *
         ******************************************************************************************/

        // Move pathUnit forward in a speed depending of the slowed status
        transform.position =
            Vector3.MoveTowards(transform.position,
                                currentTargetCellPosition,
                                group.UnitHasStatus(Status.LetMemberBehindCatchUp, memberIndex)
                                ? movementSettings.movementSpeed / 2 * Time.deltaTime
                                : movementSettings.movementSpeed * Time.deltaTime);

        // Take the next currentTargetCell if the current target cell is reached and no nextCell is in queue
        if (distToCurrentTargetCell < 0.1)
        {
            path.RemoveAt(0);
            needNextCell = true;
        }
    }

    /// <summary>
    /// Get the ground y value of the next waypoint to prevent floating/hovering
    /// </summary>
    /// <param name="cellPosition">Current target waypoint position</param>
    /// <returns>Y-value of the ground in the current target waypoint cell</returns>
    private float GetCellGroundY(Vector3 cellPosition)
    {
        RaycastHit hit;

        if (Physics.Raycast(cellPosition.PojectOnY(10000), Vector3.down, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.CompareTag("Ground"))
            {
                return hit.point.y;
            }
        }

        throw new System.Exception($"Invalide waypoint position: No Ground ({this.gameObject.name})");
    }

    private IEnumerator CreateFirstPathAfterSpawn()
    {
        // Make sure this will called after the GridStartManager is finished
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        canCreatePath = true;
    }
}
