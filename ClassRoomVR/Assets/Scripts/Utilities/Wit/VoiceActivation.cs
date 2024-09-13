using System.Collections.Generic;
using UnityEngine;
using Meta.WitAi;
using TMPro;
using MathNet.Numerics.Statistics;
using Oculus.Voice;

namespace ClassRoomVR
{
    /// <summary>
    /// Controla la activación de comandos de voz y la interacción con los estudiantes en base al análisis de audio.
    /// </summary>
    public class VoiceActivation : MonoBehaviour
    {
        [SerializeField] private AppVoiceExperience _appVoiceExperience; // Experiencia de voz para interacción con el asistente
        [SerializeField] private TextMeshProUGUI _textMeshPro; // Componente para mostrar texto en la interfaz de usuario

        private StudentsController _studentsController; // Controlador de estudiantes
        private List<Student> _selectedStudents; // Lista de estudiantes seleccionados
        private List<float> _volumeList; // Lista de niveles de volumen capturados
        private string _currentText; // Texto actual transcrito
        private bool _greetingsSentInitial; // Indica si se han enviado saludos iniciales

        /// <summary>
        /// Método que se ejecuta al inicializar el objeto.
        /// </summary>
        private void Awake()
        {
            InitializeComponents();
            InitializeVoiceExperience();
        }

        /// <summary>
        /// Inicializa los componentes y variables necesarias para la activación de voz.
        /// </summary>
        private void InitializeComponents()
        {
            _studentsController = ClassManager.Instance.GetStudentsController();
            _selectedStudents = new List<Student>();
            _volumeList = new List<float>();
            _currentText = string.Empty;
            _greetingsSentInitial = false;
        }

        /// <summary>
        /// Inicializa la experiencia de voz y establece los eventos correspondientes.
        /// </summary>
        private void InitializeVoiceExperience()
        {
            if (_appVoiceExperience == null)
            {
                Debug.LogError("AppVoiceExperience no está asignado.");
                return;
            }

            GameManager.Instance.SetVoiceExperience(this);

            var voiceEvents = _appVoiceExperience.VoiceEvents;

            voiceEvents.OnComplete.AddListener((a) => _appVoiceExperience.Activate());
            voiceEvents.OnError.AddListener(HandleVoiceError);
            voiceEvents.OnResponse.AddListener(UpdateClass);
            voiceEvents.OnValidatePartialResponse.AddListener(OnValidatePartialResponse);
            voiceEvents.OnFullTranscription.AddListener(UpdateTextMeshPro);
            voiceEvents.OnMicAudioLevelChanged.AddListener(OnMicLevelChanged);
            voiceEvents.OnMicStartedListening.AddListener(() => _volumeList.Clear());
        }

        /// <summary>
        /// Maneja los errores relacionados con la activación por voz.
        /// </summary>
        /// <param name="error">Código del error.</param>
        /// <param name="message">Mensaje relacionado al error.</param>
        private void HandleVoiceError(string error, string message)
        {
            _appVoiceExperience.Activate();
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("No hay conexión a Internet.");
                GameManager.Instance.Pause(true);
            }
        }

