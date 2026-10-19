using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        if (dbReference == null)
        {
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        }

        HideAllPanels();
        if (mensagemErro != null) mensagemErro.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(true);

        string idDoOculos = SystemInfo.deviceUniqueIdentifier;
        //string idDoOculos = "1e0d1b3680fb8b3614367dc38705fd90";
#if UNITY_EDITOR
        // idDoOculos = "556b4d4918be07211402fd354c3a672d9d06fc68";
#endif
        Debug.Log($"<color=cyan>[MENU]</color> Buscando no Firebase a sala do óculos: <b>{idDoOculos}</b>");

        dbReference.Child("classroom_configs")
                   .GetValueAsync().ContinueWithOnMainThread(task =>
                   {
                       if (task.IsFaulted)
                       {
                           Debug.LogError("<color=red>[MENU]</color> Falha ao buscar no Firebase.");
                           ReturnToMainMenu();
                           MostrarErroFaltaDeSala("Erro de conexão com o servidor.");
                       }
                       else if (task.IsCompleted)
                       {
                           DataSnapshot snapshot = task.Result;
                           if (snapshot.Exists && snapshot.ChildrenCount > 0)
                           {
                               DataSnapshot salaMaisRecente = null;
                               string maiorDataStr = "";
                               int salasEncontradas = 0;

                               // Percorre todas as salas e faz o filtro localmente (C#)
                               foreach (DataSnapshot sala in snapshot.Children)
                               {
                                   // Verifica se a sala tem o ID do óculos correto
                                   if (sala.HasChild("activeHeadsetId") && sala.Child("activeHeadsetId").Value.ToString() == idDoOculos)
                                   {
                                       salasEncontradas++;

                                       // Filtra a sala mais recente
                                       if (sala.HasChild("criadoEm"))
                                       {
                                           string dataCriacao = sala.Child("criadoEm").Value.ToString();
                                           if (string.Compare(dataCriacao, maiorDataStr) >= 0)
                                           {
                                               maiorDataStr = dataCriacao;
                                               salaMaisRecente = sala;
                                           }
                                       }
                                       else
                                       {
                                           if (salaMaisRecente == null) salaMaisRecente = sala;
                                       }
                                   }
                               }

                               if (salaMaisRecente != null)
                               {
                                   Debug.Log($"<color=#00FF00>[MENU]</color> Sucesso! {salasEncontradas} sala(s) encontrada(s) para este óculos. Injetando dados da sala: {salaMaisRecente.Child("roomName").Value}");

                                   // === INJEÇÃO DE DADOS (Mantenha o seu código original aqui) ===
                                   configuracaoDaSala.NumBoys = int.Parse(salaMaisRecente.Child("numBoys").Value.ToString());
                                   configuracaoDaSala.NumGirls = int.Parse(salaMaisRecente.Child("numGirls").Value.ToString());
                                   configuracaoDaSala.NumDesks = int.Parse(salaMaisRecente.Child("numDesks").Value.ToString());

                                   int shapeInt = int.Parse(salaMaisRecente.Child("shape").Value.ToString());
                                   configuracaoDaSala.ClassShape = (ClassSettings.Shape)shapeInt;

                                   configuracaoDaSala.Rows = int.Parse(salaMaisRecente.Child("rows").Value.ToString());
                                   configuracaoDaSala.Cols = int.Parse(salaMaisRecente.Child("cols").Value.ToString());
                                   configuracaoDaSala.Radius = float.Parse(salaMaisRecente.Child("radius").Value.ToString());
                                   configuracaoDaSala.MaxDesksInSemiCircle = int.Parse(salaMaisRecente.Child("maxDesksInSemiCircle").Value.ToString());

                                   if (salaMaisRecente.HasChild("numTEA"))
                                       configuracaoDaSala.NumTEA = int.Parse(salaMaisRecente.Child("numTEA").Value.ToString());

                                   if (salaMaisRecente.HasChild("numADHD"))
                                       configuracaoDaSala.NumADHD = int.Parse(salaMaisRecente.Child("numADHD").Value.ToString());

                                   PlayerPrefs.SetString("session_configId", salaMaisRecente.Key);
                                   PlayerPrefs.SetString("session_participantId", salaMaisRecente.Child("activeParticipantId").Value.ToString());
                                   PlayerPrefs.SetString("session_facilitadorId", salaMaisRecente.Child("facilitadorId").Value.ToString());
                                   PlayerPrefs.SetString("session_instituicaoId", salaMaisRecente.Child("instituicaoId").Value.ToString());
                                   PlayerPrefs.SetString("session_roomName", salaMaisRecente.Child("roomName").Value.ToString());
                                   PlayerPrefs.Save();

                                   SceneManager.LoadScene("newClass");
                               }
                               else
                               {
                                   Debug.LogWarning($"<color=orange>[MENU]</color> Nenhuma sala associada ao óculos {idDoOculos}.");
                                   ReturnToMainMenu();
                                   MostrarErroFaltaDeSala("Nenhuma aula agendada para este dispositivo no momento.");
                               }
                           }
                           else
                           {
                               Debug.LogError("<color=red>[MENU]</color> A pasta 'classroom_configs' está vazia ou não existe.");
                               ReturnToMainMenu();
                               MostrarErroFaltaDeSala("Banco de dados vazio.");
                           }
                       }
                   });
    }

    private void MostrarErroFaltaDeSala(string textoDoErro)
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