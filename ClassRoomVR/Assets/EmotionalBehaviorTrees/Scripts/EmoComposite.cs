using BehaviorDesigner.Runtime.Tasks;
public class EmoComposite : Composite
{
    [Tooltip("")]
    public float OpennessInfluence;
    [Tooltip("")]
    public float ConscientiousnessInfluence;
    [Tooltip("")]
    public float ExtraversionInfluence;
    [Tooltip("")]
    public float AgreeablenessInfluence;
    [Tooltip("")]
    public float NeuroticismInfluence;

    public float GetOpennessInfluence() => OpennessInfluence;
    public float GetConscientiousnessInfluence() => ConscientiousnessInfluence;
    public float GetExtraversionInfluence() => ExtraversionInfluence;
    public float GetAgreeablenessInfluence() => AgreeablenessInfluence;
    public float GetNeuroticismInfluence() => NeuroticismInfluence;
}
