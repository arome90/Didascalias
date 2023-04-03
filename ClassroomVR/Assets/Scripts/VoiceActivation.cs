using UnityEngine;
using TMPro;
using Meta.WitAi;

namespace ClassRoomVR
{
    public class VoiceActivation : MonoBehaviour
    {
        bool shout;

        [SerializeField] Oculus.Voice.AppVoiceExperience appVoiceExperience;

        [SerializeField] TextMeshProUGUI fullTrascriptionText;

        [SerializeField] TextMeshProUGUI partialTrascriptionText;

        bool appVoiceActive;


        [SerializeField] StudentsController st;
        private void Start()
        {
            //GameManager.Instance.SetVoiceActivation(this);
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
            fullTrascriptionText.text = partialTrascriptionText.text = string.Empty;

            appVoiceExperience.events.onFullTranscription.AddListener((transcription) =>
            {
                fullTrascriptionText.text = transcription;
            });

            appVoiceExperience.events.OnPartialTranscription.AddListener((transcription) =>
            {
                partialTrascriptionText.text = transcription;
            });

            //appVoiceExperience.events.OnRequestCreated.AddListener((request) =>
            //{
                
            //    Activate();
            //});
            

            appVoiceExperience.events.OnRequestCompleted.AddListener(() =>
            {
                Activate();

            });


            //appVoiceExperience.events.OnStoppedListening.AddListener(() =>
            //{
            //    Activate();

            //});

            appVoiceExperience.events.OnResponse.AddListener((response) =>
            {
                UpdateClass(response);
            });

            appVoiceExperience.events.OnMicLevelChanged.AddListener((value) =>
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
            appVoiceExperience.Activate();
        }

        public void OnMicLevelChanged(float a)
        {
            if (!shout && a > 0.05f)
            {
                shout = true;
                st.SetMode(StudentsController.TalkMode.Disrespect);
                Debug.Log("Gritando");
            }

        }

        //Gestion de las ordenes del profesor
        //TO DO : CAMBIAR PARA QUE SEA GENERICO
        public void UpdateClass(Meta.WitAi.Json.WitResponseNode response)
        {
            
            var intent = WitResultUtilities.GetIntentName(response);
            var alumnos = WitResultUtilities.GetAllEntityValues(response, "wit$contact:student");
           
            switch (intent)
            {
                case "Sit":
                    st.HandleSit(alumnos);
                    break;
                case "Move":
                    st.HandleMove(alumnos, WitResultUtilities.GetFirstEntityValue(response, "places:places"));
                    break;
                case "Postpone":
                    st.HandlePostpone();
                    break;
                case "Expel":
                    st.HandleExpel(alumnos);
                    break;
                case "Disrespect":
                    st.HandleDisrespect();
                    break;
                case "Calm":
                    st.HandleCalm();
                    break;
                default:
                    Debug.LogError($"Intent no reconocido: {intent}");
                    break;

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