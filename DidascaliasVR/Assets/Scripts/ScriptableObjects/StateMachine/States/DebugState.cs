using UnityEngine;

[CreateAssetMenu(fileName ="Debug", menuName ="StateMachine/States/Debug")]
public class DebugState : Didascalia.StateMachine.State
{
    public override void Update()
    {
        base.Update();

        Debug.Log("debug state update");
    }
}
