using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ClassRoomVR {
    public class MySceneManager : MonoBehaviour {
        // -----Publics-----
        // Enum para las fases de la escena
        public enum State { AnimSituation, AnimReactSituation, GeneratePathSettings, ChoosingPath, ReactToPath, ShowFeedBack };

        [Tooltip("Booleano para la ejecucion de la situacion")]
        public bool PlayScene;

        // Haz un UI MANAGER
        public UIManager uiManager;

        // GameObject vacio para colocar los objetos de la escena
        public GameObject sceneObjects;

        // -----Privates-----
        private SoundLoudness soundController;
        private KeyWordRecognizer wordRecognizer;

        // Objetos de la escena
        private GameObject _schoolClass;
        private GameObject _teacher;
        private GameObject[] _students;
        private int[] _studentsSex;
        private int[] _problematicStudents;
        private bool[] _asientosOcupados;

        // Control de estados
        // Bool error
        private bool _error = false;
        // Bool pause
        private bool _playing = false;
        // Bool para la escena
        private bool doSceneBehaviourOnce = false;
        // Bool para los caminos
        private bool _pathChosen = false;
        private bool doPathOptionOnce = false;
        // Bool para fin de escena
        private bool _sceneFinished = false;
        // Estado de la escena
        private State _sceneState;

        // Bools para los comportamientos especificos de cada escena
        private bool specialSituatiuon = false;
        private bool specialPath = false;

        //Booleano VR vs TecladoYraton
        private bool VRHardware;

        //--Path parameters--
        private string pathFeedback;
        private AudioClip pathClip;
        private AnimationClip pathAnimClass;
        private AnimationClip pathAnimProb;
        private UnityEvent pathEvent = null;
        private string teacherCollision = "";

        //--Tiempos--
        // DeltaTime
        private float deltaTime = 0f;
        // Tiempo para empezar a ejecutar la situacion
        private float timeToStart = 3.0f;
        // Tiempo que tiene el profe para reaccionar a la situacion
        private float timeToReact = 10.0f;
        // Tiempo de espera para el feedbackFinal
        private float timeToWait = 5.0f;

        // Cosas del gm
        // Especificaciones de la escena a jugar
        private ScenePackage sceneInfo;
        // Especificaciones de la clase a generar
        private ClassInfo classInfo;
        //-------------

        // Start is called before the first frame update
        void Start() {
            sceneInfo = GameManager.Instance.getPack();
            classInfo = GameManager.Instance.getClass();
            VRHardware = GameManager.Instance.getVR();

            soundController = GetComponent<SoundLoudness>();
            wordRecognizer = new KeyWordRecognizer();

            timeToStart = sceneInfo.timeToStart;

            // Generacion de la clase
            try
            {
                _schoolClass = sceneObjects.GetComponent<Transform>().Find("ClassRoom").gameObject;
            }
            catch(Exception e)
            {
                Debug.Log("Fatal Error!: No hay un objeto classroom en los objetos de la escena");
                _error = true;
            }
            // Generacion del profesor
            generateTeacher();
            // Generamos los estudiantes
            generateChilds();

            try
            {
                sceneObjects.GetComponent<Transform>().Find("NavMesh").gameObject.SetActive(true);
            }
            catch(Exception e)
            {
                Debug.Log("Fatal Error!: No hay un navMesh en los objetos de la escena");
                _error = true;
            }

            // Mostramos el texto descriptivo de la escena
            string contexto = "";
            for(int i = 0; i < _problematicStudents.Length; i++) {
                if (i > 0 && i != _problematicStudents.Length-1) contexto += ", ";
                else if(i > 0 && i == _problematicStudents.Length-1) contexto += " y ";
                contexto += _students[_problematicStudents[i]].name;
                if (i > 1 && i == _problematicStudents.Length-1) contexto += ";"; 
            }
           
            contexto += " " + sceneInfo.iniMessage;
            uiManager.panelContexto(contexto);

            // Comprobacion de errores para volver al menu
            if (_error) loadMenu();
        }


        // Update is called once per frame
        void Update() {
            if (_playing) {
                deltaTime += Time.deltaTime;
                playSituation();
                playPathChoosing();
                playReactionToPath();
            }
        }

        //-------------------PUBLICS-------------------------
        //Metodo para cuando se pulsa el boton tras la explicacion de la escena
        public void starplaying()
        {
            _playing = PlayScene;
            _sceneState = State.AnimSituation;  //SIGUIENTE ESTADO
        }

        // Getters
        public GameObject[] getStudents()
        {
            return _students;
        }
        public GameObject[] getProblematics()
        {
            GameObject[] ps = new GameObject[sceneInfo.problematicStudents];
            for(int i = 0; i < _problematicStudents.Length; i++)
            {
                ps[i] = _students[_problematicStudents[i]];
            }
            return ps;
        }
        public GameObject getTeacher()
        {
            return _teacher;
        }
        public GameObject getClass()
        {
            return _schoolClass;
        }
        public bool[] getFreeDesks()
        {
            return _asientosOcupados;
        }

        // Setters (para situaciones especiales)
        public void setSpecialSituation(bool b)
        {
            specialSituatiuon = b;
        }
        public void setSpecialPath(bool b)
        {
            specialPath = b;
        }

        public void setCollision(string s)
        {
            teacherCollision = s;
        }

        // Otros publics
        public void loadMenu()
        {
            GameManager.Instance.LoadMainMenu();
        }
        public void resetScene(int i)
        {
            GameManager.Instance.makeChoice(i);
        }

        public void enablecameraPlayer(bool t)
        {
            _teacher.GetComponent<CameraManager>().enabled = t;
        }


        //-------------------PRIVATES-------------------------
        // METODOS DE CONTROL DE LOGICA DE LA ESCENA

        // Metodo que gestiona la presentacion inicial de la situacion
        private void playSituation()
        {
            // Si pasa el tiempo inicial de espera
            if (deltaTime > timeToStart) {
                // Hacemos k los alumnos rebeldes ejecuten su animacion y sonido
                if (_sceneState == State.AnimSituation)
                {
                    if (!doSceneBehaviourOnce)
                    {
                        for (int i = 0; i < sceneInfo.problematicStudents; i++)
                        {
                            if (sceneInfo.problematicsAnimation != null) _students[_problematicStudents[i]].GetComponent<Animator>().Play(sceneInfo.problematicsAnimation.name);
                            if (_studentsSex[_problematicStudents[i]] == 0)
                            {
                                if (sceneInfo.audioSituationFemenino != null) _teacher.GetComponent<AudioSource>().clip = sceneInfo.audioSituationFemenino;
                            }
                            else
                            {
                                if (sceneInfo.audioSituationMasculino != null) _teacher.GetComponent<AudioSource>().clip = sceneInfo.audioSituationMasculino;
                            }
                            _teacher.GetComponent<AudioSource>().Play();

                        }
                        doSceneBehaviourOnce = true;
                    }
                    // Comportamiento especial
                    if (sceneInfo.especificBehaviour.GetPersistentEventCount() > 0)
                    {
                        sceneInfo.especificBehaviour.Invoke();
                        if(specialSituatiuon) _sceneState = State.AnimReactSituation; //SIGUIENTE ESTADO
                    }
                    else
                    {
                        _sceneState = State.AnimReactSituation; //SIGUIENTE ESTADO
                    }
                }
                // Reaccion de la clase
                else if (_sceneState == State.AnimReactSituation && !_teacher.GetComponent<AudioSource>().isPlaying)
                {
                    if (sceneInfo.audioReaccionClase != null)
                    {
                        _teacher.GetComponent<AudioSource>().clip = sceneInfo.audioReaccionClase;
                        _teacher.GetComponent<AudioSource>().Play();
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
                for (int i = 0; i < sceneInfo.paths.Length; i++)
                {
                    // Mostramos los caminos a tomar
                    uiManager.panelOpciones(sceneInfo.paths[i].pathInfo);
                    // Añadimos las palabras al reconocimiento de voz
                    wordRecognizer.addWordsToKeyWord(sceneInfo.paths[i].keyWords, i, pathReaction);
                }
                
                wordRecognizer.init();
                setCollision("");
                _sceneState = State.ChoosingPath;   //SIGUIENTE ESTADO
            }
            // Durante la eleccion del camino
            else if(_sceneState == State.ChoosingPath)
            {
                // En casos donde la eleccion del camino es acercarse a los alumnos liantes
                collisionReaction();

                // Si se toma un camino
                if(_pathChosen) _sceneState = State.ReactToPath; //SIGUIENTE ESTADO
                // Se acabo el tiempo de tomar una decision
                if (deltaTime > timeToReact) {
                    Debug.Log("Se acabo el tiempo de reaccion");
                    // Si alguno de los caminos era ignorar
                    for(int i = 0; i < sceneInfo.paths.Length; i++) if (sceneInfo.paths[i].ignore) pathReaction(i);

                    soundController.StopRecordingAndCalculate();
                    deltaTime = 0;
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
                if (!doPathOptionOnce)
                {
                    // Audio de respuesta de los estudiantes
                    if (pathClip != null) {
                        _teacher.GetComponent<AudioSource>().clip = pathClip;
                        _teacher.GetComponent<AudioSource>().Play();
                    }
                    // Animaciones de respuesta de los estudiantes
                    if (pathAnimClass != null) PlayAnimationsAtDifferentTimeClass(pathAnimClass.name);
                    if (pathAnimProb != null) PlayAnimationsAtDifferentTimeProblematic(pathAnimProb.name);
                    doPathOptionOnce = true;
                }

                // Comportamiento especial del camino
                if (pathEvent.GetPersistentEventCount() > 0)
                {
                    pathEvent.Invoke();
                    if(specialPath) _sceneState = State.ShowFeedBack;   //SIGUIENTE ESTADO
                }
                else
                {
                    _sceneState = State.ShowFeedBack;   //SIGUIENTE ESTADO
                }
            }
            else if (_sceneState == State.ShowFeedBack)
            {
                //Debug.Log("FEEDBACK");
                // Si no esta el audio ejecutandose se muestra el feedback
                if (!_teacher.GetComponent<AudioSource>().isPlaying && deltaTime > timeToWait) {
                    //textContexto
                    //textContexto.text = pathFeedback;
                    uiManager.endPanel();
                    uiManager.panelContexto(pathFeedback);
                    enablecameraPlayer(false);
                    _sceneFinished = true;
                    _playing = false;
                }
            }
        }
        //----------------------------------------------------------------------------------
        //--------------Metodos para la generalizacion del camino elegido-------------------

        // Metodo para detectar colisiones del teacher con los alumnos
        private void collisionReaction()
        {
            for (int i = 0; i < sceneInfo.paths.Length; i++)
            {
                // Si has chocado con el alumno liante indicarlo de alguna forma
                if (sceneInfo.paths[i].getClose && teacherCollision == _students[_problematicStudents[0]].name) pathReaction(i);
            }
        }

        // Metodo que se llama al detectarse una palabra
        private void pathReaction(int i)
        {
            Debug.Log("CAMINO " + (i+1));
            pathFeedback = sceneInfo.paths[i].feedbackPath;
            pathClip = sceneInfo.paths[i].audio;
            pathAnimClass = sceneInfo.paths[i].pathClassAnimation;
            pathAnimProb = sceneInfo.paths[i].pathProbAnimation;
            pathEvent = sceneInfo.paths[i].especificBehaviour;
            _pathChosen = true;
            uiManager.setOptions(false);
        }

        // Metodo para ejecutar animaciones en diferente timing
        private void PlayAnimationsAtDifferentTimeProblematic(string animName)
        {
            // Play animations at different time
            float time = 0.0f;
            foreach (int prob in _problematicStudents)
            {
                time = time + 1f / 8;
                _students[prob].GetComponent<Animator>().Play(animName, 0, time);
            }
        }

        // Metodo para ejecutar animaciones en diferente timing
        private void PlayAnimationsAtDifferentTimeClass(string animName)
        {
            // Play animations at different time
            float time = 0.0f;
            foreach (GameObject s in _students)
            {
                time = time + 1f / 8;
                s.GetComponent<Animator>().Play(animName, 0, time);
            }
        }

        // -------------Metodos de generacion inicial------------------
        private void generateChilds() {
            Transform studentsPositions = null;
            try {
                studentsPositions = _schoolClass.GetComponentInChildren<Transform>().Find("Desks").GetComponentInChildren<Transform>().Find("DeskPositions");
            }
            catch(Exception e) {
                Debug.LogError("FATAL ERROR");
                Debug.LogError("No estan declaradas las posiciones de los alumnos en el prefab de la clase.");
                _error = true;
            }

            if (sceneInfo.nGroups > 1) {
                // Colocar a los chavales en grupos
                int alumnosPorGrupo = sceneInfo.nStudents / sceneInfo.nGroups;
                int nPupitres = studentsPositions.childCount;
            }

            if (sceneInfo.nStudents > 30) sceneInfo.nStudents = 30;
            _asientosOcupados = new bool[studentsPositions.childCount];

            _students = new GameObject[sceneInfo.nStudents];
            _studentsSex = new int[sceneInfo.nStudents];

            // Instanciamos los alumnos en sus posiciones de manera aleatoria(el prefab).
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
                try
                {
                    pickedStudent.GetComponentInChildren<Transform>().Find("Name").GetComponent<TextMesh>().text = pickedStudent.name;
                    pickedStudent.GetComponentInChildren<Transform>().Find("Collider").name = pickedStudent.name;
                } 
                catch(Exception e)
                {
                    Debug.Log("Al alumno " + pickedStudent.name + " le faltan componentes");
                    _error = true;
                }

                // TODO: falta el ordenamiento por grupos ;)
                // Lo colocamos en su pupitre
                Transform pos = studentsPositions.GetComponent<Transform>().GetChild(i);
                pickedStudent.transform.SetPositionAndRotation(pos.position + new Vector3(0, -0.4f, 0), pos.rotation);
                //Debug.Log("POS: " + pos.position);

                // Lo añadimos al array de estudiantes
                _students[i] = pickedStudent;
                _asientosOcupados[i] = true;
                _studentsSex[i] = sex;
            }

            // Estudiantes problematicos
            _problematicStudents = new int[sceneInfo.problematicStudents];
            int problematic = -1;
            for(int i = 0; i < sceneInfo.problematicStudents; i++) {
                // En caso de que se tengan que sentar juntos
                if (sceneInfo.problematicTogether) {
                    // Condicion inicial para el primero
                    if (problematic == -1)
                    {
                        problematic = UnityEngine.Random.Range(0, sceneInfo.nStudents);
                    }
                    // Colocacion de los demas alrededor del anterior problematico
                    else
                    {
                        int a = -1;
                        do
                        {
                            a = UnityEngine.Random.Range(0, 4);
                            switch (a)
                            {
                                case 0:
                                    a = 1;
                                    break;
                                case 1:
                                    a = -1;
                                    break;
                                case 2:
                                    a = 5;
                                    break;
                                case 3:
                                    a = -5;
                                    break;
                                default:
                                    break;
                            }
                        } while (problematic + a > sceneInfo.nStudents - 1 || problematic + a < 0);

                        problematic += a;
                    }
                }
                else
                {
                    problematic = UnityEngine.Random.Range(0, sceneInfo.nStudents);
                }
                _problematicStudents[i] = problematic;
                _students[problematic].GetComponentInChildren<Transform>().Find("Name").GetComponent<TextMesh>().color = Color.red;
            }

            // Ejecutamos animaciones con distinto timing
            PlayAnimationsAtDifferentTimeClass(classInfo.idleAnim.name);
        }

        private void generateTeacher()
        {
            if (VRHardware)
            {
                _teacher = sceneObjects.GetComponent<Transform>().Find("PlayerVR").gameObject;
                
            }
            else _teacher = sceneObjects.GetComponent<Transform>().Find("Player").gameObject;

            _teacher.SetActive(true);
        }
    }
}