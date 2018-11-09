using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

public class HitScanTargeting : TargetingSystem
{
    private NearestEnemySpherical system;
    private WeaponBehaviour weapon;
    private GameObject target;

    private bool wasCritical;
    private TargetPointManager targetPointManager;
    private int rnd, copiedPointsLength;
    private TargetPoint tarPoint;
    private GameObject lastTarget;
    private GameObject projector;
    private SinusScale[] projectorScales;

    public HitScanTargeting(WeaponBehaviour weapon)
    {
        this.weapon = weapon;
        system = new NearestEnemySpherical(
                                            LayerMask.GetMask("Shootable"),
                                            LayerMask.GetMask("Shootable", "Obstacle"),
                                            this.weapon.transform.position,
                                            this.weapon.transform.forward,
                                            this.weapon.param.Range,
                                            this.weapon.param.Angle
                                          );
    }

    public override Target[] GetTargets(Vector3 position, Vector3 direction, Parameters parameters)
    {
        if (parameters.SalveCount <= 0)
        {
            Debug.LogWarning("ATTENTION: The SalveCount parameter of the weapon called \"" + weapon.gameObject.name + "\" is less than or equal to 0. Must be at least 1 or higher!", weapon.gameObject);
            return new Target[] { };
        }

        target = system.getTargetEnemy();
        if (target == null)
        {
            return new Target[] { };
        }

        Target[] targets = new Target[parameters.SalveCount];
        int length = targets.Length;

        targetPointManager = target.GetComponentInChildren<TargetPointManager>();
        if (targetPointManager == null)
        {
            throw new MissingComponentException("Target called \"" + target.gameObject.name + "\" does not have any TargetPointManager!");
        }

        List<TargetPoint> copiedPoints;

        // Check if critical hit
        if (wasCritical = (UnityEngine.Random.value <= parameters.CriticalChance))
        {
            copiedPoints = new List<TargetPoint>(targetPointManager.getCriticalTargetPoints());
            copiedPointsLength = targetPointManager.getCriticalCount();
        }
        else
        {
            copiedPoints = new List<TargetPoint>(targetPointManager.getUncriticalTargetPoints());
            copiedPointsLength = targetPointManager.getUncriticalCount();
        }

        for (int i = 0; i < 2; i++)
        {
            while (copiedPointsLength > 0)
            {
                rnd = UnityEngine.Random.Range(0, copiedPointsLength);
                tarPoint = copiedPoints[rnd];
                if (tarPoint.isInShootingAngle(position))
                {
                    targets[0] = tarPoint.getCalculatedHitPoint(parameters.Accuracy);
                    break;
                }
                copiedPoints.RemoveAt(rnd);
                copiedPointsLength -= 1;
            }

            if (copiedPointsLength == 0 && i == 0)
            {
                if (wasCritical)
                {
                    copiedPoints = new List<TargetPoint>(targetPointManager.getUncriticalTargetPoints());
                    copiedPointsLength = targetPointManager.getUncriticalCount();
                }
                else
                {
                    copiedPoints = new List<TargetPoint>(targetPointManager.getCriticalTargetPoints());
                    copiedPointsLength = targetPointManager.getCriticalCount();
                }
            }
            else if (copiedPointsLength == 0)
            {
                Debug.LogWarning("ATTENTION: Cannot target any TargetPoint on the target called \"" + target.gameObject.name + "\" (no available TargetPoint is visible). Targets should have available TargetPoints in every direction!", target.gameObject);
                return new Target[] { };
            }
            else
            {
                break;
            }
        }

        for (int i = 1; i < length; i++)
        {
            targets[i] = tarPoint.getCalculatedHitPoint(targets[0], parameters.Precision);
        }

        // Debugging
        /*for (int i = 0; i < length; i++)
        {
            var a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            a.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            GameObject.Instantiate(a, targets[i].TargetPos, Quaternion.identity);
        }*/

        return targets;
    }

    public override void OnDestroy()
    {
        if (projector != null)
        {
            GameObject.Destroy(projector);
        }
    }

    public override void UpdateTargetSystem(Vector3 position, Vector3 direction)
    {
        system.updateNearestEnemies(position, direction);
        target = system.getTargetEnemy();
        if (target != lastTarget)
        {
            lastTarget = target;
            if (weapon.param.TargetProjector != null)
            {
                if (target != null)
                {
                    if (projector == null)
                    {
                        projector = GameObject.Instantiate(weapon.param.TargetProjector);
                        projectorScales = projector.GetComponentsInChildren<SinusScale>();
                    }
                    projector.transform.parent = target.transform;
                    projector.transform.localPosition = Vector3.zero;
                    if (projectorScales.Length == 2)
                    {
                        var extents = target.GetComponent<Collider>().bounds.size;
                        var biggest = (extents.x > extents.z) ? extents.x : extents.z;
                        if (projectorScales[0].Offset > projectorScales[1].Offset)
                        {
                            projectorScales[0].Offset = biggest + 0.6f;
                            projectorScales[1].Offset = biggest;
                        }
                        else
                        {
                            projectorScales[1].Offset = biggest + 0.6f;
                            projectorScales[0].Offset = biggest;
                        }
                    }
                    projector.SetActive(true);
                }
                else if (target == null && projector != null)
                {
                    projector.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("Your weapon called \"" + weapon.gameObject.name + "\" has no TargetProjector defined in its parameters! Thus you cannot see the active target in game.", weapon);
            }
        }
    }
}
