using ClassRoomVR;
using Oculus.Voice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 
public class ChangeWitApp : MonoBehaviour
{
    private AppVoiceExperience witApp;
    // Start is called before the first frame update
    void Start()
    {
        witApp = GetComponent<AppVoiceExperience>();
        GameManager.Instance.OnLanguageChanged.AddListener(ChangeWitAppLanguage);
        ChangeWitAppLanguage();
    }

    private void OnEnable()
    {
        ChangeWitAppLanguage();
    }

    public void ChangeWitAppLanguage()
    {
        witApp.RuntimeConfiguration.witConfiguration = GameManager.Instance.Language;
    }
}
