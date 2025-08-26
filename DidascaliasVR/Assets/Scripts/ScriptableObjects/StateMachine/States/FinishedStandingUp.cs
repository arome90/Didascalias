using Didascalia.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "FinishedStandingUp", menuName = "StateMachine/Transitions/FinishedStandingUp")]
public class FinishedStandingUp : Transition
{
    protected bool _finished = false;
    protected StudentBehaviour _behaviour;

    private void OnStandUp()
    {
        _finished = true;
    }

    public override void Initialize(StateMachine machine)
    {
        base.Initialize(machine);

        _behaviour = machine.GetComponent<StudentBehaviour>();

        _behaviour.OnStandUp.AddListener(OnStandUp);
    }

    public override bool Check()
    {
        return _finished;
    }

    public override void OnCheck()
    {
        base.OnCheck();

        _behaviour.OnStandUp.RemoveListener(OnStandUp);
    }
}
