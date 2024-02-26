using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProps", menuName = "Character Assets/Character Props")]
public class CharacterProps : ScriptableObject
{
    [System.Serializable]
    public class PropSection
    {
        public Mesh propMesh;
        public string propName;
    }

    public List<PropSection> propSections = new List<PropSection>();
}
