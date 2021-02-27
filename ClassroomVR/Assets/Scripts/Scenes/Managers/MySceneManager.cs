using System;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR {
    public class MySceneManager : MonoBehaviour {
        // Publics
        //public GameManager _gm;
        public GameObject sceneObjects;

        // Privates
        // Objetos de la escena
        private GameObject teacher;
        private GameObject[] students;
        private int[] studentsSex;
        private int[] problematicStudents;

        // Otras cosis
        // Booleano de control de juego
        private bool playing = false;
        private bool initAnim = true;
        // DeltaTime
        private float deltaTime = 0f;
        // Tiempo para empezar a ejecutar la situacion
        private float timeToStart = 2.0f;


        //-------------
        // Cosas del gm
        [Tooltip("Esto se lo pasa el gm en funcion del nivel elegido")]
        public ScenePackage sceneInfo; // En realidad es private
        [Tooltip("Esto lo coge del gm")]
        public ClassInfo classInfo; // En realidad es private
        
        //public Canvas canvas; // En realidad lo coge del gm, no existe aqui

        // Start is called before the first frame update
        void Start() {
            // Generacion de la clase
            Instantiate(classInfo.clase, sceneObjects.transform);
            // Temporales, deberiamos poner un punto de aparicion para el teacher en el prefab de la clase
            teacher = Instantiate(classInfo.teacher,sceneObjects.transform);
           // teacher = sceneObjects.transform.Find("PlayerVR").gameObject;
            teacher.transform.position = classInfo.clase.GetComponentInChildren<Transform>().Find("TeacherDesk").position + new Vector3(0, 1.7f, 1);
            teacher.transform.Rotate(new Vector3(0, 180, 0));
            Destroy(sceneObjects.transform.Find("PlayerVR").gameObject);

            // Generamos los chavales
            
            generateChilds();

            // Mostramos el texto descriptivo de la escena (ademas del boton obvio :P)
            // Obviamente esto no es asi :), 
            // Tendriamos una referencia al canvas y de ahi cogemos el texto que sea el bueno 
            Text iniText = gameObject.AddComponent<Text>(); 
            iniText.text = sceneInfo.iniMessage;

            // Esto se hace al pulsar el boton "inicio"
            playing = true;
        }

        // Update is called once per frame
        void Update() {
            if (playing) {
                deltaTime += UnityEngine.Time.deltaTime;
                sceneLogic();
            }
        }



        //-------------------PRIVATES-------------------------

        private void sceneLogic() {
            // Hacemos k los alumnos rebeldes ejecuten su animacion y sonido
            if (deltaTime > timeToStart && initAnim) {
                for (int i = 0; i < sceneInfo.problematicStudents; i++) {
                    students[problematicStudents[i]].GetComponent<Animator>().Play(sceneInfo.problematicsAnimations[i].name);
                    if (studentsSex[problematicStudents[i]] == 0)
                    {
                        if (sceneInfo.audiosSituationFemenino.Length > i) teacher.GetComponent<AudioSource>().clip = sceneInfo.audiosSituationFemenino[i];
                    }
                    else
                    {
                        if (sceneInfo.audiosSituationMasculino.Length > i) teacher.GetComponent<AudioSource>().clip = sceneInfo.audiosSituationMasculino[i];
                    }
                    teacher.GetComponent<AudioSource>().Play();
                }
                initAnim = false;
            }
        }


        private void generateChilds() {

            Transform studentsPositions = classInfo.clase.GetComponentInChildren<Transform>().Find("Desks").GetComponentInChildren<Transform>().Find("DeskPositions");

            if (sceneInfo.nGroups > 1)
            {
                // Colocar a los chavales en grupos
                int alumnosPorGrupo = sceneInfo.nStudents / sceneInfo.nGroups;
                int nPupitres = studentsPositions.childCount;
            }

            if (sceneInfo.nStudents > 30) sceneInfo.nStudents = 30;

            students = new GameObject[sceneInfo.nStudents];
            studentsSex = new int[sceneInfo.nStudents];

            // Instanciamos los alumnos en sus posiciones de manera aleatoria.
            for (int i = 0; i < sceneInfo.nStudents; i++) {

                // Elegimos el sexo del estudiante
                GameObject pickedStudent;
                int sex = UnityEngine.Random.Range(0, 2); // 0 mujer, 1 hombre
                if (sex == 0) { 
                    pickedStudent = Instantiate(classInfo.girlsPrefabs[UnityEngine.Random.Range(0, classInfo.girlsPrefabs.Length)], sceneObjects.transform);
                    pickedStudent.name = classInfo.girlsNames[UnityEngine.Random.Range(0, classInfo.girlsNames.Length)];
                }
                else
                {
                    pickedStudent = Instantiate(classInfo.boysPrefabs[UnityEngine.Random.Range(0, classInfo.boysPrefabs.Length)], sceneObjects.transform);
                    pickedStudent.name = classInfo.boysNames[UnityEngine.Random.Range(0, classInfo.boysNames.Length)];
                }

                // Le ponemos el nombre
                pickedStudent.GetComponentInChildren<Transform>().Find("Name").GetComponent<TextMesh>().text = pickedStudent.name;

                // TODO: falta el ordenamiento por grupos ;)
                // Lo colocamos en su pupitre
                Transform pos = studentsPositions.GetComponent<Transform>().GetChild(i);
                pickedStudent.transform.SetPositionAndRotation(pos.position + new Vector3(0, -0.4f, 0), pos.rotation);

                // Lo añadimos al array de estudiantes
                students[i] = pickedStudent;
                studentsSex[i] = sex;
            }

            // Estudiantes problematicos
            problematicStudents = new int[sceneInfo.problematicStudents];

            for(int i = 0; i < sceneInfo.problematicStudents; i++) {
                int problematic = UnityEngine.Random.Range(0, sceneInfo.nStudents);
                problematicStudents[i] = problematic;
                students[problematic].GetComponentInChildren<Transform>().Find("Name").GetComponent<TextMesh>().color = Color.red;
            }


        }
    }
}