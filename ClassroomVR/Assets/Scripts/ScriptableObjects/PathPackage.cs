using UnityEngine;
using UnityEngine.Events;

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

        [Tooltip("Animacion de reaccion de la clase a la respuesta del profe")]
        public AnimationClip pathClassAnimation;
        [Tooltip("Animacion de reaccion de los problematicos a la respuesta del profe")]
        public AnimationClip pathProbAnimation;

        // Se debe crear un prefab, ubicados en "Resources/prefabs/ScenesBeheviours" al cual se le añade un script con los metodos que se quieran implementar
        [Tooltip("Comportamiento especial de la escena tras la eleccion del camino")]
        public UnityEvent especificBehaviour;

        [Tooltip("Feedback final")]
        public string feedbackPath;

        [Tooltip("Audio del feedback final")]
        public AudioClip finalFeedback;

        [Tooltip("Booleano que indica si es un camino correcto")]
        public bool correctPath;
    }
}
