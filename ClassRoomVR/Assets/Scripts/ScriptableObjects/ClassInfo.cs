using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "ClassInfo", menuName = "ScriptableObject/ClassInfo", order = 3)]
    public class ClassInfo : ScriptableObject
    {
        [Serializable]
        public struct NamesLanguage
        {
            public Didascalia_LocalizationManager.Languages language;
            public List<string> maleNames;
            public List<string> femaleNames;
        }

        [Header("Class Information Used to Generate Scenes")]
        [Tooltip("Names depending on app language\nPLEASE FOLLOW THIS ORDER FOR THE ENUMATOR:" +
            "\nSPANISH\nPORTUGUESE\nENGLISH")]

        [SerializeField] private List<NamesLanguage> _names; // Prefijo "_" para variables privadas

        public List<NamesLanguage> GetNames()
        {
            return _names;
        }
    }
}
