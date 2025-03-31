using System.Collections;
using UnityEngine;
using Meta.WitAi.TTS.Utilities;
using System;
using Meta.WitAi.TTS.Data;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona la respuesta hablada de un estudiante usando TTSSpeaker.
    /// </summary>
    public class ResponseStudent2 : MonoBehaviour
    {
        [SerializeField] private string _dateId = "[DATE]"; // Identificador de la fecha en el texto
        private TTSSpeaker _speaker; // Componente TTSSpeaker para la síntesis de voz
        private Student2 _student; // Componente Student para gestionar el color y el movimiento de la mandíbula
        private AudioClip _asyncClip; // Clip de audio para reproducción asíncrona
        private bool _isSpeaking; // Indica si el estudiante está hablando

        /// <summary>
        /// Propiedad que indica si el estudiante está hablando.
        /// </summary>
        public bool IsSpeaking => _isSpeaking;

        private void Start()
        {
            _student = GetComponent<Student2>(); // Obtiene el componente Student
            _asyncClip = GetComponent<AudioClip>(); // Obtiene el componente AudioClip
            _speaker = GetComponent<TTSSpeaker>(); // Obtiene el componente TTSSpeaker

            // Configura el ID de la voz según el género del estudiante
            _speaker.VoiceID = _student.GetGender() == Gender2.Men ? "WIT$CAM" : "MARIA";
            _speaker.Events.OnPlaybackComplete.AddListener(OnPlaybackComplete); // Añade un listener para el evento de finalización de reproducción
        }

        /// <summary>
        /// Inicia la reproducción del texto hablado.
        /// </summary>
        /// <param name="text">Texto a reproducir.</param>
        public void SpeakText(string text)
        {
            _student.SetColor(Color.yellow); // Cambia el color del estudiante a amarillo
            _speaker.Speak(FormatText(text)); // Reproduce el texto formateado
            _isSpeaking = true; // Marca que el estudiante está hablando
            _student.MoveJaw(); // Mueve la mandíbula del estudiante
        }

        /// <summary>
        /// Reproduce el texto hablado de manera asíncrona.
        /// </summary>
        /// <param name="text">Texto a reproducir.</param>
        /// <param name="queued">Indica si el texto debe añadirse a una cola de reproducción.</param>
        /// <returns>IEnumerator para la reproducción asíncrona.</returns>
        private IEnumerator SpeakTextAsync(string text, bool queued)
        {
            yield return queued ? _speaker.SpeakQueuedAsync(new[] { text }) : _speaker.SpeakAsync(text);

            if (_asyncClip != null)
            {
                _speaker.AudioSource.PlayOneShot(_asyncClip); // Reproduce el clip de audio asíncrono
            }
        }

        /// <summary>
        /// Maneja la finalización de la reproducción del texto hablado.
        /// </summary>
        /// <param name="speaker">Instancia del TTSSpeaker que realizó la reproducción.</param>
        /// <param name="data">Datos del clip de TTS que se reprodujo.</param>
        private void OnPlaybackComplete(TTSSpeaker speaker, TTSClipData data)
        {
            _isSpeaking = false; // Marca que el estudiante ha terminado de hablar
            _student.SetColor(_student.IsProblematicStudent() ? Color.red : Color.white); // Cambia el color del estudiante según su estado
        }

        /// <summary>
        /// Formatea el texto reemplazando el identificador de fecha con la fecha y hora actuales.
        /// </summary>
        /// <param name="text">Texto a formatear.</param>
        /// <returns>Texto formateado.</returns>
        private string FormatText(string text)
        {
            if (text.Contains(_dateId))
            {
                string dateString = $"{DateTime.Now.ToLongDateString()} at {DateTime.Now.ToLongTimeString()}";
                return text.Replace(_dateId, dateString); // Reemplaza el identificador de fecha con la fecha y hora actuales
            }
            return text;
        }
    }
}
