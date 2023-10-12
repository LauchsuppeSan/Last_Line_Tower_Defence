using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(ObstacleAgent))]
public class NavAgent : MonoBehaviour
{
    [SerializeField]
    public float attackRange = 5f;
    [SerializeField]
    public float observationRange = 80f;
    [SerializeField]
    public bool showAttackAreaInEditor = false;
    [SerializeField]
    public bool showObservationAreaInEditor = false;

    private ObstacleAgent agent;
    private Transform mainTarget;
    private Transform currentTarget;
    private bool targetHasChanged;
    private bool startInitializeCompleted;

    private void Start()
    {
        StartCoroutine(SpawnInitializeProcess());
    }

    private IEnumerator SpawnInitializeProcess()
    {
        yield return new WaitForEndOfFrame();
        agent = GetComponent<ObstacleAgent>();
        agent.SetAttackRange(attackRange);

        AddSelfToAgentCollection();
        SetTarget(GameObject.FindGameObjectWithTag("MainTarget").transform);
        
        mainTarget = currentTarget;
        startInitializeCompleted = true;

    }

    // Update is called once per frame
    void Update()
    {
        if(!startInitializeCompleted) { return; }

        if (targetHasChanged)
        {
            // Make sure the ray will start above the ground
            Vector3 pointAboveTarget = currentTarget.position + (Vector3.up * 0.2f);

            Ray targetPosition = new Ray(pointAboveTarget, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(targetPosition, out hit))
            {
                agent.SetDestination(hit.point);
                targetHasChanged = false;
            }
        }
        else
        {
            CheckForNewTarget();
        }
    }

    public void SetTarget(Transform target)
    {
        currentTarget = target;
        targetHasChanged = true;
    }

    private float DistToTarget(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    /// <summary>
    /// Checks for a target in range and set this as activ if one is found otherwise will set HQ as target
    /// </summary>
    private void CheckForNewTarget()
    {
        try
        {
            // Check for some closer target (will trigger the catch block if no possible target is in range)
            Transform nearestTarget =
                ObjectCollections.Towers
                .Where(tower => DistToTarget(tower) <= observationRange)
                .OrderBy(tower => DistToTarget(tower))
                .First()
                .transform;

            // If closer target is found switch to it
            if (nearestTarget != currentTarget)
            {
                SetTarget(nearestTarget);
            }

        }
        catch (Exception e)
        {
            if (e.Message.Contains("contains no elements") == false
                && e.Message.Contains("null") == false)
            { throw e; }

            // If no tower in range is found and if current target is >NOT< equal HQ set the HQ as target again
            if (currentTarget == null || !currentTarget.CompareTag("MainTarget"))
            {
                SetTarget(mainTarget);
            }

            return;
        }
    }





    private void OnDestroy()
    {
        RemoveSelfFromAgentCollection();
    }

    private void AddSelfToAgentCollection()
    {
        ObjectCollections.Agents.Add(gameObject);
    }

    private void RemoveSelfFromAgentCollection()
    {
        ObjectCollections.Agents.Remove(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (showAttackAreaInEditor == false && showObservationAreaInEditor == false) { return; }

        if (showAttackAreaInEditor)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        if (showObservationAreaInEditor)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, observationRange);
        }
    }
}
