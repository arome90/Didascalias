using UnityEngine;

[CreateAssetMenu(fileName ="Debug", menuName ="StateMachine/States/Debug")]
public class DebugState : Didascalia.StateMachine.State
{
    public override void OnEnter()
    {
        base.OnEnter();

        Debug.Log("[DebugState.cs] Entered debug state", this);
    }
}
