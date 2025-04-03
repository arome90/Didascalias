using ClassRoomVR;
using OVR.OpenVR;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Utilities.Extensions;
using static OVRHaptics;

public class LoadingScene : MonoBehaviour
{
    [SerializeField]
    private string nextScene = "MainScene"; // Nombre de la escena principal
    [SerializeField]
    private TextMeshProUGUI textMeshPro;
    [SerializeField]
    private TextMeshProUGUI text2;
    [SerializeField]
    string[] files;

    int cont;
    bool reload;
    void Start()
    {
        cont = 0;
        reload = true;
        StartCoroutine(LoadConfig());
    }

    IEnumerator SetupGameFiles()
    {
        int N = files.Length;
        int cont = 0;
        foreach (string file in files)
        {
            text2.text = file;
            //string fileName = Path.GetFileName(file);
            string fileName = file;
            string persistentFile = Path.Combine(Application.persistentDataPath, fileName);
            if (!File.Exists(persistentFile) || reload) // Solo copia si no existe en persistentDataPath
            {
                Debug.Log($"Copiando archivo: {fileName}");
                yield return CopyFileToPersistentDataPath(fileName);
            }
            else
            {
                Debug.Log($"Archivo ya copiado: {fileName}, saltando el proceso.");
            }
            cont++;
            int percentage = cont / N * 100;
            textMeshPro.text = percentage.ToString() + "%";
        }

        textMeshPro.text = "100%";

        // Simulación de carga adicional
        yield return new WaitForSeconds(1f);

        // Cargar la siguiente escena
        SceneManager.LoadScene(nextScene);
    }

    IEnumerator LoadConfig()
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "config.json");
        string destinationPath = Path.Combine(Application.persistentDataPath, "config.json");
        if (!File.Exists(destinationPath)) // Solo copia si no existe en persistentDataPath
        {
            UnityWebRequest request = UnityWebRequest.Get(sourcePath);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllText(destinationPath, request.downloadHandler.text);
                Debug.Log($"Archivo copiado: {destinationPath}");
            }
            else
            {
                Debug.LogError($"Error al copiar {"config.json"}: {request.error}");
                yield break;
            }
        }

        var dictionary = LoadManager.Instance.LoadDataFromJson<string, Dictionary<string, object>>(destinationPath);
        if (dictionary == null || !LoadManager.Instance.SaveObject("config", dictionary))
        {
            Debug.LogError("Failed to load config.json file");
            yield break;
        }

        Dictionary<string, Dictionary<string, object>> config_ = null;
        if (LoadManager.Instance.GetObject("General", ref config_))
        {
            if (config_.TryGetValue("General", out var innerDict))
            {
                if (innerDict.TryGetValue("reload", out var value))
                {
                    if (value.GetType() == typeof(bool)) reload = (bool)value;
                }

            }
        }

        StartCoroutine(SetupGameFiles());

    }

    IEnumerator CopyFileToPersistentDataPath(string fileName)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        string destinationPath = Path.Combine(Application.persistentDataPath, fileName);

        UnityWebRequest request = UnityWebRequest.Get(sourcePath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllText(destinationPath, request.downloadHandler.text);
            Debug.Log($"Archivo copiado: {destinationPath}");
        }
        else
        {
            Debug.LogError($"Error al copiar {fileName}: {request.error}");
        }
    }
}