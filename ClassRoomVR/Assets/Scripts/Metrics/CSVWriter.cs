using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utilities.Extensions;

namespace ClassRoomVR
{
    /// <summary>
    /// CSVWriter gestiona la recopilación periódica de datos de estudiantes y ambiente,
    /// almacenándolos en archivos CSV. 
    /// </summary>
    public class CSVWriter : MonoBehaviour
    {
        [SerializeField]
        private float snapshotInterval;
        [SerializeField]
        private int maxSnapshots=100;
        private int snapshotCounter;

        private Dictionary<string, Student> students;
        private ClimateManager climateManager;

        private StringBuilder studentDataBuffer;
        private StringBuilder classroomDataBuffer;

        private string studentDataPath;
        private string classroomDataPath;

        private const int MAX_BUFFER_SIZE = 10000; 

        private bool initialized=false;

        void Start()
        {
            //LoadConfiguration();

            //if (!isActiveAndEnabled) return;

            snapshotCounter = 0;
            climateManager = ClimateManager.Instance;

            studentDataBuffer = new StringBuilder(MAX_BUFFER_SIZE);
            classroomDataBuffer = new StringBuilder(MAX_BUFFER_SIZE);

            _=InitializeFilesAsync();

            InvokeRepeating(nameof(CaptureDataSnapshot), snapshotInterval, snapshotInterval);
        }

        /// <summary>
        /// Carga configuraciones desde archivo externo.
        /// </summary>
        void LoadConfiguration()
        {
            Dictionary<string, Dictionary<string, object>> config = null;
            if (LoadManager.Instance.GetObject("config", ref config))
            {
                if (config.TryGetValue("csv", out var csvConfig))
                {
                    if (csvConfig.TryGetValue("maxSnapshot", out var maxSnapValue) && maxSnapValue is int)
                        maxSnapshots = (int)maxSnapValue;

                    if (csvConfig.TryGetValue("snapshotTime", out var snapshotTimeValue) && snapshotTimeValue is float)
                        snapshotInterval = (float)snapshotTimeValue;

                    if (csvConfig.TryGetValue("activate", out var activateValue) && activateValue is bool)
                        this.SetActive((bool)activateValue);
                }
            }
        }

        /// <summary>
        /// Inicializa los archivos CSV con encabezados.
        /// </summary>
        private async Task InitializeFilesAsync()
        {
            string creationTime = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string folderPath = Path.Combine(Application.persistentDataPath, "CSV");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            studentDataPath = Path.Combine(folderPath, $"{creationTime}_students_data.csv");
            classroomDataPath = Path.Combine(folderPath, $"{creationTime}_classroom_data.csv");
            string initialDataPath = Path.Combine(folderPath, $"{creationTime}_initial_data.csv");

            students = ClassManager.Instance.getStudents();

            // Preparar todos los datos iniciales en un solo StringBuilder
            var initialDataBuilder = new StringBuilder();
            initialDataBuilder.AppendLine("Name," + string.Join(",", Enum.GetNames(typeof(PersonalityType))));

            foreach (var kvp in students)
            {
                string traits = string.Join(",", Array.ConvertAll(kvp.Value.GetTraits(), t => t.ToString(CultureInfo.InvariantCulture)));
                initialDataBuilder.AppendLine($"{kvp.Key},{traits}");
            }

            // Escrituras asíncronas en paralelo, sin bloquear el hilo principal
            await Task.WhenAll(
                WriterManager.Instance.WriteToStreamWriter(
                    studentDataPath,
                    "Name,Actions,Attention," + string.Join(",", Enum.GetNames(typeof(EmotionType))) + ",Time"
                ),
                WriterManager.Instance.WriteToStreamWriter(classroomDataPath, "EnvironmentalClimate,Time"),
                WriterManager.Instance.WriteToStreamWriter(initialDataPath, initialDataBuilder.ToString())
            );

            // Cierre explícito del archivo inicial
            WriterManager.Instance.CloseStreamWriter(initialDataPath);
            initialized = true;
        }


        /// <summary>
        /// Captura instantáneas periódicas de datos.
        /// </summary>
        void CaptureDataSnapshot()
        {
            snapshotCounter++;

            foreach (var kvp in students)
            {
                string attention = kvp.Value.getAttention().ToString(CultureInfo.InvariantCulture);
                string emotions = string.Join(",", Array.ConvertAll(kvp.Value.GetEmotions(), e => e.ToString(CultureInfo.InvariantCulture)));
                studentDataBuffer.AppendLine($"{kvp.Key},{kvp.Value.getAnimatorAction()},{attention},{emotions},{Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture)}");
            }

            classroomDataBuffer.AppendLine($"{climateManager.environmentalClimate.ToString(CultureInfo.InvariantCulture)},{Time.realtimeSinceStartup.ToString(CultureInfo.InvariantCulture)}");

            if (!initialized) return;

            if (snapshotCounter >= maxSnapshots || studentDataBuffer.Length > MAX_BUFFER_SIZE)
            {
                _ = FlushDataBuffersAsync();
                snapshotCounter = 0;
            }
        }

        /// <summary>
        /// Escribe los datos del buffer usando WriterManager y limpia los buffers.
        /// </summary>
        async Task FlushDataBuffersAsync()
        {
            var studentData = studentDataBuffer.ToString();
            var classroomData = classroomDataBuffer.ToString();

            studentDataBuffer.Clear();
            classroomDataBuffer.Clear();

            await Task.WhenAll(
                WriterManager.Instance.WriteToStreamWriter(studentDataPath, studentData),
                WriterManager.Instance.WriteToStreamWriter(classroomDataPath, classroomData)
            );
        }

        /// <summary>
        /// Asegura que todos los datos se guarden al destruir el objeto.
        /// </summary>
        async void OnDestroy()
        {
            await FlushDataBuffersAsync();
            WriterManager.Instance.CloseStreamWriter(studentDataPath);
            WriterManager.Instance.CloseStreamWriter(classroomDataPath);
        }
    }
}
