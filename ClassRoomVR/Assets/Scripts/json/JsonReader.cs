using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClassRoomVR
{
    // Clase auxiliar para deserializar el JSON
    [System.Serializable]
    public class Wrapper
    {
        public List<Entry> entries;

        public Dictionary<string, float> ToDictionary()
        {
            Dictionary<string, float> dict = new Dictionary<string, float>();
            foreach (var entry in entries)
            {
                dict[entry.name] = entry.value;
            }
            return dict;
        }
    }

    [System.Serializable]
    public class Entry
    {
        public string name;
        public float value;
    }
}