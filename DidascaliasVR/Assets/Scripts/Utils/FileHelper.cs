using System;
using System.IO;
using UnityEngine;

public static class FileHelper
{
    /// <summary>
    /// Lee todo el contenido de un archivo de texto de forma segura sin dejar bloqueos de lectura.
    /// </summary>
    public static string GetTextFromFile(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"[FileHelper] El archivo no existe en la ruta: {path}");
            return string.Empty;
        }

        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FileHelper] Error al leer el archivo en '{path}': {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Guarda o sobreescribe un texto completo en un archivo (ideal para resúmenes).
    /// </summary>
    public static bool SaveToFile(string path, string content)
    {
        try
        {
            EnsureDirectoryExists(path);
            File.WriteAllText(path, content);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FileHelper] Error al guardar el archivo en '{path}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Añade una entrada al final de un archivo de log sin borrar el contenido previo.
    /// </summary>
    public static bool AppendLog(string path, string logMessage, bool addTimestamp = true)
    {
        try
        {
            EnsureDirectoryExists(path);

            string entry = addTimestamp
                ? $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {logMessage}"
                : logMessage;

            using (StreamWriter writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine(entry);
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FileHelper] Error al añadir log en '{path}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Garantiza que la carpeta contenedora de la ruta exista antes de intentar escribir.
    /// </summary>
    public static void EnsureDirectoryExists(string path)
    {
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}