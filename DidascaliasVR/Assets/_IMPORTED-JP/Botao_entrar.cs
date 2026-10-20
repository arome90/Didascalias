using System.Collections;
using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;

public class Botao_entrar : MonoBehaviour
{
    [Header("Conexão com a Equipe")]
    public ClassSettings configuracaoDaSala;

    [Header("Paineis da Interface")]
    public GameObject mainMenuPanel;
    public GameObject connectDevicePanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject loadingPanel;

    [Header("Elementos de Erro")]
    public GameObject mensagemErro;

    private DatabaseReference dbReference;

    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == Firebase.DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("<color=#00FF00>[Firebase]</color> Pronto para conectar!");
            }
        });
    }

    public void OpenConnectDeviceScreen() { HideAllPanels(); if (connectDevicePanel != null) connectDevicePanel.SetActive(true); }
    public void OpenSettingsScreen() { HideAllPanels(); if (settingsPanel != null) settingsPanel.SetActive(true); }
    public void OpenCreditsScreen() { HideAllPanels(); if (creditsPanel != null) creditsPanel.SetActive(true); }
    public void ReturnToMainMenu() { HideAllPanels(); if (mainMenuPanel != null) mainMenuPanel.SetActive(true); }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (connectDevicePanel != null) connectDevicePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    public void EntrarNaSala()
    {
        //if (dbReference == null)
        //{
        //    dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        //}

        HideAllPanels();
        if (mensagemErro != null) mensagemErro.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(true);

        //string idDoOculos = "1e0d1b3680fb8b3614367dc38705fd90";
#if UNITY_EDITOR
        // idDoOculos = "556b4d4918be07211402fd354c3a672d9d06fc68";
#endif

        FirebaseClassroomManager.StartRoomResult result = 
            FirebaseClassroomManager.Instance.StartRoom();

        if (result.wasSuccesful)
        {
            configuracaoDaSala = result.settings;
        }
        else
        {
            ShowErrorOnRoomNotFound(result.errorText);
            ReturnToMainMenu();
        }
    }

    private void ShowErrorOnRoomNotFound(string textoDoErro)
    {
        if (mensagemErro != null)
        {
            TMP_Text textoUI = mensagemErro.GetComponentInChildren<TMP_Text>();
            if (textoUI != null) textoUI.text = textoDoErro;

            mensagemErro.SetActive(true);
            StartCoroutine(EsconderErroAposTempo());
        }
    }

    private IEnumerator EsconderErroAposTempo()
    {
        yield return new WaitForSeconds(4f);
        if (mensagemErro != null) mensagemErro.SetActive(false);
    }
}