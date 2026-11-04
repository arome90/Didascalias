using System;
using UnityEngine;

public class LanguageButton : MonoBehaviour
{
    [SerializeField]
    private Language _language;

    public void SetLanguage(string language)
    {
        Language newLang;

        Enum.TryParse(language, true, out newLang);

        LanguageManager.ChangeLanguage(newLang);
    }

    public void SetSelectedLanguage() => LanguageManager.ChangeLanguage(_language);
}
