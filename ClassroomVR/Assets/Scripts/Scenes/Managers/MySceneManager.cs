using System;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR {
    public class MySceneManager : MonoBehaviour {
        // -----Publics-----
        // Enum para las fases de la escena
        public enum State { AnimSituation, AnimReactSituation, GeneratePathSettings, ChoosingPath, ReactToPath };

        public bool PlayScene;

        // GameObject vacio para colocar los objetos de la escena
        public GameObject sceneObjects;

        // -----Privates-----
        private SoundLoudness soundController;
        private KeyWordRecognizer wordRecognizer;

        // Objetos de la escena
        private GameObject teacher;
        private GameObject[] students;
        private int[] studentsSex;
        private int[] problematicStudents;

        // Otras cosis
        // Bool pause
        private bool _playing = false;
        // Bool para los camino
        private bool pathChosen = false;
        // Estado de la escena
        private State _sceneState;

        //--Path parameters--
        private string pathFeedback;
        private AudioClip pathClip;

        //--Tiempos--
        // DeltaTime
        private float deltaTime = 0f;
        // Tiempo para empezar a ejecutar la situacion
        private float timeToStart = 2.0f;
        // Tiempo que tiene el profe para reaccionar a la situacion
        private float timeToReact = 10.0f;

        //-------------
        // Cosas del gm
        [Tooltip("Esto se lo pasa el gm en funcion del nivel elegido")]
        public ScenePackage sceneInfo; // En realidad es private
        [Tooltip("Esto lo coge del gm")]
        public ClassInfo classInfo; // En realidad es private
        
        //public Canvas canvas; // En realidad lo coge del gm, no existe aqui

        // Start is called before the first frame update
        void Start() {
            soundController = GetComponent<SoundLoudness>();
            wordRecognizer = new KeyWordRecognizer();

            // Generacion de la clase
            Instantiate(classInfo.clase, sceneObjects.transform);
            // Generacion del profesor
            generateTeacher();
            // Generamos los estudiantes
            generateChilds();

            // Mostramos el texto descriptivo de la escena (ademas del boton obvio :P)
            // Obviamente esto no es asi :), 
            // Tendriamos una referencia al canvas y de ahi cogemos el texto que sea el bueno 
            Text iniText = gameObject.AddComponent<Text>(); 
            iniText.text = sceneInfo.iniMessage;

            // Esto se hace al pulsar el boton "inicio"
            _playing = PlayScene;
            _sceneState = State.AnimSituation;  //SIGUIENTE ESTADO
        }

        // Update is called once per frame
        void Update() {
            if (_playing) {
                deltaTime += UnityEngine.Time.deltaTime;
                playSituation();
                playPathChoosing();
                playReactionToPath();
            }
        }

        //-------------------PRIVATES-------------------------
        // Metodo que gestiona la presentacion inicial de la situacion
        private void playSituation()
        {
            // Hacemos k los alumnos rebeldes ejecuten su animacion y sonido
            if (deltaTime > timeToStart)
            {
                if (_sceneState == State.AnimSituation)
                {
                    for (int i = 0; i < sceneInfo.problematicStudents; i++)
                    {
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
                    _sceneState = State.AnimReactSituation; //SIGUIENTE ESTADO
                }
                // Reaccion de la clase
                else if (_sceneState == State.AnimReactSituation && !teacher.GetComponent<AudioSource>().isPlaying)
                {
                    if (sceneInfo.audioReaccionClase != null)
                    {
                        teacher.GetComponent<AudioSource>().clip = sceneInfo.audioReaccionClase;
                        teacher.GetComponent<AudioSource>().Play();
                    }
                    _sceneState = State.GeneratePathSettings;   //SIGUIENTE ESTADO
                    soundController.setCommentFinished();
                    deltaTime = 0;
                }
            }
        }

        // Metodo que gestiona la eleccion del camino tras la presentacion de la situacion
        private void playPathChoosing()
        {
            // Parametros especificos de los caminos
            if(_sceneState == State.GeneratePathSettings)
            {
                Debug.Log("AÑADIR PALABRAS");
                // Añadimos las palabras al reconocimiento de voz
                wordRecognizer.addWordsToKeyWord(sceneInfo.keyWords1, path1Reaction);
                wordRecognizer.addWordsToKeyWord(sceneInfo.keyWords2, path2Reaction);
                wordRecognizer.addWordsToKeyWord(sceneInfo.keyWords3, path3Reaction);
                wordRecognizer.init();
                _sceneState = State.ChoosingPath;   //SIGUIENTE ESTADO
            }
            // Durante la eleccion del camino
            else if(_sceneState == State.ChoosingPath)
            {
                // En casos donde la eleccion del camino es acercarse a los alumnos liantes
                collisionReaction();

                // Si se toma un camino
                if(pathChosen) _sceneState = State.ReactToPath; //SIGUIENTE ESTADO
                // Se acabo el tiempo de tomar una decision
                if (deltaTime > timeToReact) {
                    soundController.StopRecordingAndCalculate();
                    _sceneState = State.ReactToPath;   //SIGUIENTE ESTADO
                }
            }
        }

        // Metodo que gestiona la reaccion al camino elegido
        private void playReactionToPath()
        {
            // Reaccion al camino tomado
            if(_sceneState == State.ReactToPath)
            {

            }
        }


        //--------------Metodos para la generalizacion del camino elegido-------------------
        private void collisionReaction()
        {
            if (sceneInfo.pos1)
            {

            }
            if (sceneInfo.pos2)
            {

            }
            if (sceneInfo.pos3)
            {

            }
        }

        private void path1Reaction()
        {
            Debug.Log("CAMINO1");
            pathFeedback = sceneInfo.feedbackPath1;
            pathClip = sceneInfo.audio1;
            pathChosen = true;
        }

        private void path2Reaction()
        {
            Debug.Log("CAMINO2");
            pathFeedback = sceneInfo.feedbackPath2;
            pathClip = sceneInfo.audio2;
            pathChosen = true;
        }

        private void path3Reaction()
        {
            Debug.Log("CAMINO3");
            pathFeedback = sceneInfo.feedbackPath3;
            pathClip = sceneInfo.audio3;
            pathChosen = true;
        }

        // -------------Metodos de generacion inicial------------------
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

        private void generateTeacher()
        {
            teacher = Instantiate(classInfo.teacher, sceneObjects.transform);
            //teacher = sceneObjects.GetComponent<Transform>().Find("PlayerVR").gameObject;
            Transform teacherIniPos = classInfo.clase.GetComponentInChildren<Transform>().Find("ParquetFloor").Find("ClassPositions").Find("TeacherIni");
            //Debug.Log("POS->" + teacherIniPos.position);
            teacher.transform.position = teacherIniPos.position;
            teacher.transform.Rotate(new Vector3(0, 180, 0));
            //Destroy(sceneObjects.transform.Find("PlayerVR").gameObject);
        }

    }
}