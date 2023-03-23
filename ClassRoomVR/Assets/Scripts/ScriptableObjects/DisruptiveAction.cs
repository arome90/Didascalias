using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

 namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "DisruptiveAction", menuName = "ScriptableObject/DisruptiveAction", order = 5)]
    public class DisruptiveAction : ScriptableObject
    {

        [Tooltip("Animaciones para la 'situacion critica', colocar en orden de ejecucion")]
        public AnimationClip problematicsAnimation;
        [Tooltip("Audios necesarios para la 'situacion critica', colocar en orden de ejecucion")]
        public AudioClip audioSituationMasculino;
        [Tooltip("Audios necesarios para la 'situacion critica', colocar en orden de ejecucion")]
        public AudioClip audioSituationFemenino;
        
        [Tooltip("Comportamiento especial de la escena en cualquier momento de ejecucion durante la eleccion del camino")]
        public UnityEngine.Events.UnityEvent especificBehaviour;
    }

     
}