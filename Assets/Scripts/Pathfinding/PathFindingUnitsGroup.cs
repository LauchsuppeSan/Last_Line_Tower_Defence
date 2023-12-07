using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGS;
using static Types;

public class PathFindingUnitsGroup
{
    // Defines how much cells should be between two members of the group
    public double distanceBetweenMembers { get; private set; }

    // List of all PathUnits in the group and its statuses
    private List<(UnitInformation Statuses, PathUnit Unit)> group = null;
    private GameObject groupsParent;


    #region Group
    
    /// <summary>
    /// Creates a new group in the constructor
    /// </summary>
    public PathFindingUnitsGroup(double distanceBetween, GameObject parent)
    {
        group = new List<(UnitInformation, PathUnit)>();
        distanceBetweenMembers = distanceBetween;
        groupsParent = parent;
        groupsParent.GetComponent<AttackFormation>().group = this;
    }

    /// <summary>
    /// Adds the given PathUnit to a group and returns the index of the unit in the group (memberIndex)
    /// </summary>
    /// <param name="unit">PathUnit to add</param>
    /// <returns>Index of new PathUnit in the group (memberIndex)</returns>
    public void AddToGroup(PathUnit unit, Status[] statuses)
    {
        // Fail if no group exist
        if(group == null)
        { throw new System.Exception("Unable to add PathUnit to group: Group does not exist"); }

        // Add the pathUnit and its stauses to the group
        UnitInformation unitsStatuses = new UnitInformation(statuses);
        group.Add((unitsStatuses, unit));

        // Tell the pathUnit its group and its index in it
        unit.group = this;
        unit.memberIndex = group.Count - 1;
    }

    /// <summary>
    /// Removes the PathUnit with the given memberIndex from the group and adjusts the member index of all members behind the removed one
    /// </summary>
    /// <param name="memberIndex">Member to remove</param>
    public void RemoveFromGroup(int memberIndex)
    {
        if(memberIndex == 0 && memberIndex <= group.Count)
        {
            (UnitInformation Statuses, PathUnit Unit) memberToDelet = group.Find(member => member.Unit.memberIndex == memberIndex);
            
            List<(UnitInformation Statuses, PathUnit Unit)> allGroupMembersBehindDeletedMember =
                group.FindAll(member => member.Unit.memberIndex > memberToDelet.Unit.memberIndex);

            int listIndexToDelete = group.IndexOf(memberToDelet);

            group.RemoveAt(listIndexToDelete);
        }
    }

    #endregion Group

    #region Statuses

    /// <summary>
    /// Adds a new status for a member of the group
    /// </summary>
    /// <param name="newStatus">The status to add for the unit</param>
    /// <param name="memberIndex">Index of the member in the group</param>
    public void AddStatusForUnit(Status newStatus, int memberIndex)
    {
        // Get group member
        UnitInformation member = GetUnitStatusInformationOfMember(memberIndex);

        // Set new status for the member if it is not already activ
        member.AddNewStatus(newStatus);
    }

    /// <summary>
    /// Adds a status for every member in the group
    /// </summary>
    /// <param name="newStatus">The status to add to the group</param>
    public void AddStatusForGroup(Status newStatus)
    {
        // Loop though the members and adds the new status to each of them
        for (int memberIndex = 0; memberIndex < group.Count; memberIndex++)
        {
            AddStatusForUnit(newStatus, memberIndex);
        }
    }

    /// <summary>
    /// Remove a status for a member of the group
    /// </summary>
    /// <param name="statusToRemove">Status to remove from member</param>
    /// <param name="index">Index of the member in the group</param>
    public void RemoveStatusFromUnit(Status statusToRemove, int memberIndex)
    {
        // Get group member
        UnitInformation member = GetUnitStatusInformationOfMember(memberIndex);

        // Remove the status if it is activ
        member.RemoveStatus(statusToRemove);
    }

    /// <summary>
    /// Removes a status from each member in the group
    /// </summary>
    /// <param name="statusToRemove"></param>
    public void RemoveStatusFromGroup(Status statusToRemove)
    {
        // Loop through the members and removes the status from each of them
        for (int memberIndex = 0; memberIndex < group.Count; memberIndex++)
        {
            RemoveStatusFromUnit(statusToRemove, memberIndex);
        }
    }

