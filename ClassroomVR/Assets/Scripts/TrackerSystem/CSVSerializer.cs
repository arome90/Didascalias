using UnityEngine;
using System.IO;

namespace ClassRoomVR
{
    public class CSVSerializer : MonoBehaviour
    {
        private string extension = ".csv";
        private string file = "session_";
        private string folderName = "/Logs/";
        private int sessionID = 0;

        private string filename = "";
        private string fullpath = "";

        // Constructor
        public CSVSerializer() {
            // Creamos el directorio si no existe
            string directory = Application.dataPath + folderName;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        // Inicializa el archivo que se usara durante la escena
        public void iniFile(string sceneName) {
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
            w.Write("Emocion, Mano Izquierda, Mano Derecha, Pierna Izquierda, Pierna Derecha, Cabeza, Direccion de vista, Apertura mano Izq, Apertura mano Der\n");
            //w.Write(System.DateTime.Now.ToString() + "\n");
            w.Close();
        }

        // Deja interniliado (usando para el cambio de intervalo)
        public void writeWhiteSpaces()
        {
            using (StreamWriter w = File.AppendText(fullpath))
            {
                w.Write("\n");
                w.Write("\n");
            }
        }

        // Guarda en el archivo que se este usando la info
        public bool saveData(string data)
        {
            bool result = false;

            // Cambios respectivos para buena visualizacion
            data = data.Replace(",", "/");
            data = data.Replace(";", ",");
            data += "\n";

            // Guardamos el data
            using (StreamWriter w = File.AppendText(fullpath))
            {
                w.Write(data);
                result = true;
            }
            return result;
        }
    }   // end CsvSerializer
}