using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;

public enum Language
{
    Spanish = 1,
    Portuguese = 0,
}

public class LanguageManager : Singleton<LanguageManager>
{
    public static Language ActiveLanguage;
    public static UnityEvent<Language> OnActiveLanguageChanged = new UnityEvent<Language>();

    private void Start()
    {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        yield return LocalizationSettings.InitializationOperation;
        ChangeLanguage(Language.Spanish);
    }

    public static void ChangeLanguage(Language newLang)
    {
        if (ActiveLanguage == newLang) return;

        ActiveLanguage = newLang;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[(int)ActiveLanguage];

        if (OnActiveLanguageChanged != null) OnActiveLanguageChanged.Invoke(ActiveLanguage);
    }

    public void Button_ChangeLanguage(Language newLang) => ChangeLanguage(newLang);
}