    /// <summary>
    /// Get the horizontal distance beween the given group member ant the group member infront of it
    /// </summary>
    /// <param name="memberIndex">Index of the group member to get the distance to its member infront</param>
    /// <returns>Distance between the given member and the member infront of it</returns>
    public double GetHorizontalDistToMemberForward(int memberIndex)
    {
        // If member has no forward member return -1
        if(memberIndex == 0)
        { return -1; }

        // Get member and forward member positions with the same height level
        Vector3 memberPosition = group[memberIndex].Unit.gameObject.transform.position.PojectOnY(0);
        Vector3 forwardMemberPosition = group[memberIndex - 1].Unit.gameObject.transform.position.PojectOnY(0);

        // Return the horizontal distance to each other
        return Vector3.Distance(memberPosition, forwardMemberPosition);
    }

    /// <summary>
    /// Get the horizontal distnace between the given group member and the group member behind it
    /// </summary>
    /// <param name="memberIndex">Index of the group member to get the distance to its member behind</param>
    /// <returns>Distance between the given member and the member behind it</returns>
    public double GetHorizontalDistToMemberBackward(int memberIndex)
    {
        // If no member is behind return -1
        if(memberIndex == group.Count - 1)
        { return -1; }

        // Return the distance from the member infront the member behind. So basically return the distance to the member behind.
        return GetHorizontalDistToMemberForward(memberIndex + 1);
    }

    /// <summary>
    /// Checks if the given group member has the given status
    /// </summary>
    /// <param name="statusToCheck">Status to check for activity on the given group member</param>
    /// <returns>True if the given member of the group has the given status. Otherwise false</returns>
    public bool UnitHasStatus(Status statusToCheck, int memberIndex)
    {
        // Get group member
        UnitInformation member = GetUnitStatusInformationOfMember(memberIndex);

        // Return if the given member has the given status or not
        return member.HasStatus(statusToCheck);
    }

    /// <summary>
    /// Checks if at least one group member has the given status
    /// </summary>
    /// <param name="statusToCheck">Status to check for activity on at least one group member</param>
    /// <returns>True if at least on member of the group has the given status. Otherwise false</returns>
    public bool SomeUnitInGroupHasStatus(Status statusToCheck)
    {
        // Loop throug each member and check if the given status is activ on it
        for (int memberIndex = 0; memberIndex < group.Count; memberIndex++)
        {
            // If member has the given status breake the loop and return true
            if(UnitHasStatus(statusToCheck, memberIndex))
            {
                return true;
            }
        }

        // If no member has the given status retun false
        return false;
    }

    /// <summary>
    /// Gets the Status informations of the member with the given index
    /// </summary>
    /// <param name="memberIndex">Index of the member in the group</param>
    /// <returns>Status informations from the member with the given index</returns>
    private UnitInformation GetUnitStatusInformationOfMember(int memberIndex)
    {
        return group[memberIndex].Statuses;
    }

    #endregion Statuses



    public void SurroundCell(int cellIndex)
    {
        AddStatusForGroup(Status.Waiting);
        AttackFormation.GetCellsForFormation(cellIndex, cellIndex - 1, 3);
    }

    public void ReturnToMovementFormation()
    {
        // Muss noch gegen einen neuen status namen "Reformation" oder so getauscht werden
        // Und wenn das wieder in line formatieren durch ist, dann erst wird Moving geadded
        AddStatusForGroup(Status.Moving);
    }




    /// <summary>
    /// Collection of all activ informations of a group member
    /// </summary>
    private struct UnitInformation
    {
        // All actif statuses for the member
        private List<Status> statusesList;

        /// <summary>
        /// Initialises a new member
        /// </summary>
        /// <param name="statuses">Startuses for the new member</param>
        public UnitInformation(Status[] statuses)
        {
            statusesList = new List<Status>(statuses);
        }

        /// <summary>
        /// Adds a new status to the member if it is not already activ
        /// </summary>
        /// <param name="statusToAdd">The new status to add</param>
        public void AddNewStatus(Status statusToAdd)
        {
            // Return if status is already activ for the member
            if(statusesList.Contains(statusToAdd))
            { return; }

            if(statusToAdd == Status.Waiting)
            {
                RemoveStatus(Status.Moving);
            }
            else if(statusToAdd == Status.Moving)
            {
                RemoveStatus(Status.Waiting);
            }

            // Add new status
            statusesList.Add(statusToAdd);
        }

        /// <summary>
        /// Removes a status from the member if it has it
        /// </summary>
        /// <param name="statusToRemove">Status to remove</param>
        public void RemoveStatus(Status statusToRemove)
        {
            // Return if the member does not have the status to remove
            if(!statusesList.Contains(statusToRemove))
            { return; }

            // Remove the status from the member
            statusesList.Remove(statusToRemove);
        }

        /// <summary>
        /// Check if the member has the given status
        /// </summary>
        /// <param name="status">Status to check</param>
        /// <returns>TRue if the member has the given status otherwise false</returns>
        public bool HasStatus(Status status)
        {
            return statusesList.Contains(status);
        }
    }
}
