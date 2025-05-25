using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class GenericCSVWriter : MonoBehaviour
{
    [SerializeField] private float snapshotTime = 5f;
    [SerializeField] private int maxSnapshot = 50;
    [SerializeField] private string filePrefix = "data";
    [SerializeField] private List<string> columns;

    private int snapshotCount;
    private Queue<List<string>> rowQueue;
    private string filePath;

    void Start()
    {
        rowQueue = new Queue<List<string>>();
        snapshotCount = 0;
        InitFile();
        InvokeRepeating(nameof(RegisterData), 0f, snapshotTime);
    }

    private void InitFile()
    {
        string creationTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string folderPath = Path.Combine(Application.persistentDataPath, "CSV");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        filePath = Path.Combine(folderPath, $"{creationTime}_{filePrefix}.csv");

        // Escribe cabecera
        WriterManager.Instance.WriteToStreamWriter(filePath, string.Join(",", columns));
    }

    /// <summary>
    /// Agrega una fila de datos para guardar.
    /// </summary>
    public void EnqueueRow(List<string> row)
    {
        rowQueue.Enqueue(row);
    }

    /// <summary>
    /// Se invoca periódicamente para registrar datos.
    /// </summary>
    private void RegisterData()
    {
        snapshotCount++;
        // Aquí deberías obtener los datos de donde corresponda, por ejemplo:
        // var row = GetRowFromDataSource();
        // En este ejemplo, solo se simula una fila dummy:
        var dummyRow = new List<string>();
        foreach (var col in columns)
            dummyRow.Add(UnityEngine.Random.Range(0f, 1f).ToString(CultureInfo.InvariantCulture));
        EnqueueRow(dummyRow);

        if (snapshotCount >= maxSnapshot)
        {
            SaveData();
            snapshotCount = 0;
        }
    }

    /// <summary>
    /// Guarda todas las filas pendientes en el archivo.
    /// </summary>
    private async void SaveData()
    {
        while (rowQueue.Count > 0)
        {
            var row = rowQueue.Dequeue();
            string line = string.Join(",", row);
            await WriterManager.Instance.WriteToStreamWriter(filePath, line);
        }
    }

    private void OnDestroy()
    {
        SaveData();
    }

}
