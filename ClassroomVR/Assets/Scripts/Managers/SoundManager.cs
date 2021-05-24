using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClassRoomVR
{
    public class SoundManager : MonoBehaviour
    {
        public MicToVokaturi _vokaturi;

        public MicrophoneManager _micManager;

        [HideInInspector]
        public KeyWordRecognizer _recognizer;

        public SoundLoudness _loudness;


        void Awake()
        {
            _recognizer = new KeyWordRecognizer();
        }

        public string processVokaturiInfo()
        {
            string result;
            if (_vokaturi.finalSad == _vokaturi.finalNeutral && _vokaturi.finalSad == _vokaturi.finalHappy && _vokaturi.finalSad == _vokaturi.finalFear && _vokaturi.finalSad == _vokaturi.finalAnger)
            {
                result = "\n" + "Vokaturi no ha podido identificar las emociones correctamente. Todos los valores asignados a 0,1 en la ponderación final." + "\n";
            }
            else
            {
                string happyVoice = "El porcentaje de comentarios con emocion Happy ha sido de: " + (_vokaturi.mediaHappy.ToString() + " y en la ponderación final de: " + _vokaturi.finalHappy.ToString());
                string sadVoice = "El porcentaje de comentarios con emocion Sad ha sido de: " + (_vokaturi.mediaSad.ToString() + " y en la ponderación final de: " + _vokaturi.finalSad.ToString());
                string angerVoice = "El porcentaje de comentarios con emocion Anger ha sido de: " + (_vokaturi.mediaAnger.ToString() + " y en la ponderación final de: " + _vokaturi.finalAnger.ToString());
                string fearVoice = "El porcentaje de comentarios con emocion Fear ha sido de: " + (_vokaturi.mediaFear.ToString() + " y en la ponderación final de: " + _vokaturi.finalFear.ToString());
                string neutralVoice = "El porcentaje de comentarios con emocion Neutral ha sido de: " + (_vokaturi.mediaNeutral.ToString() + " y en la ponderación final de: " + _vokaturi.finalNeutral.ToString());
                result = "\n" + happyVoice + "\n" + sadVoice + "\n" + angerVoice + "\n" + fearVoice + "\n" + neutralVoice + "\n\n";
            }
            return result;
            
        }
    }
}