using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ClassRoomVR
{
    // Clase auxiliar para deserializar el JSON
    [System.Serializable]
    public class KeyValueWrapper
    {
        public List<KeyValue> entries;

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
    public class KeyValue
    {
        public string name;
        public float value;
    }


    [System.Serializable]
    public class EntryKeyValueDictionaryWrapper
    {
        public List<EntryKeyValueDictionary> entries;

        public Dictionary<string, Dictionary<string, float>> ToDictionary()
        {
            var dict = new Dictionary<string, Dictionary<string, float>>();
            foreach (var entry in entries)
            {
                dict[entry.name] = entry.GetValuesAsDictionary(); // Convierte cada lista en un diccionario
            }
            return dict;
        }
    }

    [System.Serializable]
    public class EntryKeyValueDictionary
    {
        public string name;
        public List<KeyValue> values; // Usa una lista en lugar de un diccionario

        public Dictionary<string, float> GetValuesAsDictionary()
        {
            return values?.ToDictionary(kvp => kvp.name, kvp => kvp.value) ?? new Dictionary<string, float>();
        }
    }
}
