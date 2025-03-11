using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;
namespace ClassRoomVR
{
    public class LoadManager
    {
        private static LoadManager instance;
        private static readonly object lockObject = new object();
        Dictionary<string, object> mainDictionary = new Dictionary<string, object>();

        public static LoadManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new LoadManager();
                        }
                    }
                }
                return instance;
            }
        }

        public static void DestroyInstance()
        {
            lock (lockObject)
            {
                if (instance != null)
                {
                    // Limpiar los recursos si es necesario
                    instance.mainDictionary.Clear();
                    instance = null;
                }
            }
        }

        // Método para obtener un diccionario por clave y tipo (lectura/escritura)
        public bool GetObject<T>(string key, ref T d)
        {
            if (mainDictionary.TryGetValue(key, out var value) && value is T dictionary)
            {
                d = dictionary;
                return true;
            }
            return false;
        }

        public Dictionary<TKey, TValue> LoadDataFromJson<TKey, TValue>(string jsonPath)
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, jsonPath);
            var result = new Dictionary<TKey, TValue>();
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                try
                {
                    // Deserializar el JSON a la estructura de datos
                    //result = JsonUtility.FromJson<Dictionary<TKey, TValue>>(json);
                    result = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(json);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error parsing JSON file: {ex.Message}");
                    result = null;
                }
            }
            else
            {
                Debug.LogError($"File not found at path: {filePath}");
                result = null;
            }
            return result;
        }

        public bool SaveObject<T>(string key, T s)
        {
            if (mainDictionary.ContainsKey(key))
            {
                return false;
            }
            mainDictionary[key] = (T)s;
            return true;
        }

        public Dictionary<TKey, TValue> ConvertDictionary<TKey, TValue>(Dictionary<string, TValue> originalDict)
            where TKey : struct, Enum
        {
            var dict = new Dictionary<TKey, TValue>();
            foreach (var p in originalDict)
            {
                if (Enum.TryParse(p.Key, out TKey key))
                {
                    dict[key] = p.Value;
                }
            }
            return dict;
        }

        /// <summary>
        /// Convierte un Dictionary<string, Dictionary<string, TValue>> a un Dictionary<TOuterKey, Dictionary<TInnerKey, TValue>>.
        /// </summary>
        /// <typeparam name="TOuterKey">El tipo de las claves externas, debe ser un enum.</typeparam>
        /// <typeparam name="TInnerKey">El tipo de las claves internas, debe ser un enum.</typeparam>
        /// <typeparam name="TValue">El tipo de los valores.</typeparam>
        /// <param name="originalDict">El diccionario original a convertir.</param>
        /// <returns>Un nuevo diccionario con las claves convertidas a los tipos especificados.</returns>
        public Dictionary<TOuterKey, Dictionary<TInnerKey, TValue>> ConvertDictionary<TOuterKey, TInnerKey, TValue>(
            Dictionary<string, Dictionary<string, TValue>> originalDict)
            where TOuterKey : struct, Enum
            where TInnerKey : struct, Enum
        {
            var newDict = new Dictionary<TOuterKey, Dictionary<TInnerKey, TValue>>();

            foreach (var outerPair in originalDict)
            {
                if (Enum.TryParse(outerPair.Key, out TOuterKey outerKey))
                {
                    //var innerDict = new Dictionary<TInnerKey, TValue>();
                    //foreach (var innerPair in outerPair.Value)
                    //{
                    //    if (Enum.TryParse(innerPair.Key, out TInnerKey innerKey))
                    //    {
                    //        innerDict[innerKey] = innerPair.Value;
                    //    }
                    //}

                    newDict[outerKey] = ConvertDictionary<TInnerKey, TValue>(outerPair.Value);
                }
            }
            return newDict;
        }

    }
}
