using ClassRoomVR;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Gestiona la carga de archivos necesarios y la transición entre escenas en la aplicación.
/// Muestra el progreso de la carga usando componentes TextMeshProUGUI.
/// </summary>
public class LoadingScene : MonoBehaviour
{
    [SerializeField]
    private string nextScene = "MainScene"; // Nombre de la escena principal
    [SerializeField]
    private TextMeshProUGUI textMeshPro; // Texto para mostrar el porcentaje de progreso
    [SerializeField]
    private TextMeshProUGUI text2; // Texto para mostrar el nombre del archivo actualmente cargado.
    [SerializeField]
    string[] files; // Lista de archivos que deben copiarse a la ruta persistente.

    private int cont;
    bool reload;

    void Start()
    {
        cont = 0;
        reload = true;
        StartCoroutine(LoadConfig());
    }
    /// <summary>
    /// Copia todos los archivos necesarios a la ruta persistente, mostrando el progreso.
    /// </summary>
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
                Debug.Log($"Copying file: {fileName}");
                yield return CopyFileToPersistentDataPath(fileName);
            }
            else
            {
                Debug.Log($"File already copied: {fileName}, skipping process.");
            }
            cont++;
            int percentage = cont / N * 100;
            textMeshPro.text = percentage.ToString() + "%";
        }

        textMeshPro.text = "100%";

        // Simulación de carga adicional
        yield return new WaitForSeconds(1f);

        // Cargar la siguiente escena
        //SceneManager.LoadScene(nextScene);
        //Con transición
        //SceneTransitionManager.Singleton.GoToSceneAsync(SceneManager.GetSceneByName(nextScene).buildIndex);
        SceneTransitionManager.Singleton.GoToSceneAsync(1);
    }

    /// <summary>
    /// Carga y verifica el archivo de configuración principal antes de continuar.
    /// </summary>
    IEnumerator LoadConfig()
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "config.json");
        string destinationPath = Path.Combine(Application.persistentDataPath, "config.json");
        // Si ya existe el archivo en la ruta persistente, intentamos cargarlo y revisar la clave 'use'        if (File.Exists(destinationPath))
        {
            var dictionary1 = LoadManager.Instance.LoadDataFromJson<string, Dictionary<string, object>>(destinationPath);
            if (dictionary1 == null)
            {
                Debug.LogError("Failed to load config.json file");
                yield break;
            }
           
            if (dictionary1.TryGetValue("General", out var innerDict))
            {
                if (innerDict.TryGetValue("use", out var value))
                {
                    if (value.GetType() == typeof(bool)) reload = !(bool)value;
                }

            }
        }

        // Si el archivo no existe o se requiere recarga, lo copiamos desde los assets
        if (!File.Exists(destinationPath) || reload) 
        {
            UnityWebRequest request = UnityWebRequest.Get(sourcePath);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllText(destinationPath, request.downloadHandler.text);
                Debug.Log($"File copied: {destinationPath}");
            }
            else
            {
                Debug.LogError($"Error copying config.json: {request.error}");
                yield break;
            }
        }

        Dictionary<string, Dictionary<string, object>> config_ = LoadManager.Instance.LoadDataFromJson<string, Dictionary<string, object>>(destinationPath);
        if (config_ == null || !LoadManager.Instance.SaveObject("config", config_))
        {
            Debug.LogError("Failed to load config.json file");
            yield break;
        }
                    
        StartCoroutine(SetupGameFiles());
    }

    /// <summary>
    /// Copia un archivo específico desde los assets a la ruta persistente usando UnityWebRequest.
    /// </summary>
    /// <param name="fileName">Nombre del archivo a copiar.</param>
    IEnumerator CopyFileToPersistentDataPath(string fileName)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, fileName);
        string destinationPath = Path.Combine(Application.persistentDataPath, fileName);

        UnityWebRequest request = UnityWebRequest.Get(sourcePath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllText(destinationPath, request.downloadHandler.text);
            Debug.Log($"File copied: {destinationPath}");
        }
        else
        {
            Debug.LogError($"Error copying {fileName}: {request.error}");
        }
    }
}