        /// <summary>
        /// Activa o desactiva el texto mostrado en pantalla.
        /// </summary>
        /// <param name="active">True para activar, false para desactivar.</param>
        public void ActiveText(bool active)
        {
            if (_textMeshPro != null)
            {
                _textMeshPro.transform.parent.gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// Activa la experiencia de voz.
        /// </summary>
        public void Activate()
        {
            _appVoiceExperience?.Activate();
        }

        /// <summary>
        /// Actualiza el texto mostrado en el componente TextMeshPro.
        /// </summary>
        /// <param name="transcribedText">Texto transcrito.</param>
        private void UpdateTextMeshPro(string transcribedText)
        {
            _currentText = transcribedText;
            if (_textMeshPro != null)
            {
                _textMeshPro.text = transcribedText;
            }
        }

        /// <summary>
        /// Establece el nivel de audio en base a la media de los niveles de volumen capturados.
        /// </summary>
        private void SetLevelAudio()
        {
            if (_volumeList.Count == 0) return;

            double averageVolume = _volumeList.Mean();
            TalkMode mode;

            if (averageVolume > -30)
            {
                mode = TalkMode.Disrespect;
                Debug.Log($"Shouting: {(int)averageVolume}");
            }
            else if (averageVolume < -50)
            {
                mode = TalkMode.Good;
                Debug.Log($"Whispering: {(int)averageVolume}");
            }
            else
            {
                mode = TalkMode.Normal;
                Debug.Log($"Normal: {(int)averageVolume}");
            }

            _studentsController.SetMode(mode);
        }

        /// <summary>
        /// Evento que se activa cuando cambia el nivel de audio del micrófono.
        /// </summary>
        /// <param name="audioLevel">Nivel de audio en flotante.</param>
        private void OnMicLevelChanged(float audioLevel)
        {
            if (audioLevel <= 0) return;

            float db = 20 * Mathf.Log10(audioLevel);
            if (db > -65)
            {
                _volumeList.Add(db);
            }
        }

        /// <summary>
        /// Valida respuestas parciales de reconocimiento de voz y selecciona a los estudiantes mencionados.
        /// </summary>
        /// <param name="sessionData">Datos de la sesión de voz.</param>
        private void OnValidatePartialResponse(Meta.WitAi.Data.VoiceSession sessionData)
        {
            string[] names = sessionData.response.GetAllEntityValues("wit$contact:student");

            if (names == null || names.Length == 0) return;

            foreach (string name in names)
            {
                if (TryGetStudent(name, out Student student))
                {
                    if (_selectedStudents.Count == 0)
                    {
                        _selectedStudents.Clear();
                    }
                    _selectedStudents.Add(student);
                    _studentsController.HandleCall(student);
                }
            }
        }

        /// <summary>
        /// Intenta obtener un estudiante por su nombre.
        /// </summary>
        /// <param name="studentName">Nombre del estudiante.</param>
        /// <param name="student">Referencia al objeto estudiante.</param>
        /// <returns>True si encuentra al estudiante, false en caso contrario.</returns>
        private bool TryGetStudent(string studentName, out Student student)
        {
            return _studentsController.TryGetStudent(studentName, out student);
        }

        /// <summary>
        /// Actualiza la clase en función de la respuesta del sistema de reconocimiento de voz.
        /// </summary>
        /// <param name="response">Respuesta de voz procesada.</param>
        public void UpdateClass(Meta.WitAi.Json.WitResponseNode response)
        {
            if (string.IsNullOrEmpty(_currentText)) return;

            SetLevelAudio();

            string intentName = response.GetIntentName();
            var insult = response.GetFirstEntityValue("Insultos:Insultos");

            switch (intentName)
            {
                case "Sentarse":
                    _studentsController.HandleSit(_selectedStudents);
                    break;
                case "MoverAlumno":
                    _studentsController.HandleMove(_selectedStudents, WitResultUtilities.GetFirstEntityValue(response, "Posiciones:Posiciones"));
                    break;
                case "CambiarAlumno":
                    _studentsController.HandleChange(_selectedStudents);
                    _studentsController.Resolutions = Actions.Separados | Actions.Levantarse;
                    break;
                case "Postponer":
                    _studentsController.HandlePostpone();
                    break;
                case "Expulsion":
                    _studentsController.HandleExpel(_selectedStudents);
                    _studentsController.Resolutions = Actions.Insultar;
                    break;
                case "Saludos":
                    if (!_greetingsSentInitial)
                    {
                        _greetingsSentInitial = true;
                        _studentsController.PlayAllSentence("Buenos días profesor");
                    }
                    break;
                default:
                    intentName = "No hay intención";
                    break;
            }

            Debug.Log($"{intentName}\n{response.GetTranscription()}");

            if (!string.IsNullOrEmpty(insult))
            {
                _studentsController.SetMode(TalkMode.Disrespect);
            }
        }
    }
}
