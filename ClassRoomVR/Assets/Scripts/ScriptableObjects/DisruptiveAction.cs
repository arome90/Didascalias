using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

 namespace ClassRoomVR
{
    //Actualizar siempre respecto a la lista de posiciones que pueden moverse los alumnos
    public enum Positions 
    {
        None=-1,FrontSide,BackCorner,Doors
    }
    [CreateAssetMenu(fileName = "DisruptiveAction", menuName = "ScriptableObject/DisruptiveAction", order = 5)]
    public class DisruptiveAction : ScriptableObject
    {

        public AnimationClip problematicsAnimation;
        public AudioClip audioSituationMasculino;
        public AudioClip audioSituationFemenino;

        public AudioClip reaccionClase;
        public AudioClip ruido;
        public bool risas;

        [Tooltip("Numero de personas que participan en la accion disruptiva")]
        public int numStudents=1;

        [Tooltip("Posicion a la que se desplaza ")]
        public  Positions pos;

        public float timeToReact;

        public  GameObject  bh;

        //[Tooltip("Comportamiento especial de la escena en cualquier momento de ejecucion durante la eleccion del camino")]
        //public UnityEngine.Events.UnityEvent especificBehaviour;
    }


}