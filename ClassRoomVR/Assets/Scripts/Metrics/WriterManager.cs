using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// WriterManager gestiona la escritura asíncrona y segura de archivos.
/// Usa el patrón singleton para garantizar una única instancia.
/// Mantiene múltiples StreamWriters para distintos archivos y asegura acceso seguro con locks.
/// </summary>
public class WriterManager : GenericSingleton<WriterManager>
{
    // Diccionario para guardar los StreamWriter por ruta de archivo.
    private Dictionary<string, StreamWriter> streamWriters
        = new Dictionary<string, StreamWriter>();

    // Diccionario de locks para asegurar acceso seguro a cada archivo.
    private Dictionary<string, object> lockObjects = new Dictionary<string, object>();

    // Lista de tareas pendientes de escritura para poder esperar su finalización al salir.
    private List<Task> pendingTasks = new List<Task>();

    // Lock general para modificar la lista de tareas pendientes.
    private object pendingTasksLock = new object();

    // Bandera para indicar si la aplicación está cerrando.
    private bool isQuitting = false;

    /// <summary>
    /// Crea o recupera un StreamWriter para la ruta especificada.
    /// Si el archivo ya está abierto, lo reutiliza.
    /// </summary>
    /// <param name="path">Ruta del archivo a abrir.</param>
    /// <returns>StreamWriter asociado a la ruta.</returns>
    public StreamWriter CreateStreamWriter(string path)
    {
        if (!streamWriters.ContainsKey(path))
        {
            StreamWriter writer = new StreamWriter(path, true); // 'true' para añadir al archivo existente
            streamWriters[path] = writer;
            lockObjects[path] = new object();
        }
        return streamWriters[path];
    }

    /// <summary>
    /// Escribe contenido en un archivo de forma asíncrona y thread-safe.
    /// Si la aplicación está cerrando, no realiza la escritura.
    /// </summary>
    /// <param name="path">Ruta del archivo.</param>
    /// <param name="content">Contenido a escribir (línea).</param>
    public async Task WriteToStreamWriter(string path, string content)
    {
        if (isQuitting) return;

        StreamWriter writer = CreateStreamWriter(path);
        object lockObject = lockObjects[path];

        Task writeTask = Task.Run(() =>
        {
            lock (lockObject)
            {
                writer.WriteLine(content);
                writer.Flush();
            }
        });

        lock (pendingTasksLock)
        {
            pendingTasks.Add(writeTask);
        }

        await writeTask;

        lock (pendingTasksLock)
        {
            pendingTasks.Remove(writeTask);
        }
    }

    /// <summary>
    /// Al cerrar la aplicación, espera a que terminen todas las tareas de escritura
    /// y cierra todos los archivos abiertos de manera segura.
    /// </summary>
    private void OnApplicationQuit()
    {
        isQuitting = true;
        lock (pendingTasksLock)
        {
            Task.WaitAll(pendingTasks.ToArray());
        }
        foreach (var writer in streamWriters.Values)
        {
            writer.Close();
        }
        streamWriters.Clear();
        lockObjects.Clear();
    }
}