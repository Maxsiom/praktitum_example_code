using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Weapons;

public abstract class TargetingSystem {

    public abstract void UpdateTargetSystem(Vector3 position, Vector3 direction);
    public abstract void OnDestroy();

    public abstract Target[] GetTargets(Vector3 position, Vector3 direction, Parameters parameters);
    
}
