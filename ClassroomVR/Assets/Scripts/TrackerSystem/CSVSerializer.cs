using UnityEngine;
using System.IO;

namespace ClassRoomVR {
    public class CSVSerializer : MonoBehaviour
    {
        private static string extension = ".csv";
        private static string file = "session_";
        private static string folderName = "/Logs/";
        private static int sessionID = 0;

        private static string filename = "";
        private static string fullpath = "";

        private static string recognicedWord = "";

        // Inicializa el archivo que se usara durante la escena
        public static void iniFile(string sceneName) {
            recognicedWord = "";
            // Creamos el directorio si no existe
            string directory = Application.dataPath + folderName;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            // Ubicacion y nombre del archivo
            filename = file + sceneName + "_" + sessionID + extension;
            fullpath = Application.dataPath + folderName + filename;
            // Cambiamos la id para k no coincida con ningun otro archivo ya existente
            while (File.Exists(fullpath))
            {
                sessionID++;
                filename = file + sceneName + "_" + sessionID.ToString() + extension;
                fullpath = Application.dataPath + folderName + filename;
            }

            File.Create(fullpath).Close();
            StreamWriter w = File.AppendText(fullpath);
            w.Write("Emocion, Mano Izquierda, Mano Derecha, Pierna Izquierda, Pierna Derecha, Cabeza, Direccion de vista, Apertura mano Izq, Apertura mano Der,Distancia,\n");
            //w.Write(System.DateTime.Now.ToString() + "\n");
            w.Close();
        }

        // Guarda en el archivo que se este usando la info en cualquier momento
        public static bool saveData(string data)
        {
            bool result = false;

            // Guardamos el data
            using (StreamWriter w = File.AppendText(fullpath))
            {
                w.Write(data);
                result = true;
            }
            return result;
        }
        //Almacena los datos en la lista para escribirla al final
        public static void storeData(string data)
        {
            recognicedWord = data;
        }
        //Salva la informacion de la palabra reconocida al final del archivo
        public static void saveRcogniceWord()
        {
            if (recognicedWord == "") recognicedWord = "No se ha detectado ninguna palabra";
            saveData(recognicedWord);
        }
    }   // end CsvSerializer
}