using ClassRoomVR;
using Oculus.Voice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeWitApp2 : MonoBehaviour
{
    private AppVoiceExperience witApp;
    // Start is called before the first frame update
    void Start()
    {
        witApp = GetComponent<AppVoiceExperience>();
        GameManager2.Instance.OnLanguageChanged.AddListener(ChangeWitAppLanguage);
        ChangeWitAppLanguage();
    }

    public void ChangeWitAppLanguage()
    {
        witApp.RuntimeConfiguration.witConfiguration = GameManager2.Instance.Language;
    }
}
