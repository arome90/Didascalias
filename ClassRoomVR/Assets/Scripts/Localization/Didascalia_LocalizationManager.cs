using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.Tables;

public class Didascalia_LocalizationManager : MonoBehaviour
{
    #region Singleton

    private static Didascalia_LocalizationManager _instance = null;
    public static Didascalia_LocalizationManager Instance { get { return _instance; } }

    private void SingletonManagement()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    #endregion

    private void Awake()
    {
        SingletonManagement();
    }

    public enum TableCollections
    {
        MENU = 0,
        SPANISH = 1,
        TUTORIAL = 2,
        CLASE = 3
    }

    public enum Languages
    {
        SPANISH,
        PORTUGUESE
    }


    [Tooltip("Debe seguir el orden del enumerado TABLE_COLLECTIONS:\nMENU = 0\nSPANISH = 1\n")]
    [SerializeField] private StringTableCollection[] _stringTableCollections;

    private StringTable _lastTable;

    private static Languages _currentLanguage;

    public static Languages CurrentLanguage { get { return _currentLanguage; } }

    /// <summary>
    /// Traduce un texto a otro idioma dada una tabla de términos. Esta tabla está identificada
    /// por un parámetro TABLE_COLLECTIONS. En caso de querer traducir de castellano a portugués, por ejemplo,
    /// se daría la siguiente llamada:
    /// string traduction = GetTranslation("Texto", TABLE_COLLECTIONS.SPANISH, SystemLanguage.Portuguese);
    /// </summary>
    /// <param name="key"> Clave que identifica el texto a traducir </param>
    /// <param name="collection"> Colleción de String Tables a utilizar para traducir </param>
    /// <param name="targetLanguage"> Idioma objetivo de la traducción </param>
    /// <param name="traduction"> Traducción resultante; o descripción del error en caso de haberlo </param>
    /// <returns>"false" si ha fallado la traducción; "true" en caso contrario</returns>
    public bool GetTranslation(string key, TableCollections collection,
        Languages targetLanguage, out string traduction)
    {
        traduction = "ERROR";

        StringTableCollection c = _stringTableCollections[(int)collection];

        var table = c.StringTables[(int)targetLanguage];
        StringTableEntry entry = table.GetEntry(key);
        traduction = entry.LocalizedValue;

        return true;
    }

    public static void ChangeLanguage(int localeIndex)
    {
        _currentLanguage = (Languages)localeIndex;
    }
}