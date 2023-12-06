using UnityEngine;
using TMPro;
using Meta.WitAi;
using Meta.WitAi.Data;
using Meta.WitAi.Json;

namespace ClassRoomVR
{
    public class VoiceActivation : MonoBehaviour
    {
        bool shout;
        [SerializeField] Oculus.Voice.AppVoiceExperience appVoiceExperience;
        //[SerializeField] TextMeshProUGUI fullTranscriptionText;
        //[SerializeField] TextMeshProUGUI partialTranscriptionText;
        bool appVoiceActive;
        [SerializeField] StudentsController st;

        private void Start()
        { 
            Activate();
        }



        //public void OnResponse(WitResponseNode response)
        //{

        //    if (!string.IsNullOrEmpty(response["text"]))
        //    {
        //        Debug.Log("I heard: " + response["text"]);
        //    }
        //    else
        //    {
        //        Debug.Log(
        //             "I dont heard ");
        //    }
        //}


        private void Awake()
        {
            GameManager.Instance.SetVoiceActivation(this);

            appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(() =>
            {
                Activate();
            });

            //appVoiceExperience.VoiceEvents.OnResponse.AddListener((response) =>
            //{
            //    UpdateClass(response);
            //});

            appVoiceExperience.VoiceEvents.OnValidatePartialResponse.AddListener((response) =>
            {
                UpdateClass(response);
            });

            appVoiceExperience.VoiceEvents.OnMicLevelChanged.AddListener((value) =>
            {
                OnMicLevelChanged(value);
            });
        }



        //private static void DisplayValues(string prefix, string[] info) 
        //{
        //    foreach(var i in info) 
        //    {
        //        Logger A = new Logger();
        //        A.Log()
        //    }
        //}

        public void Activate()
        {
            Debug.Log("Habla");
            appVoiceExperience.ActivateImmediately();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.G)) 
            {
                shout = true;
                st.SetMode(TalkMode.Disrespect);
                Debug.Log("Gritando");
            }

        }
        float miclevel;
        public void OnMicLevelChanged(float a)
        {
            miclevel = a;
           // Debug.Log(a +" "+ Time.time);
            if (!shout && a > 0.8f /*0.8f*/)
            {
                shout = true;
                StartCoroutine(Wait());
                //AnalyticsManager.CustomEvent("Gritar");
                st.SetMode(TalkMode.Disrespect);
                Debug.Log("Gritando");
            }
           
        }

        
        System.Collections.IEnumerator Wait() 
        {
            float startTime = Time.time; // Guarda el tiempo inicial
            yield return new WaitUntil(() => miclevel < 0.8f && miclevel> 0.2f );
            float elapsedTime = Time.time - startTime; // Calcula el tiempo transcurrido
            Debug.Log("Tiempo transcurrido: " + elapsedTime + " segundos");
            shout = false;
        }


        // Validate & set color
        public void OnValidateColorSet(VoiceSession sessionData, string color)
        {
            //Color c;
            //if (TryGetColor(color, out c))
            //{
            //    SetColor(c);
            //    sessionData.validResponse = true;
            //}
        }

        Student studentSelected;
        public void OnValidateStudent(VoiceSession sessionData, string student)
        {
            Student s;
            if (TryGetStudent(student,out s)) 
            {
                studentSelected = s;
                sessionData.validResponse = true;
            }
        }

        private bool TryGetStudent(string studentName, out Student s)
        {
            // Checkea student name
            if (ClassManager.Instance.GetStudentsController().TryGetStudent(studentName,out s))
            {
                return true;
            }
            // No existe
            return false;
        }


        //Gestion de las ordenes del profesor
        //TO DO : CAMBIAR PARA QUE SEA GENERICO
        public void UpdateClass(VoiceSession sessionData)
        {

            WitResponseNode response = sessionData.response;
            var intent = WitResultUtilities.GetIntentName(response);

            var alumnos = WitResultUtilities.GetAllEntityValues(response, "wit$contact:student");
           
            var insulto =  WitResultUtilities.GetFirstEntityValue(response, "Insultos:Insultos");
            Debug.Log(intent + response.GetTranscription());
            

            switch (intent)
            {
                case "Sentarse":
                    st.HandleSit(alumnos);
                    break;
                case "CambiarSitio":
                    st.HandleMove(alumnos, WitResultUtilities.GetFirstEntityValue(response, "Posiciones:Posiciones"));
                    break;
                case "Postponer":
                    st.HandlePostpone();
                    break;
                case "Expulsion":
                    st.HandleExpel(alumnos);
                    break;
                case "LLamarAlumno":
                    st.HandleCall(alumnos);
                    break;
                default:
                    Debug.Log($"Intent no reconocido: {intent}");
                    break;

            }

            if (insulto != "") 
            {
                st.HandleDisrespect();
            }
        }




        //[MatchIntent("Move")]
        //public void OnHandleMoveIntentWithConduit(Meta.WitAi.Json.WitResponseNode response) 
        //{
        //    Debug.Log("hola");
        //}
        // TODO:Investigar acerca de conduit
        //[MatchIntent("change_color")]
        //public void OnHandleColorIntentWithConduit(Color color, Shape shape)
        //{
        //    Debug.Log($"OnHandleColorIntent was triggered via Conduit with color {color} {color.ToString()} and shape {shape} {shape.ToString()}");

        //    var shapeTransform = transform.Find(shape.ToString());
        //    if (shapeTransform)
        //    {
        //        if (ColorUtility.TryParseHtmlString(color.ToString(), out var unityColor))
        //        {
        //            SetColor(shapeTransform, unityColor);
        //        }
        //    }
        //}


    }

}