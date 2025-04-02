using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine;
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
           // Destroy(this.gameObject);
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
        WEB = 3,
        AUDIO = 4
    }

    public enum Languages
    {
        SPANISH,
        PORTUGUESE,
        ENGLISH
    }

    [Tooltip("Debe seguir el orden del enumerado TABLE_COLLECTIONS:\nMENU = 0\nSPANISH = 1\nTUTORIAL = 2\nWEB = 3\nAUDIO = 4")]
    [SerializeField] private LocalizedStringTable[] _stringTableCollections;

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
    /// <param name="traduction"> Traducción resultante; o descripción del error en caso de haberlo </param>
    /// <returns>"false" si ha fallado la traducción; "true" en caso contrario</returns>
    public bool GetTranslation(string key, TableCollections collection, out string traduction)
    {
        traduction = "ERROR";

        StringTable table = _stringTableCollections[(int)collection].GetTable();

        StringTableEntry entry = table.GetEntry(key);
        traduction = entry.LocalizedValue;

        return true;
    }

    public static void ChangeLanguage(int localeIndex)
    {
        _currentLanguage = (Languages)localeIndex;
    }
}