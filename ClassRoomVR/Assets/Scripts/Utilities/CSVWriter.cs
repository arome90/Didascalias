using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Utilities.Extensions;

namespace ClassRoomVR
{
    public class CSVWriter : MonoBehaviour
    {
        private StreamWriter writer_1;
        private StreamWriter writer_2;
        [SerializeField]
        private float snapshotTime;
        [SerializeField] 
        private int maxSnapshot;
        private int snapshotCount;

        private Dictionary<string, Student> students;

        private ClimateManager climateManager;

        private Queue<string> queue_1;
        private Queue<string> queue_2;

        void Start()
        {
            loadConfig();

            if (!this.isActiveAndEnabled) return;
            snapshotCount = 0;
            climateManager = ClimateManager.Instance;
            queue_1 = new Queue<string>();
            queue_2 = new Queue<string>();
            inicialWrite();
            InvokeRepeating("RegisterData", 0.0f, snapshotTime);
        }

        void loadConfig()
        {
            Dictionary<string, Dictionary<string, object>> config_ = null;
            if (LoadManager.Instance.GetObject("config", ref config_))
            {
                if (config_.TryGetValue("csv", out var innerDict))
                {
                    if (innerDict.TryGetValue("maxSnapshot", out var value))
                    {
                        if (value.GetType() == typeof(int)) maxSnapshot = (int)value;
                    }
                    if (innerDict.TryGetValue("snapshotTime", out var value_2))
                    {
                        if (value_2.GetType() == typeof(float)) snapshotTime = (float)value_2;
                    }
                    if (innerDict.TryGetValue("activate", out var value_3))
                    {
                        if (value_3.GetType() == typeof(bool)) this.SetActive((bool)value_3);
                    }
                }
               
            }
        }

        void inicialWrite()
        {
            string creationTime = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); // Obtiene la hora actual
            string folderPath = Path.Combine(Application.persistentDataPath, "CSV");
            // Crear la carpeta si no existe
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath_1 = Path.Combine(folderPath, $"{creationTime}_students_datas.csv");
            string filePath_2 = Path.Combine(folderPath, $"{creationTime}_classroom_datas.csv");
            string filePath_3 = Path.Combine(folderPath, $"{creationTime}_ini_datas.csv");

            writer_1 = new StreamWriter(filePath_1, true); // 'true' para añadir datos sin sobrescribir
            writer_2 = new StreamWriter(filePath_2, true); // 'true' para añadir datos sin sobrescribir
            StreamWriter writer_3=new StreamWriter(filePath_3, true);

            students = ClassManager.Instance.getStudents();

            string w1 = "Name,Actions,Attention," + string.Join(",", Enum.GetNames(typeof(EmotionType))) + ",Time";
            writer_1.WriteLine(w1);

            writer_2.WriteLine("environmentalClimate,Time");

            string w3 = "Name," + string.Join(",", Enum.GetNames(typeof(PersonalityType)));
            writer_3.WriteLine(w3);


            foreach (KeyValuePair<string, Student> kvp in students)
            {
                string i = kvp.Key + "," + string.Join(",", Array.ConvertAll(kvp.Value.GetTraits(), e => e.ToString(CultureInfo.InvariantCulture)));
                writer_3.WriteLine(i);
            }

            writer_1.Flush();
            writer_2.Flush();

            writer_3.Flush();
            writer_3.Close();
        }
        void RegisterData()
        {
            snapshotCount++;
            foreach (KeyValuePair<string, Student> kvp in students)
            {
                string attentionString = kvp.Value.getAttention().ToString(CultureInfo.InvariantCulture);
                string emotionsString = string.Join(",", Array.ConvertAll(kvp.Value.GetEmotions(), e => e.ToString(CultureInfo.InvariantCulture)));
                string i = kvp.Key + "," + kvp.Value.getAnimatorAction() + "," + attentionString + "," + emotionsString + "," + Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture);
                queue_1.Enqueue(i);
            }
            string i2= climateManager.environmentalClimate.ToString(CultureInfo.InvariantCulture) + "," + Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture);
            queue_2.Enqueue(i2);
            if(snapshotCount > maxSnapshot)
            {
                saveData();
                snapshotCount = 0;
            }
        }

        void saveData()
        {
            while (queue_1.Count > 0)
            {
                writer_1.WriteLine(queue_1.Dequeue());
            }
            while (queue_2.Count > 0)
            {
                writer_2.WriteLine(queue_2.Dequeue());
            }
            writer_1.Flush();
            writer_2.Flush();
        }

        void OnDestroy()
        {
            writer_1.Flush();
            writer_2.Flush();
            writer_1.Close();
            writer_2.Close();
        }
    }
}
