using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace ClassRoomVR
{
    /// <summary>
    /// Singleton para la gestión de carga y guardado de objetos y diccionarios genéricos,
    /// con soporte para serialización y deserialización JSON.
    /// </summary>
    public class LoadManager
    {
        private static LoadManager instance;
        private static readonly object lockObject = new object();

        /// <summary>
        /// Diccionario principal que almacena cualquier tipo de objeto utilizando una clave string.
        /// </summary>
        Dictionary<string, object> mainDictionary = new Dictionary<string, object>();

        /// <summary>
        /// Propiedad para acceder a la instancia Singleton de LoadManager.
        /// </summary>
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


        /// <summary>
        /// Destruye la instancia Singleton y limpia sus datos.
        /// </summary>
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

        /// <summary>
        /// Obtiene un objeto de tipo T almacenado en el diccionario principal.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a recuperar.</typeparam>
        /// <param name="key">Clave utilizada para el almacenamiento.</param>
        /// <param name="d">Referencia de salida al objeto recuperado.</param>
        /// <returns>True si se encuentra y el tipo coincide, false en caso contrario.</returns>
        public bool GetObject<T>(string key, ref T d)
        {
            if (mainDictionary.TryGetValue(key, out var value) && value is T dictionary)
            {
                d = dictionary;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Carga un diccionario desde un archivo JSON utilizando Newtonsoft.Json.
        /// </summary>
        /// <typeparam name="TKey">Tipo de clave del diccionario.</typeparam>
        /// <typeparam name="TValue">Tipo de valor del diccionario.</typeparam>
        /// <param name="jsonPath">Ruta al archivo JSON.</param>
        /// <returns>El diccionario cargado, o null si falla.</returns>
        public Dictionary<TKey, TValue> LoadDataFromJson<TKey, TValue>(string jsonPath)
        {
            var result = new Dictionary<TKey, TValue>();
            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                try
                {
                    // Deserializar el JSON a la estructura de datos
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
                Debug.LogError($"File not found at path: {jsonPath}");
                result = null;
            }
            return result;
        }

        /// <summary>
        /// Guarda un objeto en el diccionario principal solo si la clave no existe.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto.</typeparam>
        /// <param name="key">Clave para almacenar.</param>
        /// <param name="s">Objeto a guardar.</param>
        /// <returns>True si se guarda, false si la clave ya existe.</returns>
        public bool SaveObject<T>(string key, T s)
        {
            if (mainDictionary.ContainsKey(key))
            {
                return false;
            }
            mainDictionary[key] = s;
            return true;
        }

        /// <summary>
        /// Fuerza el guardado (sobrescribe) de un objeto en el diccionario principal.
        /// </summary>
        /// <typeparam name="T">Tipo del objeto.</typeparam>
        /// <param name="key">Clave para almacenar.</param>
        /// <param name="s">Objeto a guardar.</param>
        public void ForceSaveObject<T>(string key, T s)
        {
            mainDictionary[key] = s;
        }

        /// <summary>
        /// Convierte un diccionario con claves string a uno con claves Enum.
        /// </summary>
        /// <typeparam name="TKey">Tipo Enum para las claves.</typeparam>
        /// <typeparam name="TValue">Tipo de los valores.</typeparam>
        /// <param name="originalDict">Diccionario original con claves string.</param>
        /// <returns>Nuevo diccionario con claves Enum.</returns>
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
                    newDict[outerKey] = ConvertDictionary<TInnerKey, TValue>(outerPair.Value);
                }
            }
            return newDict;
        }

        /// <summary>
        /// Rellena un diccionario con los valores del enum como claves y un valor por defecto.
        /// </summary>
        /// <typeparam name="TKey">El tipo de las claves del diccionario, debe ser un enum.</typeparam>
        /// <typeparam name="TValue">El tipo de los valores del diccionario.</typeparam>
        /// <param name="dict">El diccionario a rellenar.</param>
        /// <param name="defaultValue">El valor por defecto a asignar a cada clave.</param>
        public void FillDictionary<TKey, TValue>(ref Dictionary<TKey, TValue> dict, TValue defaultValue) where TKey : Enum
        {
            dict.Clear();
            foreach (TKey key in Enum.GetValues(typeof(TKey)))
            {
                dict[key] = defaultValue;
            }
        }
    }
}
