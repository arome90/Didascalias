using Didascalia.StateMachine;
using UnityEngine;

[CreateAssetMenu(fileName = "ChangePlacesRequested", menuName = "StateMachine/Transitions/ChangePlacesRequested")]
public class ChangePlacesRequested : Transition
{
    StudentBehaviour _behaviour;

    bool _changePlaces = false;
    private void OnChangePlacesRequested()
    {
        _changePlaces = true;
    }

    public override void Initialize(StateMachine machine)
    {
        base.Initialize(machine);

        _behaviour = machine.GetComponent<StudentBehaviour>();
        _behaviour.OnChangePlacesRequested.AddListener(OnChangePlacesRequested);
    }

    public override bool Check()
    {
        return _changePlaces;
    }

    public override void OnCheck()
    {
        _behaviour.MoveTo(_behaviour.SitSpot);

        _behaviour.OnChangePlacesRequested.RemoveListener(OnChangePlacesRequested);
    }
}
