using Didascalia.StateMachine;
using UnityEngine;

namespace Didascalia.StateMachine
{
public abstract class Transition : ScriptableObject
{
    protected StateMachine _machine;
    public Didascalia.StateMachine.State NextState = null;

    public virtual void Initialize(StateMachine machine) {
        if(NextState == null)
        {
            Debug.LogError("Transition does not have a NextState defined");
        }

        _machine = machine;
    }
    public abstract bool Check();

    public virtual void OnCheck() { }
}
}
