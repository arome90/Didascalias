using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.WitAi;
using Meta.WitAi.Json;

namespace ClassRoomVR
{
    public class VoiceActivation : MonoBehaviour
    {
        private Wit wit;
        bool shout;
        private void Start()
        {
            wit = GetComponent<Wit>();
            GameManager.Instance.SetVoiceActivation(this);

        }



        public void OnResponse(WitResponseNode response)
        {

            if (!string.IsNullOrEmpty(response["text"]))
            {
                Debug.Log("I heard: " + response["text"]);
            }
            else
            {
                Debug.Log(
                     "I dont heard ");
            }
        }


        public void ActivateWit()
        {
            Debug.Log("Habla");
            wit.Activate();
        }

        public void OnMicLevelChanged(float a)
        {
            if (!shout && a > 0.05f)
            {
                shout = true;
                GameManager.Instance.GetClassManager().GetStudentsController().SetMode(StudentsController.TalkMode.Disrespect);
                Debug.Log("Gritando");
            }

        }

    }

}