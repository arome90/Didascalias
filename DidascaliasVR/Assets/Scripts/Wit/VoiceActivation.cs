using AYellowpaper.SerializedCollections;
using Meta.WitAi;
using NUnit.Framework;
using Oculus.Voice;
using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A�ade funcionalidad gen�rica a los eventos recibidos por Wit,
/// adem�s de activar permanentemente el micr�fono cuando el jugador 
/// est� hablando 
/// </summary>
public class VoiceActivation : Singleton<VoiceActivation>
{
    [Header("References")]
    [SerializeField] 
    AppVoiceExperience appVoiceExperience;

    [Header("Debug")]
    [SerializeField] 
    WitDebugPanel _debugPanel;
    TMPro.TextMeshProUGUI _debugText;

    [Header("Events")]
    /// <summary>
    /// Diccionario que contiene una intenci�n y el m�todo al que queremos
    /// llamar cuando se registre una entrada con dicha intenci�n
    /// 
    /// No se usa para conflictos, sólo para las acciones que se llamen por el habla del usuario
    /// </summary>
    [SerializeField, SerializedDictionary("Intention", "On response to intention")]
    SerializedDictionary<Intention, UnityEvent<WitMessageData>> _onResponseToIntent;

    #region events
    private UnityEvent<WitMessageData> _onValidatePartialResponse;
    public UnityEvent<WitMessageData> OnValidatePartialResponse { get { return _onValidatePartialResponse; } }
    #endregion 

    void Start()
    {
        // st = ClassManager.Instance.GetStudentsController();
        _debugText = _debugPanel?.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }

    /// <summary>
    /// Activa o desactiva el panel de debug de informaci�n
    /// </summary>
    /// <param name="active"> Si se activa o no </param>
    public void ActivateDebugPanel(bool active)
    {
        if (!_debugPanel) return;
        _debugPanel.enabled = active;
    }

    /// <summary>
    /// Activa el reconocimiento de voz
    /// </summary>
    public void ActivateVoice()
    {
        if (appVoiceExperience != null)
            appVoiceExperience.Activate();
    }

    protected override void Awake()
    {
        base.Awake();
        appVoiceExperience.OnInitialized += ActivateVoice;
        AddVoiceListeners();

        // GameManager.Instance.SetVoiceExperience(this);
        // studentsSelected = new List<Student>();
    }

    /// <summary>
    /// A�ade a los eventos de Wit las funciones necesarias para el
    /// funcionamiento de la aplicaci�n
    /// </summary>
    void AddVoiceListeners()
    {
        if(_onValidatePartialResponse == null)
        {
            _onValidatePartialResponse = new UnityEvent<WitMessageData>();
        }

        // Cuando se complete la petici�n, volvemos a activar la 
        // voz de nuestro jugador para realizar una escucha continua
        appVoiceExperience.VoiceEvents.OnComplete.AddListener((a) =>
        {
            ActivateVoice();
        });

        // En caso de que exista alg�n error, volvemos a activar
        // el comando de voz. Deber�an tratarse los errores de conectividad
        // en un ConnectionManager, o algo parecido
        appVoiceExperience.VoiceEvents.OnError.AddListener((a, b) =>
        {
            ActivateVoice();
            /* NO HACE FALTA QUE EST� AQU� SI LO COMPROBAMOS EN EL HTTPCLIENT CONTINUAMENTE
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("nO HAY INTERNET");
                GameManager.Instance.LostSessionConnection();
            }
            */
        });

        // Al recibir la respuesta, la procesamos
        appVoiceExperience.VoiceEvents.OnResponse.AddListener((response) =>
        {
            OnResponse(response);
        });

        // Al recibir una respuesta todav�a parcial, vemos qu� estudiantes han sido
        // llamados por el jugador
        appVoiceExperience.VoiceEvents.OnValidatePartialResponse.AddListener((sessionData) =>
        {
            _onValidatePartialResponse.Invoke(MakeMessage(sessionData.response));
        });

        // Lanzamos el texto por el panel de Debug
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener((transcription) =>
        {
            SetMainPanelText(transcription);
        });
    }

    #region DEBUG PANEL
    void SetMainPanelText(string text)
    {
        _debugPanel.SetMainText(text);
    }

    /// <summary>
    /// Monta un string con los estudiantes resaltados en color azul y separados
    /// entre comas con un punto al final.
    /// </summary>
    /// <param name="data"> Nombres de alumnos seleccionados </param>
    /// <returns> Estudiantes seleccionados </returns>
    string GetStudentsInData(System.Collections.Generic.List<string> data)
    {
        string message = "";
        int i = 0;
        foreach (string name in data)
        {
            message += "<color=blue>" + name + "</color>";
            if (i != data.Count - 1)
            {
                message += ", ";
            }
            else
            {
                message += '.';
            }
            ++i;
        }
        return message;
    }

    /// <summary>
    /// A�ade un panel de DEBUG con los estudiantes seleccionados
    /// </summary>
    /// <param name="data"> Datos de respuesta de Wit </param>
    void ChangeSelectedStudents(WitMessageData data)
    {
        string students = GetStudentsInData(StudentManager.Instance.GetSelectedStudents());
        if (students == "") return;

        string message = "Selected: " + students;
        _debugPanel.ChangeStudentPanel(message);
    }

    /// <summary>
    /// A�ade un panel con la intenci�n a ejecutar y los estudiantes afectados
    /// </summary>
    /// <param name="data"> Datos de respuesta de Wit </param>
    void AddPanelWithIntent(WitMessageData data)
    {
        if (data.Intention == Intention.None) return;

        string message = data.Intention.ToString()
            + ": " + GetStudentsInData(StudentManager.Instance.GetSelectedStudents());

        _debugPanel.AddPanel(message);
    }
    #endregion

    /// <summary>
    /// Generamos el mensaje seg�n los datos de la sesi�n de Wit
    /// Estos datos rellenan el nombre de los estudiantes afectados y la intenci�n del mensaje
    /// </summary>
    /// <param name="response"> Datos de la sesi�n de Wit </param>
    /// <returns> Informaci�n del mensaje transcrito </returns>
    WitMessageData MakeMessage(Meta.WitAi.Json.WitResponseNode response)
    {
        if (response == null) return new WitMessageData { Intention = Intention.None };

        string[] names = response.GetAllEntityValues("wit$contact:student");

        WitMessageData messageData = new WitMessageData();
        messageData.Names = new System.Collections.Generic.List<string>();
        foreach(string name in names)
        {
            if(StudentManager.Instance.GetStudent(name) != null)
            {
                messageData.Names.Add(name);
            }
        }
        Enum.TryParse(response.GetIntentName(), out Intention intent);
        messageData.Intention = intent;
        messageData.Transcription = response.GetTranscription();

        return messageData;
    }

    /// <summary>
    /// Recibimos la respuesta final de Wit.
    /// La parseamos e invocamos a los eventos correspondientes para que
    /// se activen seg�n hemos designado en el inspector
    /// </summary>
    /// <param name="response"> Respuesta final de Wit </param>
    public void OnResponse(Meta.WitAi.Json.WitResponseNode response)
    {
        WitMessageData messageData = MakeMessage(response);
        if (_onResponseToIntent.TryGetValue(messageData.Intention, out var action))
        {
            action.Invoke(messageData);
            AddPanelWithIntent(messageData);
        }
        else
        {
            Didascalia.Utils.Error.DebugbreakFailMessage(
                "Missing action in response to Intention: " + messageData.Intention,
                this
            );   
        }
    }
}
