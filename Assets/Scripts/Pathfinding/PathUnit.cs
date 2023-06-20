using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TGS;
using UnityEngine;
using static Types;

public class PathUnit : MonoBehaviour, IDamageableByTowers
{
    [SerializeField]
    public Transform target;
    [SerializeField]
    public PathUnitSettings movementSettings = new PathUnitSettings();
    [SerializeField]
    public int fullHealthValue = 100;
    [SerializeField, Tooltip("Incoming damage from explosions is reduced by the percentage specified here"), Range(0f,100f)]
    public float defenceAgainstExplosions = 5f;

    [SerializeField]
    public int currentHealthDebugView;

    // Group informations
    [HideInInspector] public PathFindingUnitsGroup group;
    [HideInInspector] public int memberIndex;

    private TerrainGridSystem tgs;
    private bool canCreatePath = false;
    private List<int> path = new List<int>();
    private int currentHealth;

    private void Start()
    {
        // Set TerrainGridSystem
        tgs = FindObjectOfType<TerrainGridSystem>();

        if (target == null)
        {
            target = GameObject.Find("HQ").transform;
        }

        // Set start parameters
        currentHealth = fullHealthValue;
        currentHealthDebugView = currentHealth;

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
        if (!group.UnitHasStatus(Status.Moving, memberIndex) )
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

        float movementSpeed =
            (group.UnitHasStatus(Status.Waiting, memberIndex),
             group.UnitHasStatus(Status.LetMemberBehindCatchUp, memberIndex),
             group.UnitHasStatus(Status.Slowed, memberIndex)) switch
            {
                (true, _, _) => 0,
                (_, true, true) => movementSettings.movementSpeed * 0.2f,
                (_, true, _) => movementSettings.movementSpeed * 0.3f,
                (_, _, true) => movementSettings.movementSpeed * 0.5f,
                _ => movementSettings.movementSpeed,
            };

        // Move pathUnit forward in a speed depending of the slowed status
        transform.position =
            Vector3.MoveTowards(transform.position,
                                currentTargetCellPosition,
                                movementSpeed * Time.deltaTime);

        // Take the next currentTargetCell if the current target cell is reached and no nextCell is in queue
        if (distToCurrentTargetCell < 0.1)
        {
            path.RemoveAt(0);
            needNextCell = true;
        }
    }

    private float GetMovementSpeedByConsiderStatusEffects()
    {
        return 
            (group.UnitHasStatus(Status.Waiting, memberIndex),
             group.UnitHasStatus(Status.LetMemberBehindCatchUp, memberIndex),
             group.UnitHasStatus(Status.Slowed, memberIndex)) switch
            {
                (true, _, _) => 0,
                (_, true, true) => movementSettings.movementSpeed * 0.2f,
                (_, true, _) => movementSettings.movementSpeed * 0.3f,
                (_, _, true) => movementSettings.movementSpeed * 0.5f,
                _ => movementSettings.movementSpeed,
            };
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
        // Make sure this will called after the GridStartManager is finished by waiting two frames
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        canCreatePath = true;
    }






    /// <summary>
    /// Reduces the incoming raw damage by the percentage the unit has a defence against this kind of damage and returns the real damage value
    /// </summary>
    /// <param name="damageType"></param>
    /// <param name="incomingRawDamage"></param>
    /// <returns></returns>
    public float CalculateComittedDamage(DamageType damageType, float incomingRawDamage)
    {
        Func<float, float> onePercentOfIncoming =
            rawIncomming => incomingRawDamage * 0.01f;

        Func<float, float> wantedPercentageOfDamage =
            defPercentage => 100f - defPercentage;

        return
            damageType switch
            {
                DamageType.Explosion => onePercentOfIncoming(incomingRawDamage) * wantedPercentageOfDamage(defenceAgainstExplosions),
                _ => incomingRawDamage
            };
    }

    /// <summary>
    /// Degreas the current health of the unit by the given value
    /// </summary>
    /// <param name="reductionValue"></param>
    public void DegreaseHealth(float reductionValue)
    {
        currentHealth -= (int) reductionValue;
        currentHealthDebugView = currentHealth;
    }

    /// <summary>
    /// Checks if the health of the unit is 0 or less
    /// </summary>
    /// <returns>true: Unit is dead
    ///          <br>false: Unit is alive</br></returns>
    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    /// <summary>
    /// Adds a status effect to the path unit or its entire grupe for the given amound of time sin secondes (0 means permanent status)
    /// </summary>
    /// <param name="statusToAdd"></param>
    /// <param name="effectDuration">Status activ time. 0 means endless</param>
    /// <param name="toEntireGroup"></param>
    public void AddStatus(Status statusToAdd, float effectDuration, bool toEntireGroup)
    {
        if(toEntireGroup)
        {
            group.AddStatusForGroup(statusToAdd);
        }
        else
        {
            group.AddStatusForUnit(statusToAdd, memberIndex);
        }

        if(effectDuration != 0) 
        {
            StartCoroutine(AddStatusAndRemoveAfterTime(statusToAdd, effectDuration, toEntireGroup));
        }
    }

    /// <summary>
    /// Adds a given status to the unit or its enire group and removes it after time if activtime is not 0
    /// </summary>
    /// <param name="status">Status to add</param>
    /// <param name="activTime">Time between add and remove of the status (0 means permanent status)</param>
    /// <param name="entireGroup">Entire group or just unit status</param>
    /// <returns></returns>
    private IEnumerator AddStatusAndRemoveAfterTime(Status status, float activTime, bool entireGroup)
    {
        // Add status to unit or entire group
        if (entireGroup)
        {
            group.AddStatusForGroup(status);
        }
        else
        {
            group.AddStatusForUnit(status, memberIndex);
        }
        
        // Remove it should be
        if(activTime != 0)
        {
            // Wait the given time
            yield return new WaitForSeconds(activTime);

            // Remove status from unit or entire group
            if (entireGroup)
            {
                group.RemoveStatusFromGroup(status);
            }
            else
            {
                group.RemoveStatusFromUnit(status, memberIndex);
            }
        }
    }

    private void OnDestroy()
    {
        group.RemoveFromGroup(memberIndex);
    }
}
