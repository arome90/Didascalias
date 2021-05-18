using UnityEngine;
using UnityEngine.Events;

//---------------------------------------------------------------------
// Se pueden crear instancias de ScriptableObject en los recursos
namespace ClassRoomVR {
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/scenePackage", order = 1)]
    public class ScenePackage : ScriptableObject {

        //-----
        [Header("Generacion de la clase")]

        [Tooltip("Numero total de estudiantes, maximo 30")]
        public int nStudents;

        [Tooltip("Estudiantes problematicos entre los estudiantes")]
        public int problematicStudents;


        [Tooltip("Estudiantes problematicos juntos")]
        public bool problematicTogether;

        [Tooltip("Numero de grupos, 0 si no se quieren formar grupos")]
        public int nGroups = 0;

        //------
        [Header("Recursos especificos de la situacion")]
        
        [Tooltip("Mensaje con la informacion inicial de la situacion")]
        public string iniMessage;

        [Tooltip("Audio a reproducir como contexto de la escena.")]
        public AudioClip contextClip;

        [Tooltip("Tiempo para dar clase antes de que se ejecute la situacion")]
        public float timeToStart = 0;

        [Tooltip("Tiempo para reaccionar a la situacion, si es 0 sera maxFloat")]
        public float timeToReact = 10.0f;

        [Tooltip("Animaciones para la 'situacion critica', colocar en orden de ejecucion")]
        public AnimationClip problematicsAnimation;

        [Tooltip("Audios necesarios para la 'situacion critica', colocar en orden de ejecucion")]
        public AudioClip audioSituationMasculino;
        [Tooltip("Audios necesarios para la 'situacion critica', colocar en orden de ejecucion")]
        public AudioClip audioSituationFemenino;

        [Tooltip("Audio de reaccion de la clase a la situacion critica")]
        public AudioClip audioReaccionClase;

        // TODO
        // Se debe crear un prefab, ubicados en "Resources/prefabs/ScenesBeheviours" al cual se le añade un script con los metodos que se quieran implementar
        [Tooltip("Comportamiento especial de la escena en cualquier momento de ejecucion durante la eleccion del camino")]
        public UnityEvent especificBehaviour;

        //-----
        [Header("Posibles paths a tomar por el profesor")]
        public PathPackage[] paths;
    }
}