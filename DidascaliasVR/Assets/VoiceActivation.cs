using Meta.WitAi;
using Oculus.Voice;
using UnityEngine;

public class VoiceActivation : MonoBehaviour
{
    [SerializeField] 
    AppVoiceExperience appVoiceExperience;

    [SerializeField] 
    GameObject _debugPanel;
    TMPro.TextMeshProUGUI _debugText;

    void Start()
    {
        // st = ClassManager.Instance.GetStudentsController();
        _debugText = _debugPanel?.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    }

    public void ActivateDebugPanel(bool active)
    {
        _debugPanel?.SetActive(active);
    }

    public void ActivateVoice()
    {
        if (appVoiceExperience != null)
            appVoiceExperience.Activate();
    }

    private void Awake()
    {
        ActivateVoice();
        // GameManager.Instance.SetVoiceExperience(this);
        // studentsSelected = new List<Student>();

        // Cuando se complete la petición, volvemos a activar la 
        // voz de nuestro jugador para realizar una escucha continua
        appVoiceExperience.VoiceEvents.OnComplete.AddListener((a) =>
        {
            appVoiceExperience.Activate();
        });

        // En caso de que exista algún error, volvemos a activar
        // el comando de voz. Deberían tratarse los errores de conectividad
        // en un ConnectionManager, o algo parecido
        appVoiceExperience.VoiceEvents.OnError.AddListener((a, b) =>
        {
            appVoiceExperience.Activate();
            /* NO HACE FALTA QUE ESTÉ AQUÍ SI LO COMPROBAMOS EN EL HTTPCLIENT CONTINUAMENTE
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.Log("nO HAY INTERNET");
                GameManager.Instance.LostSessionConnection();
            }
            */
        });

        // Al recibir la respuesta, la procesamos y cambiamos el estado de la clase
        appVoiceExperience.VoiceEvents.OnResponse.AddListener((response) =>
        {
            UpdateClass(response);
        });

        // Al recibir una respuesta todavía parcial, vemos qué estudiantes han sido
        // llamados por el jugador
        appVoiceExperience.VoiceEvents.OnValidatePartialResponse.AddListener((response) =>
        {
            OnValidatePartialResponse(response);
        });

        // Lanzamos el texto por el panel de Debug
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener((transcription) =>
        {
            if (_debugText)
            {
                _debugText.text = transcription;
            }
        });
    }

    public void OnValidatePartialResponse(Meta.WitAi.Data.VoiceSession sessionData)
    {
        if (sessionData.response == null) return;

        string[] names = sessionData.response.GetAllEntityValues("wit$contact:student");

        if (names != null && names.Length > 0)
        {
            // Student s;
            for (int i = 0; i < names.Length; i++)
            {
                //if (TryGetStudent(names[i], out s))
                //{
                //    if (i == 0)
                //    {
                //        // studentsSelected.Clear();
                //    }
                //    // studentsSelected.Add(s);
                //    // st.HandleCall(s);
                //}
            }
        }
    }
    //private bool TryGetStudent(string studentName, out Student s)
    //{
    //    // Checkea student name
    //    if (ClassManager.Instance.GetStudentsController().TryGetStudent(studentName, out s))
    //    {
    //        return true;
    //    }
    //    // No existe
    //    return false;
    //}

    //Gestion de las ordenes del profesor
    //TO DO : CAMBIAR PARA QUE SEA GENERICO
    //  public void UpdateClass(VoiceSession sessionData) 
    public void UpdateClass(Meta.WitAi.Json.WitResponseNode response)
    {
        // var response = sessionData.response;
        string intentName = response.GetIntentName();
        //  var alumnos = response.GetAllEntityValues("wit$contact:student");
        var insulto = response.GetFirstEntityValue("Insultos:Insultos");

        switch (intentName)
        {
            //case "Sentarse":
            //    st.HandleSit(studentsSelected);
            //    break;
            //case "MoverAlumno":
            //    st.HandleMove(studentsSelected, WitResultUtilities.GetFirstEntityValue(response, "Posiciones:Posiciones"));
            //    break;
            //case "CambiarAlumno":
            //    st.HandleChange(studentsSelected);
            //    st.Resolutions = Actions.Separados | Actions.Levantarse;
            //    break;
            //case "Postponer":
            //    st.HandlePostpone();
            //    break;
            //case "Expulsion":
            //    st.HandleExpel(studentsSelected);
            //    st.Resolutions = Actions.Insultar;
            //    break;
            //case "Saludos":
            //    Didascalia_LocalizationManager.Instance.GetTranslation("greetingsTeacher",
            //        Didascalia_LocalizationManager.TableCollections.AUDIO,
            //        out string traduction);
            //    st.PlayAllSentence(traduction);
            //    break;
            default:
                intentName = "No hay intencion";
                break;
        }

        Debug.Log(intentName + "\n" + response.GetTranscription());

        if (insulto != "")
        {
            // st.SetMode(TalkMode.Disrespect);
        }
    }
}
