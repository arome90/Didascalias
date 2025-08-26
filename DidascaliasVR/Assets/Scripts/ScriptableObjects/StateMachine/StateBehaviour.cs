using Didascalia.StateMachine;
using UnityEngine;

public abstract class StateBehaviour : ScriptableObject
{
    protected StateMachine _machine;
    public virtual void Initialize(StateMachine machine)
    {
        _machine = machine;
    }
    public abstract void Update();
}
