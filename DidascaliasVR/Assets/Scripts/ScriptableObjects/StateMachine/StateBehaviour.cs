using Didascalia.StateMachine;
using System;
using UnityEngine;

[Obsolete("We do not use these anymore. We use StudentBehaviour States and Animator States")]
public abstract class StateBehaviour : ScriptableObject
{
    protected StateMachine _machine;
    public virtual void Initialize(StateMachine machine)
    {
        _machine = machine;
    }
    public abstract void Update();
}
