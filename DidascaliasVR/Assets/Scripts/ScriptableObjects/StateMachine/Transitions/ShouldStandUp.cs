using System;
using Didascalia.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "ShouldStandUp", menuName = "StateMachine/Transitions/ShouldStandUp")]
public class ShouldStandUp : Transition
{
    StudentBehaviour _behaviour = null;

    public override void Initialize(StateMachine machine)
    {
        base.Initialize(machine);

        _behaviour = machine.GetComponent<StudentBehaviour>();
        _behaviour.OnStandUpRequested.AddListener(OnStandUpRequested);
    }

    private void OnStandUpRequested()
    {
        _behaviour.SetOnFoot();
    }

    public override bool Check()
    {
        return _behaviour.State != StudentState.Sitting;
    }

    public override void OnCheck()
    {
        _behaviour.OnStandUpRequested.RemoveListener(OnStandUpRequested);
        _behaviour.SetOnFoot();
    }
}
