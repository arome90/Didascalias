using UnityEngine;

[CreateAssetMenu(fileName ="BehaviourPatternModifier", menuName ="ScriptableObjects/BehaviourPatternModifier")]
public class BehaviourPatternModifier : ScriptableObject
{
    [Header("Base")]
    [Tooltip("Chance that student will laugh at conflicted students")]
    public int _laughAtOthers = 0;

    [Tooltip("Chance that student will trash talk about conflicted students")]
    public int _talkAboutOthers = 0;

    [Tooltip("Chance that student will talk to conflicted students")]
    public int _talkWithOthers = 0;

    [Tooltip("Chance that student will look at conflicted students")]
    public int _lookAtOthers = 0;

    [Header("Modifiers"), Tooltip("Maximum Increse/Decrease of each field per student")]
    [Range(0, 50)]
    public int _laughModifier = 0;
    [Range(0, 50)]
    public int _talkAboutModifier = 0;
    [Range(0, 50)]
    public int _talkToModifier = 0;
    [Range(0, 50)]
    public int _lookAtModifier = 0;
}
