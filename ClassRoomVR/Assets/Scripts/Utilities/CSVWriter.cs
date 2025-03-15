using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using UnityEngine;

namespace ClassRoomVR
{
    public class CSVWriter : MonoBehaviour
    {
        private StreamWriter writer_1;
        private StreamWriter writer_2;
        [SerializeField]
        private float snapshotTime;

        private Dictionary<string, Student> students;

        private ClimateManager climateManager;

        void Start()
        {
            climateManager= ClimateManager.Instance;
            inicialWrite();
            InvokeRepeating("WriteData", 0.0f, snapshotTime);
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
        void WriteData()
        {
            foreach (KeyValuePair<string, Student> kvp in students)
            {
                string attentionString = kvp.Value.getAttention().ToString(CultureInfo.InvariantCulture);
                string emotionsString = string.Join(",", Array.ConvertAll(kvp.Value.GetEmotions(), e => e.ToString(CultureInfo.InvariantCulture)));
                string i = kvp.Key + "," + kvp.Value.getAnimatorAction() + "," + attentionString + "," + emotionsString + "," + Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture);
                writer_1.WriteLine(i);
            }
            
            writer_2.WriteLine(climateManager.environmentalClimate.ToString(CultureInfo.InvariantCulture));
        }

        void OnDestroy()
        {
            writer_1.Close();
            writer_2.Close();
        }
    }
}
