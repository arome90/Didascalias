using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;
using System;

public class RegistroOculos : MonoBehaviour
{
    [Header("Interface")]
    [Tooltip("Arraste o Input Field onde o usuário digita o código")]
    public TMP_InputField inputCodigo;

    [Tooltip("Arraste um TextMeshPro para mostrar as mensagens de erro ou sucesso")]
    public TextMeshProUGUI textoFeedback;

    public void ConectarDispositivo()
    {
        // Força o código da universidade para testes diretos
        string codigoInstituicao = inputCodigo.text;

        textoFeedback.text = "<color=white>Verificando código...</color>";

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("instituicoes").Child(codigoInstituicao).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                textoFeedback.text = "<color=red>Erro de conexão com a rede.</color>";
                Debug.LogError($"[Firebase] Erro na busca: {task.Exception}");
                return;
            }
            if (task.IsCompleted && task.Result.Exists)
            {
                RegistrarOculosNaInstituicao(codigoInstituicao);
            }
            else
            {
                textoFeedback.text = "<color=red>Código não encontrado.</color>";
            }
        });
    }

    private void RegistrarOculosNaInstituicao(string codigoInstituicao)
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        string modelo = SystemInfo.deviceModel;

        // Formata a data para o padrão ISO 8601
        string dataAtual = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        DatabaseReference oculosRef = dbRef.Child("instituicoes").Child(codigoInstituicao).Child("oculos").Child(deviceID);

        // Preenche os dados
        oculosRef.Child("Oculos_id").SetValueAsync(deviceID);
        oculosRef.Child("dataRegistro").SetValueAsync(dataAtual);
        oculosRef.Child("instituicaoId").SetValueAsync(codigoInstituicao);

        // O último SetValueAsync carrega a ação de finalização para dar o feedback positivo
        oculosRef.Child("modelo").SetValueAsync(modelo).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                textoFeedback.text = "<color=red>Erro ao salvar os dados do dispositivo.</color>";
            }
            else if (task.IsCompleted)
            {
                textoFeedback.text = "<color=#00FF00>Dispositivo conectado com sucesso!</color>";

                // Salva o código localmente para a Unity lembrar a qual instituição esse óculos pertence
                PlayerPrefs.SetString("InstituicaoID", codigoInstituicao);
                PlayerPrefs.Save();
            }
        });
    }
}