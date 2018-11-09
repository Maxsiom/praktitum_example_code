using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Target {

    public Target(TargetPoint targetObj, Vector3 offset)
    {
        this.targetObject = targetObj;
        this.offset = offset;
    }

    private TargetPoint targetObject;
    public GameObject TargetObject { get { try { return targetObject.gameObject; } catch (MissingReferenceException) { return null; } } }
    public TargetPoint TargetPoint { get { return targetObject; } }

    private Vector3 offset;
    public Vector3 TargetPos
    {
        get {
            try {
                return targetObject.transform.position + offset;
            } catch (MissingReferenceException)
            {
                return Vector3.zero;
            }
        }
    }

}
