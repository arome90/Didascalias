using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClassRoomVR
{
    public class SituationAudioPack : MonoBehaviour
    {
        [Header("Audios a reproducir en el escenario seleccionado")]


        [Tooltip("Audio del contexto. Primer audio de la escena.")]
        public AudioClip _contextClip; 
        [Tooltip("Audios posibles en funcion del path elegido.")]
        public AudioClip[] pathclips;

    }
}
