using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPointManager : MonoBehaviour
{

    /// <summary>
    /// Array of all target points sorted by first critical elements and then uncritical elements
    /// </summary>
    private TargetPoint[] targets;

    /// <summary>
    /// Array of all critical target points
    /// </summary>
    private TargetPoint[] critTargets;

    /// <summary>
    /// Array of all uncritical target points
    /// </summary>
    private TargetPoint[] uncritTargets;

    /// <summary>
    /// Amount of critical target points (Or: Position of the first uncritical target point in the targets array)
    /// </summary>
    private int criticalCount;

    /// <summary>
    /// Amount of uncritical target points
    /// </summary>
    private int uncriticalCount;

    // Use this for initialization
    void Start()
    {
        if (gameObject.transform.parent == null)
        {
            throw new MissingComponentException("TargetPointManager must be a direct child (object) of the possible target called \"" + gameObject.name + "\"!");
        }

        if (gameObject.transform.parent.gameObject.layer != LayerMask.NameToLayer("Shootable"))
        {
            Debug.LogWarning("ATTENTION: Layer of the possible target called \"" + gameObject.transform.parent.gameObject.name + "\" is not \"Shootable\"!", gameObject.transform.parent.gameObject);
        }

        // Get all target points on the enemy
        targets = transform.parent.gameObject.GetComponentsInChildren<TargetPoint>();

        if (targets == null || targets.Length == 0)
        {
            throw new MissingComponentException("The possible target called \"" + gameObject.transform.parent.gameObject.name + "\" does not have any TargetPoints!");
        }

        // Sort the array: First critical target points, then uncritical target points
        Array.Sort(targets, delegate (TargetPoint a, TargetPoint b) {
            if (a.critical && !b.critical)
            {
                return -1;
            }
            else if (!a.critical && b.critical)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        });

        // Assign this manager script to all target points and initialize critical count
        int targetsLength = targets.Length;

        for (int i = 0; i < targetsLength; i++)
        {
            targets[i].setTargetPointManager(this);

            var c = targets[i].GetComponent<Collider>();
            if (c != null)
            {
                c.enabled = false;
            }

            var mr = targets[i].GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.enabled = false;
            }

            if (targets[i].critical)
            {
                criticalCount += 1;
            }
        }

        uncriticalCount = targetsLength - criticalCount;

        critTargets = new TargetPoint[criticalCount];
        Array.Copy(targets, critTargets, criticalCount);

        uncritTargets = new TargetPoint[uncriticalCount];
        Array.Copy(targets, criticalCount, uncritTargets, 0, uncriticalCount);
    }

    /// <summary>
    /// Get an array with all target points on the enemy. It is sorted by critical target points first and then uncritical target points.
    /// </summary>
    /// <returns>sorted TargetPoint array (first critical elements then uncritical elements)</returns>
    public TargetPoint[] getTargetPoints()
    {
        return targets;
    }

    /// <summary>
    /// Get an array with all critical target points on the enemy.
    /// </summary>
    /// <returns>critical TargetPoint array</returns>
    public TargetPoint[] getCriticalTargetPoints()
    {
        return critTargets;
    }

    /// <summary>
    /// Get an array with all uncritical target points on the enemy.
    /// </summary>
    /// <returns>uncritical TargetPoint array</returns>
    public TargetPoint[] getUncriticalTargetPoints()
    {
        return uncritTargets;
    }

    /// <summary>
    /// Amount of critical target points (Or: Position of the first uncritical target point in the array)
    /// </summary>
    /// <returns>Amount of critical target points</returns>
    public int getCriticalCount()
    {
        return criticalCount;
    }

    /// <summary>
    /// Amount of uncritical target points
    /// </summary>
    /// <returns>Amount of uncritical target points</returns>
    public int getUncriticalCount()
    {
        return uncriticalCount;
    }
}