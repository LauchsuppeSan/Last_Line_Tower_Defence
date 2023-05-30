using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGS;
using static Types;

public class GroupSpawner : MonoBehaviour
{
    [SerializeField, Tooltip("Units in the group")]
    List<GroupMembers> groupMembers;
    [SerializeField, Range(0, 3), Tooltip("0 means the group members will each be in its own cell but all members are next to each other. With no cells beween them")]
    int cellsBetweenMembers = 0;

    double distBeweenTwoCells = 0;
    private PathUnitPrefabs pathUnitPrefabs;
    private TerrainGridSystem tgs;
    private Vector3 spawnPosition;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Wait5Secundes());
    }

    IEnumerator Wait5Secundes()
    {
        yield return new WaitForSeconds(5);
        
        // Get tgs and the center of the cell the spawner is inside as spawn position
        tgs = FindObjectOfType<TerrainGridSystem>();
        Cell cellToSpawnIn = tgs.CellGetAtPosition(transform.position, worldSpace:true);
        spawnPosition = tgs.CellGetPosition(cellToSpawnIn);
        spawnPosition = spawnPosition.PojectOnY(GetSpawnCellGroundY());

        // Get acess to prefabs
        pathUnitPrefabs = FindObjectOfType<PathUnitPrefabs>();

        // Get distance from one cell to another
        Vector3 cell0 = tgs.CellGetPosition(0).PojectOnY(0);
        Vector3 cell1 = tgs.CellGetPosition(1).PojectOnY(0);
        distBeweenTwoCells = Vector3.Distance(cell0, cell1);

        // Create a empty GameObject that will be handle the scripts for the whole group (not unit personal scripts) 
        GameObject groupsParent = Utils.SpawnEmptyObjectWithComponents("PathUnitGroup", new AttackFormation());
        
        // Make shure the group parent is created before creating the group units
        yield return new WaitForEndOfFrame();
        
        // Spawn first group
        SpawnNewGroup(groupsParent);
    }

    /// <summary>
    /// Detect the y-value of the ground in tne center of the spawn position cell
    /// </summary>
    /// <returns>y-value of spawn position cell ground</returns>
    private float GetSpawnCellGroundY()
    {
        RaycastHit hit;

        if(Physics.Raycast(spawnPosition.PojectOnY(10000), Vector3.down, out hit, Mathf.Infinity))
        {
            if(hit.collider.gameObject.CompareTag("Ground")
                || hit.collider.gameObject.CompareTag("Walkable"))
            {
                return hit.point.y;
            }
        }

        throw new System.Exception($"Invalide spawn position: No Ground or Walkable detected ({this.gameObject.name})");
    }

    /// <summary>
    /// Creates a new group of pathUnits
    /// </summary>
    private void SpawnNewGroup(GameObject groupsParent)
    {
        // Create a new group with the specified cells between each member (+1 because if the user say 0 cells between the distance is still 1 cell width and at 1 cell between the distance are 2 cell widths etc)
        PathFindingUnitsGroup newGroup = new PathFindingUnitsGroup(cellsBetweenMembers + 1 * distBeweenTwoCells, groupsParent);
        
        // Initialise some variables
        PathUnit newPathUnit;
        Status[] startStatuses;
        int indexInGroup;

        // Loop through each member in the group and spawnes it
        for (int memberIndex = 0; memberIndex < groupMembers.Count; memberIndex++)
        {
            GroupMembers memberType = groupMembers[memberIndex];

            // Create new PathUnit
            newPathUnit =
                memberType switch
                {
                    GroupMembers.MediumTank => SpawnMediumTank(),
                    _ => throw new System.Exception("Unable to spawn PathUnit: Unknown typ")
                };

            // Create the initialize statuses for the new PathUnit
            startStatuses = new Status[] { Status.Moving };

            // Add pathUnit to its group 
            newGroup.AddToGroup(newPathUnit, startStatuses);

            // Set parent
            newPathUnit.transform.SetParent(groupsParent.transform);
        }
    }

    /// <summary>
    /// Spawn a new medium tank path unit
    /// </summary>
    /// <returns>The new spawned PathUnit instance</returns>
    private PathUnit SpawnMediumTank()
    {
        return Instantiate(pathUnitPrefabs.GetMediumTankPrefab(), spawnPosition, Quaternion.identity).GetComponent<PathUnit>();
    }
}
