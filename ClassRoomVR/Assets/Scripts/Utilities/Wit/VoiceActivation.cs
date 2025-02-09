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

        public float silenceThreshold = 15.0f;

        List<float> volumeList;

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
                }
                appVoiceExperience.Deactivate();
                Activate();
            });

            appVoiceExperience.VoiceEvents.OnMicLevelChanged.AddListener((value) =>
            {
                OnMicLevelChanged(value);
            });


            appVoiceExperience.VoiceEvents.OnMicStartedListening.AddListener(() =>
            {
                volumeList.Clear();
            });

            volumeList = new List<float>();

        }

        public float getLevelAudio()
        {
            return (float)volumeList.Mean();
        }

        public void OnMicLevelChanged(float a)
        {
            float db = 20 * Mathf.Log10(a*1000);
            //Debug.Log("DB: "+db);
            //Debug.Log(":"+a);

            if (db > silenceThreshold)
            {
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
            //SetLevelAudio();

            if (text.Length > 0)
            {
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
                        Didascalia_LocalizationManager.Instance.GetTranslation("greetingsTeacher",
                            Didascalia_LocalizationManager.TableCollections.CLASE, Didascalia_LocalizationManager.CurrentLanguage,
                            out string traduction);
                        st.PlayAllSentence(traduction);
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