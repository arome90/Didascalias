using System;
using UnityEngine;
using UnityEngine.UI;

public class LanguageButton : MonoBehaviour
{
    [SerializeField]
    private Language _language;

    private void Awake()
    {
        LanguageManager.OnActiveLanguageChanged.AddListener(DisableIfLanguageIsSame);
    }
    private void OnDestroy()
    {
        LanguageManager.OnActiveLanguageChanged.RemoveListener(DisableIfLanguageIsSame);
    }

    private void DisableIfLanguageIsSame(Language newLang)
    {
        GetComponent<Button>().interactable = _language != newLang;
    }

    public void SetLanguage(string language)
    {
        Language newLang;

        Enum.TryParse(language, true, out newLang);

        LanguageManager.ChangeLanguage(newLang);
    }

    public void SetSelectedLanguage() => LanguageManager.ChangeLanguage(_language);
}
