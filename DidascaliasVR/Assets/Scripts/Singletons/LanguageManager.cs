using UnityEngine.Events;

public enum Language
{
    Spanish = 0,
    Portuguese = 1,
}

public static class LanguageManager
{
    public static Language ActiveLanguage;
    public static UnityEvent<Language> OnActiveLanguageChanged = new UnityEvent<Language>();

    public static void ChangeLanguage(Language newLang)
    {
        if (ActiveLanguage == newLang) return;

        ActiveLanguage = newLang;
        if (OnActiveLanguageChanged != null) OnActiveLanguageChanged.Invoke(ActiveLanguage);
    }
}
