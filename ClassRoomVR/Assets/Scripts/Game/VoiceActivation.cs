using UnityEngine;
using TMPro;
using Meta.WitAi;
using Meta.WitAi.Data;
using UnityEngine.UI;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;

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
            volumeList = new List<float>();
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

            appVoiceExperience.VoiceEvents.OnResponse.AddListener((response) =>
            {
                UpdateClass(response);
            });

            appVoiceExperience.VoiceEvents.OnValidatePartialResponse.AddListener((response) =>
            {
                OnValidatePartialResponse(response);
            });

            appVoiceExperience.VoiceEvents.OnMicAudioLevelChanged.AddListener((value) =>
            {
                OnMicLevelChanged(value);
            });




            appVoiceExperience.VoiceEvents.OnMicStartedListening.AddListener(() =>
            {
                volumeList.Clear();
            });

            appVoiceExperience.VoiceEvents.OnMicStoppedListening.AddListener(() =>
            {

                double media = volumeList.Mean();
                if (media > -25)
                {
                    Debug.Log("Gritando");
                    st.SetMode(TalkMode.Disrespect);
                }
                else if(media < -54) 
                {
                    Debug.Log("Susurrando");
                    st.SetMode(TalkMode.Good);

                }
                else { Debug.Log("Normal"); st.SetMode(TalkMode.Normal); }
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
            appVoiceExperience.Activate();
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
        List<float> volumeList;
        public void OnMicLevelChanged(float a)
        {
            float db = 20 * Mathf.Log10(a);
            if (db > -65)
            {
                Debug.Log(db);
               // Debug.Log("Nota" + Unity.Mathematics.math.remap(-60, -20, 0f, 1f, db));
                volumeList.Add(db);              
            }
        }

        
        //System.Collections.IEnumerator Wait() 
        //{
        //    float startTime = Time.time; // Guarda el tiempo inicial
        //    yield return new WaitUntil(() => miclevel < 0.8f && miclevel> 0.2f );
        //    float elapsedTime = Time.time - startTime; // Calcula el tiempo transcurrido
        //    Debug.Log("Tiempo transcurrido: " + elapsedTime + " segundos");
        //    shout = false;
        //}


        public void OnValidatePartialResponse(VoiceSession sessionData)
        {
            string[] names = sessionData.response.GetAllEntityValues("wit$contact:student");
            if (names != null && names.Length > 0)
            {
                OnValidateStudent(sessionData, names[0]);
            }
        }


        Student studentSelected;
        public void OnValidateStudent(VoiceSession sessionData, string student)
        {
            Student s;
            if (TryGetStudent(student,out s)) 
            {
                studentSelected = s;
                st.HandleCall(s);
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
      //  public void UpdateClass(VoiceSession sessionData) 
        public void UpdateClass(Meta.WitAi.Json.WitResponseNode response)
        {
           // var response = sessionData.response;
            string intentName = response.GetIntentName();
          //  var alumnos = response.GetAllEntityValues("wit$contact:student");
            var insulto = response.GetFirstEntityValue("Insultos:Insultos");
            Debug.Log(intentName + response.GetTranscription());
            

            switch (intentName)
            {
                case "Sentarse":
                    st.HandleSit(studentSelected);
                    break;
                //case "CambiarSitio":
                //    st.HandleMove(alumnos, WitResultUtilities.GetFirstEntityValue(response, "Posiciones:Posiciones"));
                //    break;
                case "Postponer":
                    st.HandlePostpone();
                    break;
                case "Expulsion":
                    st.HandleExpel(studentSelected);
                    break;
            }

            if (insulto != "") 
            {
                st.HandleDisrespect();
            }
            else { st.HandleNormal(); }
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