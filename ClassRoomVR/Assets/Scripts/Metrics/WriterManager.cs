using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class WriterManager : GenericSingleton<WriterManager>
{
    private Dictionary<string, StreamWriter> streamWriters = new Dictionary<string, StreamWriter>();
    private Dictionary<string, object> lockObjects = new Dictionary<string, object>();
    private List<Task> pendingTasks = new List<Task>();
    private object pendingTasksLock = new object();
    private bool isQuitting = false;

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