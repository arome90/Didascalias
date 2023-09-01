using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;

[System.Serializable]
public class DataSystem
{
    public string Hash { get; set; } = string.Empty;

    public int NumStudents { get; set; }

    public Age Age { get; set; }

    public StructureMode StructureMode { get; set; }

    public GenerateMode Mode { get; set; }

    public ClassRoomVR.StudentInfo[] Students { get; set; }

    public int MenCount { get; set; }
    public int WomenCount { get; set; }
}

public class SaveSystem
{
    public static void SaveData(DataSystem data)
    {
        data.Hash = string.Empty;
        data.Hash = CalculateHash(JsonUtility.ToJson(data));

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, "save.json");
        File.WriteAllText(path, json);
    }

    public static DataSystem LoadData()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            DataSystem data = JsonUtility.FromJson<DataSystem>(json);

            string hash = data.Hash;
            data.Hash = string.Empty;
            if (CalculateHash(JsonUtility.ToJson(data)).Equals(hash))
            {
                return data;
            }
        }

        return null;

    }

    public static string CalculateHash(string data)
    {
        using (SHA256Managed sha256 = new SHA256Managed())
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(data);
            byte[] hashBytes = sha256.ComputeHash(textBytes);
            return GetHexStringFromHash(hashBytes);
        }
    }

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
