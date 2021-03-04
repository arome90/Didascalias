using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/pathPackage", order = 1)]
    public class PathPackage : ScriptableObject
    {
        [Header("Informacion necesaria de un path")]

        [Tooltip("Info de que hacer para activar este camino")]
        public string pathInfo;

        [Tooltip("Palabras clave")]
        public string[] keyWords;

        // Solo deberia haber un camino de este tipo en cada escena, sino se cogera el primero que lo tenga
        [Tooltip("Acercarse al alumno problematica")]
        public bool getClose;

        // Solo deberia haber un camino de este tipo en cada escena, sino se cogera el primero que lo tenga
        [Tooltip("Booleano que indica si es un camino de ignorar")]
        public bool ignore;

        [Tooltip("Audio de reaccion de la clase final")]
        public AudioClip audio;

        [Tooltip("Animacion de reaccion a la respuesta del profe")]
        public AnimationClip pathAnimation;

        [Tooltip("Feedback final")]
        public string feedbackPath;

        [Tooltip("Booleano que indica si es un camino correcto")]
        public bool correctPath;
    }
}
