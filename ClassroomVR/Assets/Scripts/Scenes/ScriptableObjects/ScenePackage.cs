using UnityEngine;
//using UnityEditor;

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

        [Tooltip("Numero de grupos, 0 si no se quieren formar grupos")]
        // Esto mostraria una barra pero no se hacerlo bien :S
        // private static float nGroupsf = (int)EditorGUILayout.Slider(nGroupsf, 1f, 4f);
        public int nGroups = 0;

        //------
        [Header("Recursos especificos de la situacion")]
        
        [Tooltip("Mensaje con la informacion inicial de la situacion")]
        public string iniMessage;

        [Tooltip("Animaciones para la 'situacion critica', colocar en orden de ejecucion")]
        public AnimationClip[] problematicsAnimations;

        [Tooltip("Audios necesarios para la 'situacion critica', colocar en orden de ejecucion")]
        public AudioClip[] audiosSituationMasculino;
        [Tooltip("Audios necesarios para la 'situacion critica', colocar en orden de ejecucion")]
        public AudioClip[] audiosSituationFemenino;

        [Tooltip("Audio de reaccion de la clase a la situacion critica")]
        public AudioClip audioReaccionClase;

        //-----
        [Header("Posibles paths a tomar por el profesor")]
        public PathPackage[] paths;

        /*[Header("Propiedades para la respuesta del profesor")]
        [Tooltip("Posibles elecciones a tomar por el profesor")]
        public string[] posibolElections = new string[3];
        */
        [Tooltip("Tiempo de espera para dar la respuesta como 'ignorada'")]
        public float timeToWait;

        //-----
        /*
        [Header("Camino 1")]
        [Tooltip("Palabras clave")]
        public string[] keyWords1;
        [Tooltip("Posicion camino")]
        public bool pos1;
        [Tooltip("Audio final")]
        public AudioClip audio1;
        [Tooltip("Animacion de reaccion a la respuesta del profe")]
        public AnimationClip path1Animation;
        [Tooltip("Feedback final")]
        public string feedbackPath1;

        //------
        [Header("Camino 2")]
        [Tooltip("Palabras clave")]
        public string[] keyWords2;
        [Tooltip("Posicion camino")]
        public bool pos2;
        [Tooltip("Audio final")]
        public AudioClip audio2;
        [Tooltip("Animacion de reaccion a la respuesta del profe")]
        public AnimationClip path2Animation;
        [Tooltip("Feedback final")]
        public string feedbackPath2;

        //------
        [Header("Camino 3")]
        [Tooltip("Palabras clave")]
        public string[] keyWords3;
        [Tooltip("Posicion camino")]
        public bool pos3;
        [Tooltip("Audio final")]
        public AudioClip audio3;
        [Tooltip("Animacion de reaccion a la respuesta del profe")]
        public AnimationClip path3Animation;
        [Tooltip("Feedback final")]
        public string feedbackPath3;
        */
    }
}