using UnityEngine;
using System.Collections.Generic;
using Oculus.Voice;
using MathNet.Numerics.Statistics;
using Meta.WitAi;
using Meta.WitAi.Composer.Integrations;

namespace ClassRoomVR
{
    public class VoiceActivation : MonoBehaviour
    {
        [SerializeField] AppVoiceExperience appVoiceExperience;
        
         StudentsController st;
        string text;
        private void Start()
        {
            appVoiceExperience.Activate();
            st = ClassManager.Instance.GetStudentsController();
        }

        public void Activate()
        {
            if(appVoiceExperience!=null) appVoiceExperience.Activate();
        }
        private void Awake()
        {
            GameManager.Instance.SetVoiceExperience(this);
            studentSelected = null;
            appVoiceExperience.VoiceEvents.OnComplete.AddListener((a) =>
            {
                 Debug.Log("¡activarCom");
                appVoiceExperience.Activate();
            });

            appVoiceExperience.VoiceEvents.OnError.AddListener((a,b) => 
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Debug.Log("nO HAY INTERNET");
                    GameManager.Instance.Pause(true);
                }
            });

            appVoiceExperience.VoiceEvents.OnResponse.AddListener((response) =>
            {
                Debug.Log("¡update");
                UpdateClass(response);
            });

            //appVoiceExperience.VoiceEvents.OnValidatePartialResponse.AddListener((response) =>
            //{
            //    Debug.Log("¡validate");

            //    OnValidatePartialResponse(response);
            //});
                appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener((strin) =>
                {
                    text = strin;

                });
             
                
           

            appVoiceExperience.VoiceEvents.OnMicAudioLevelChanged.AddListener((value) =>
            {
                OnMicLevelChanged(value);
            });

            appVoiceExperience.VoiceEvents.OnMicStartedListening.AddListener(() =>
            {
                volumeList.Clear();
            });
        
            volumeList = new List<float>();

        }


        private void SetLevelAudio() 
        {
            double media = volumeList.Mean();
            //  Debug.Log(media);
            if (media > -30)
            {
                Debug.Log("¡Gritando " + (int)media);
                st.SetMode(TalkMode.Disrespect);
            }
            else if (media < -50)
            {
                Debug.Log("¡Susurrando " + (int)media);
                st.SetMode(TalkMode.Good);

            }
            else { Debug.Log("¡Normal " + (int)media); st.SetMode(TalkMode.Normal); }
        }
        List<float> volumeList;
        public void OnMicLevelChanged(float a)
        {
            float db = 20 * Mathf.Log10(a);
            if (db > -65)
            {
               // Debug.Log(db);
               // Debug.Log("Nota" + Unity.Mathematics.math.remap(-60, -20, 0f, 1f, db));
                volumeList.Add(db);              
            }
        }

        public void OnValidatePartialResponse(Meta.WitAi.Data.VoiceSession sessionData)
        {
            string[] names = sessionData.response.GetAllEntityValues("wit$contact:student");
            if (names != null && names.Length > 0)
            {
                OnValidateStudent(sessionData, names[0]);
            }
        }


        //temporal
        public void OnValidateResponse(Meta.WitAi.Json.WitResponseNode response)
        {
            string[] names = response.GetAllEntityValues("wit$contact:student");
            if (names != null && names.Length > 0)
            {
                Student s;
                if (TryGetStudent(names[0], out s))
                {
                    studentSelected = s;
                    st.HandleCall(s);
                }
            }
        }

        Student studentSelected;
        public void OnValidateStudent(Meta.WitAi.Data.VoiceSession sessionData, string student)
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
            
            if (text.Length > 0)
            {
                SetLevelAudio();
                // var response = sessionData.response;
                string intentName = response.GetIntentName();
                //  var alumnos = response.GetAllEntityValues("wit$contact:student");
                var insulto = response.GetFirstEntityValue("Insultos:Insultos");
                OnValidateResponse(response);

                switch (intentName)
                {
                    case "Sentarse":
                        st.HandleSit(studentSelected);
                        break;
                    case "CambiarSitio":
                        st.HandleMove(studentSelected, WitResultUtilities.GetFirstEntityValue(response, "Posiciones:Posiciones"));
                        break;
                    case "Postponer":
                        st.HandlePostpone();
                        break;
                    case "Expulsion":
                        st.HandleExpel(studentSelected);
                        break;
                    case "Saludos":
                        st.PlaySentence("Buenas profesor");
                        break;
                    default:
                        intentName = "No hay intencion";
                        break;
                }

                Debug.Log(intentName + "\n" + response.GetTranscription());

                if (insulto != "")
                {
                    st.HandleDisrespect();
                }

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