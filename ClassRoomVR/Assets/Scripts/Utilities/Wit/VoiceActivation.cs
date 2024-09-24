using UnityEngine;
using System.Collections.Generic;
using Oculus.Voice;
using MathNet.Numerics.Statistics;
using Meta.WitAi;
using Meta.WitAi.Composer.Integrations;
using MathNet.Numerics.Distributions;
using Utilities.Extensions;
using Meta.WitAi.Composer;

namespace ClassRoomVR
{
    public class VoiceActivation : MonoBehaviour
    {
        [SerializeField] AppVoiceExperience appVoiceExperience;
        StudentsController st;
        string text;
        [SerializeField] TMPro.TextMeshProUGUI textMeshPro;
        List<Student> studentsSelected;

        void Start()
        {
            st = ClassManager.Instance.GetStudentsController();
        }

        public void ActiveText(bool active)
        {
            if (textMeshPro != null) textMeshPro.transform.parent.SetActive(active);
        }
        public void Activate()
        {
            if (appVoiceExperience != null) 
                appVoiceExperience.Activate();
        }

        private void Awake()
        {
            text = string.Empty;
            GameManager.Instance.SetVoiceExperience(this);
            studentsSelected = new List<Student>();
            appVoiceExperience.VoiceEvents.OnComplete.AddListener((a) =>
            {
                appVoiceExperience.Activate();
            });

            appVoiceExperience.VoiceEvents.OnError.AddListener((a, b) =>
            {
                appVoiceExperience.Activate();
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Debug.Log("nO HAY INTERNET");
                    GameManager.Instance.Pause(true);
                }
            });

            appVoiceExperience.VoiceEvents.OnResponse.AddListener((response) =>
            {
                UpdateClass(response);
            });

            appVoiceExperience.VoiceEvents.OnValidatePartialResponse.AddListener((response) =>
            {
                OnValidatePartialResponse(response);
            });
            //appVoiceExperience.VoiceEvents.OnResponse.AddListener((response) =>
            //{
            //    OnValidateResponse(response);
            //});
            appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener((strin) =>
            {
                text = strin;
                Debug.Log(text);
                if (textMeshPro)
                {
                    textMeshPro.text = strin;
                    Debug.Log("texto");
                }
                appVoiceExperience.Deactivate();
                Activate();
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
                Student s;
                for (int i = 0; i < names.Length; i++)
                {
                    if (TryGetStudent(names[i], out s))
                    {
                        if (i == 0)
                        {
                            studentsSelected.Clear();
                        }
                        studentsSelected.Add(s);
                        st.HandleCall(s);
                    }
                }
            }
        }
        private bool TryGetStudent(string studentName, out Student s)
        {
            // Checkea student name
            if (ClassManager.Instance.GetStudentsController().TryGetStudent(studentName, out s))
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

                switch (intentName)
                {
                    case "Sentarse":
                        st.HandleSit(studentsSelected);
                        break;
                    case "MoverAlumno":
                        st.HandleMove(studentsSelected, WitResultUtilities.GetFirstEntityValue(response, "Posiciones:Posiciones"));
                        break;
                    case "CambiarAlumno":
                        st.HandleChange(studentsSelected);
                        st.Resolutions = Actions.Separados | Actions.Levantarse;
                        break;
                    case "Postponer":
                        st.HandlePostpone();
                        break;
                    case "Expulsion":
                        st.HandleExpel(studentsSelected);
                        st.Resolutions = Actions.Insultar;
                        break;
                    case "Saludos":
                        st.PlayAllSentence("Buenos días profesor");
                        break;
                    default:
                        intentName = "No hay intencion";
                        break;
                }

                Debug.Log(intentName + "\n" + response.GetTranscription());

                if (insulto != "")
                {
                    st.SetMode(TalkMode.Disrespect);
                }
            }
        }
    }
}