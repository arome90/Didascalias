using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

[System.Serializable]
public class DataSystem
{
    public string hash = string.Empty;

    public int numStu;

    public ClassRoomVR.Age edad;

    public ClassRoomVR.StructureMode structureClass;

    public ClassRoomVR.GenerateMode mode;

    public ClassRoomVR.StudentInfo[] students;

    public int men;
    public int women;
}

//[System.Serializable]
//public class Student 
//{
//   public string nameStudent;
//   public int gender;
//   public bool hasDisability;
//   public int origin;
//   public int body;
//}


public class SaveSystem
{
    public static void SaveData(DataSystem data)
    {
        data.hash = string.Empty;
        data.hash = Hash(JsonUtility.ToJson(data));

        string json = JsonUtility.ToJson(data,true);
        string path = Application.persistentDataPath + "/save.json";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        File.WriteAllText(path, json);
    }

    public static DataSystem LoadData()
    {
        string path = Application.persistentDataPath + "/save.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            DataSystem data = JsonUtility.FromJson<DataSystem>(json);

            string hash = data.hash;
            data.hash = string.Empty;
            if (Hash(JsonUtility.ToJson(data)).Equals(hash))
            {
                return data;
            }
            else return null;
        }
        else { return null; }
    }


    public static string Hash(string data)
    {
        SHA256Managed mySha256 = new SHA256Managed();
        byte[] textToBytes = Encoding.UTF8.GetBytes(data);
        byte[] hashValue = mySha256.ComputeHash(textToBytes);
        return GetHexStringFromHash(hashValue);
    }

    private static string GetHexStringFromHash(byte[] hash)
    {
        string hexString = string.Empty;
        foreach (byte b in hash)
        {
            hexString += b.ToString("x2");
        }
        return hexString;
    }
}