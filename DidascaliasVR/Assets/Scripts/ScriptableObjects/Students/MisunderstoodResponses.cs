using UnityEngine;

/// <summary>
/// A list of all the possible phrases a student can say when they don't understand what the 
/// teacher has said
/// </summary>
[CreateAssetMenu(fileName ="MisunderstoodResponses", menuName ="ScriptableObjects/MisunderstoodResponses")]
public class MisunderstoodResponses : ScriptableObject
{
    public System.Collections.Generic.List<string> PossibleResponses = new System.Collections.Generic.List<string>();
}
