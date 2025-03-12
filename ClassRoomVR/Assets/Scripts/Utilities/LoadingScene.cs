using OVR.OpenVR;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

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
    void Start()
    {
        StartCoroutine(SetupGameFiles());
    }

    IEnumerator SetupGameFiles()
    {
        int N= files.Length;
        int cont= 0;
        foreach (string file in files)
        {
            text2.text = file;
            //string fileName = Path.GetFileName(file);
            string fileName =file;
            string persistentFile = Path.Combine(Application.persistentDataPath, fileName);

            if (!File.Exists(persistentFile)) // Solo copia si no existe en persistentDataPath
            {
                Debug.Log($"Copiando archivo: {fileName}");
                yield return CopyFileToPersistentDataPath(fileName);
            }
            else
            {
                Debug.Log($"Archivo ya copiado: {fileName}, saltando el proceso.");
            }
            cont++;
            int percentage= cont/N * 100;
            textMeshPro.text = percentage.ToString() + "%";
        }

        textMeshPro.text = "100%";
        // Simulación de carga adicional
        yield return new WaitForSeconds(1f);

        // Cargar la siguiente escena
        SceneManager.LoadScene(nextScene);
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