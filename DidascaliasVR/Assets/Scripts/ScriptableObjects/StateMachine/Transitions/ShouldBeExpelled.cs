using Didascalia.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "ShouldBeExpelled", menuName = "StateMachine/Transitions/ShouldBeExpelled")]
public class ShouldBeExpelled : Transition
{
    StudentBehaviour _behaviour = null;

    bool _expellingRequested = false;
    private void OnExpellingRequested()
    {
        _expellingRequested = true;
    }

    public override void Initialize(StateMachine machine)
    {
        base.Initialize(machine);
        _behaviour = machine.GetComponent<StudentBehaviour>();
        _behaviour.OnExpellingRequested.AddListener(OnExpellingRequested);
    }

    public override bool Check()
    {
        return _expellingRequested;
    }

    public override void OnCheck()
    {
        _behaviour.MoveTo(ClassManager.Instance.GetDoor());

        _behaviour.OnExpellingRequested.RemoveListener(OnExpellingRequested);
    }
}
