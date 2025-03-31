using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;

[System.Serializable]
public class DataSystem2
{
    /// <summary>
    /// Hash del archivo JSON para verificación de integridad.
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Número de estudiantes en la clase.
    /// </summary>
    public int NumStudents { get; set; }

    /// <summary>
    /// Edad de los estudiantes.
    /// </summary>
    public Age2 Age { get; set; }

    /// <summary>
    /// Estructura del aula.
    /// </summary>
    public StructureMode2 StructureMode { get; set; }

    /// <summary>
    /// Modo de generación de datos.
    /// </summary>
    public GenerateMode2 Mode { get; set; }

    /// <summary>
    /// Información de los estudiantes.
    /// </summary>
    public ClassRoomVR.StudentInfo2[] Students { get; set; }

    /// <summary>
    /// Contador de hombres.
    /// </summary>
    public int MenCount { get; set; }

    /// <summary>
    /// Contador de mujeres.
    /// </summary>
    public int WomenCount { get; set; }
}

public static class SaveSystem2
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "save.json");

    /// <summary>
    /// Guarda los datos en un archivo JSON.
    /// </summary>
    /// <param name="data">Datos a guardar.</param>
    public static void SaveData(DataSystem2 data)
    {
        try
        {
            data.Hash = CalculateHash(JsonUtility.ToJson(data));
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error al guardar datos: {ex.Message}");
        }
    }

    /// <summary>
    /// Carga los datos desde un archivo JSON.
    /// </summary>
    /// <returns>Datos cargados o null si hay un error.</returns>
    public static DataSystem2 LoadData()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("No se encontró el archivo de guardado.");
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            DataSystem2 data = JsonUtility.FromJson<DataSystem2>(json);

            string originalHash = data.Hash;
            data.Hash = string.Empty;

            return CalculateHash(JsonUtility.ToJson(data)).Equals(originalHash) ? data : null;
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error al cargar datos: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Calcula un hash SHA-256 de los datos JSON.
    /// </summary>
    /// <param name="data">Datos en formato JSON.</param>
    /// <returns>Hash en formato hexadecimal.</returns>
    private static string CalculateHash(string data)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return GetHexStringFromHash(hashBytes);
        }
    }

    /// <summary>
    /// Convierte un arreglo de bytes de hash en una cadena hexadecimal.
    /// </summary>
    /// <param name="hash">Arreglo de bytes del hash.</param>
    /// <returns>Cadena hexadecimal que representa el hash.</returns>
    private static string GetHexStringFromHash(byte[] hash)
    {
        StringBuilder hexBuilder = new StringBuilder(hash.Length * 2);
        foreach (byte b in hash)
        {
            hexBuilder.Append(b.ToString("x2"));
        }
        return hexBuilder.ToString();
    }
}
