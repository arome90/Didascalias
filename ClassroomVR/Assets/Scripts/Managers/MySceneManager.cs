using System;
using UnityEngine;
using System.Collections;


namespace ClassRoomVR {
    public class MySceneManager : MonoBehaviour {
        //------------------------------------------------------------------------
        // -----Publics-----
        // Enum para las fases de la escena
        public enum State { AnimSituation, AnimReactSituation, GeneratePathSettings, ChoosingPath, ReactToPath, ShowFeedBack };

        [Tooltip("Booleano para la ejecucion de la situacion")]
        public bool PlayScene;

        // UI MANAGER
        public UIManager uiManager;
        // Player Movements Manager
        public PlayerMotion playerMotion;
        public OVRPlayerController playerVrMotion;

        // EmoPose (emocion - pose) manager
        public MotionCaptureManager emoPose;

        // GameObject vacio para colocar los objetos de la escena
        public GameObject sceneObjects;
        //------------------------------------------------------------------------
        // -----Privates-----
        // Managers
        // Audio
        private SoundLoudness soundController;
        private KeyWordRecognizer wordRecognizer;

        // Objetos de la escena
        private GameObject _schoolClass;
        private GameObject _teacher;
        private GameObject[] _students;
        private int[] _studentsSex;
        private int[] _problematicStudents;
        private bool[] _asientosOcupados;

        //Objetos para la mirada de los alumnos (Paso intermedio de transforms)
        public GameObject _aimNoVR;
        public GameObject _aimVR;

        // Control de estados
        // Bool error
        private bool _error = false;
        // Bool pause
        private bool _playing = false;
        // Bool para la escena
        private bool doSceneBehaviourOnce = false;
        // Bool para fin de escena
        private bool _sceneFinished = false;
        // Bool para el feedbackFinal
        private bool _endFeedback = false;
        private int _showInterval = 0;
        // Estado de la escena
        private State _sceneState;

        //Booleano VR vs TecladoYraton
        private bool VRHardware;

        //--Path parameters--
        private PathPackage selectedPath = null;
        // Bool para los caminos
        private bool _pathChosen = false;
        private bool doPathOptionOnce = false;
        private string teacherCollision = "";
        // Bools para los comportamientos especificos de cada escena
        private bool specialSituatiuon = false;
        private bool specialPath = false;
        private Vector3 probIniPos;
        private string alumsName = "";

        //--Tiempos--
        // DeltaTime
        private float deltaTime = 0f;
        // Tiempo para empezar a ejecutar la situacion
        private float timeToStart = 3.0f;
        // Tiempo que tiene el profe para reaccionar a la situacion
        private float timeToReact = 10.0f;
        // Tiempo que se tarda en resolver la situacion (de cualquier manera)
        private float timeToResolve = 0.0f;
        // Tiempo de espera para el feedbackFinal
        private float timeToWait = 5.0f;

        // Cosas del gm
        // Especificaciones de la escena a jugar
        private ScenePackage sceneInfo;
        // Especificaciones de la clase a generar
        private ClassInfo classInfo;
        //------------------------------------------------------------------------
        //------------------------------------------------------------------------
        //------------------------------------------------------------------------

        // Start is called before the first frame update
        void Start() {
            // Info necesaria
            sceneInfo = GameManager.Instance.getPack();
            classInfo = GameManager.Instance.getClass();
            VRHardware = GameManager.Instance.getVR();

            try
            {
                soundController = GetComponent<SoundLoudness>();
            }
            catch(Exception e)
            {
                Debug.Log("Fatal Error!: El sceneManager no cuenta con un componente 'SoundLoudness'");
                _error = true;
            }
            // Iniciamos reconocimiento de voz
            wordRecognizer = new KeyWordRecognizer();

            // Iniciamos captura de emoPose
            emoPose.init();
            CSVSerializer.iniFile(sceneInfo.name);

            // Data
            timeToStart = sceneInfo.timeToStart;
            if (timeToStart < 1) _sceneState = State.GeneratePathSettings;
            timeToReact = sceneInfo.timeToReact;
            if (timeToReact == 0) timeToReact = float.MaxValue;

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
            for(int i = 0; i < _problematicStudents.Length; i++) {
                if (i > 0 && i != _problematicStudents.Length-1) alumsName += ", ";
                else if(i > 0 && i == _problematicStudents.Length-1) alumsName += " y ";
                alumsName += _students[_problematicStudents[i]].name;
                if (i > 1 && i == _problematicStudents.Length-1) alumsName += ";"; 
            }

            string t = sceneInfo.iniMessage.Replace("alum", alumsName);
            uiManager.panelContexto(t);

            // Comprobacion de errores para volver al menu
            if (_error) loadMenu();
        }


        // Update is called once per frame
        void Update() {
            if (!_sceneFinished) {
                if (_playing)
                {
                    deltaTime += Time.deltaTime;
                    emoPose.update(Time.deltaTime);
                    playSituation();
                    playPathChoosing();
                    playReactionToPath();
                }
                handleInput();
            }
        }

        private void handleInput()
        {
            // Para la pausa de la escena, en el cambio de show/unshow posibles caminos a elegir
            if (Input.GetKeyUp(KeyCode.Q) && _sceneState == State.ChoosingPath)
            {
                pause();
                uiManager.setOptions(!_playing);
            }

            // Para el cambio de texto en el feedback
            if (Input.GetMouseButtonUp(0) && _sceneState == State.ShowFeedBack) endInfo();

            //Version VR // 
            if (OVRInput.GetUp(OVRInput.Button.Two) && _sceneState == State.ChoosingPath)//Boton B
            {
                pause();
                uiManager.setOptions(!_playing);
            }

            //Boton A
            if (OVRInput.GetUp(OVRInput.Button.Two) && _sceneState == State.ShowFeedBack) endInfo();
            //if (OVRInput.GetUp(OVRInput.Button.Two) && !_playing) startplaying();
        }

        //-------------------PUBLICS-------------------------
        //Metodo para cuando se pulsa el boton tras la explicacion de la escena
        public void startplaying()
        {
            _playing = PlayScene;
            //uiManager.setContext(false);
            playerMotion.enabled = _playing;
            playerVrMotion.EnableLinearMovement = _playing;
            _sceneState = State.AnimSituation;  //SIGUIENTE ESTADO
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

        // Para pausar el juego
        public void pause()//pausar para vR?=
        {
            _playing = !_playing;
            playerMotion.enabled = _playing;
            playerVrMotion.EnableLinearMovement = _playing;
        }

        //-------------------PRIVATES-------------------------
        // Muestra la info del la escena desarrollada
        private void endInfo()
        {

            string pitchChange = "Entre el comienzo de la clase y el desarrollo de la situación crítica el tono de voz se vio modificado un " +
                    (soundController.getSavedAverageSound() * 1000) + " %";
            CSVSerializer.saveData("\n" + pitchChange + "\n");
            CSVSerializer.saveRcogniceWord();
            _endFeedback = true;

            /*
            if (_showInterval == 0)
            {
                string pitchChange = "Entre el comienzo de la clase y el desarrollo de la situación crítica el tono de voz se vio modificado un " +
                    (soundController.getSavedAverageSound() * 1000) + " %";
                CSVSerializer.saveData("\n" + pitchChange+ "\n");
                CSVSerializer.saveRcogniceWord();
                uiManager.changeEndPanel(pitchChange);
            }
            if (_showInterval == 1) uiManager.changeEndPanel(emoPose.getIntInfo(1));
            if (_showInterval == 2) uiManager.changeEndPanel(emoPose.getIntInfo(0));
            if (_showInterval == 3) uiManager.changeEndPanel(emoPose.getIntInfo(2));
            _showInterval++;
            if (_showInterval > 3) _endFeedback = true;
            */
            if (_endFeedback)
            {
                loadMenu();
                //uiManager.showEndButtons();
                _sceneFinished = true;
            }
        }

        // ---METODOS DE CONTROL DE LOGICA DE LA ESCENA---
        // Metodo que gestiona la presentacion inicial de la situacion
        private void playSituation()
        {
            // Si pasa el tiempo inicial de espera
            if (deltaTime > timeToStart) {
                // Hacemos k los alumnos rebeldes ejecuten su animacion y sonido
                if (_sceneState == State.AnimSituation)
                {
                    iniPlaySituation();
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

        // Inicializaciones especiales de la ejecucion de la situacion
        private void iniPlaySituation()
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
        }

        // Metodo que gestiona la eleccion del camino tras la presentacion de la situacion
        private void playPathChoosing()
        {
            timeToResolve += Time.deltaTime;

            iniPlayPathChoosing();
            
            // Durante la eleccion del camino
            if(_sceneState == State.ChoosingPath)
            {
                // En casos donde la eleccion del camino es acercarse a los alumnos liantes
                collisionReaction();

                // Si se toma un camino
                if (_pathChosen) {
                    _sceneState = State.ReactToPath;    //SIGUIENTE ESTADO
                    soundController.StopRecordingAndCalculate();
                } 
                // Se acabo el tiempo de tomar una decision
                if (deltaTime > timeToReact) {
                    //Debug.Log("Se acabo el tiempo de reaccion");
                    // Si alguno de los caminos era ignorar
                    for(int i = 0; i < sceneInfo.paths.Length; i++) if (sceneInfo.paths[i].ignore) pathReaction(i);

                    soundController.StopRecordingAndCalculate();
                    deltaTime = 0;
                    _sceneState = State.ReactToPath;   //SIGUIENTE ESTADO
                }
            }
        }

        // Inicializaciones especiales de la eleccion de camino
        private void iniPlayPathChoosing()
        {
            // Parametros especificos de los caminos
            if (_sceneState == State.GeneratePathSettings)
            {
                emoPose.nextInterval();
                for (int i = 0; i < sceneInfo.paths.Length; i++)
                {
                    // Iniciamos el texto de los caminos a tomar
                    uiManager.initPanelOpciones(sceneInfo.paths[i].pathInfo, alumsName);
                    // Añadimos las palabras al reconocimiento de voz
                    wordRecognizer.addWordsToKeyWord(sceneInfo.paths[i].keyWords, i, pathReaction);
                }

                pause();
                uiManager.setOptions(!_playing);

                wordRecognizer.init();
                setCollision("");
                _sceneState = State.ChoosingPath;   //SIGUIENTE ESTADO
                soundController.startCollecting();
            }
        }

        // Metodo que gestiona la reaccion al camino elegido
        private void playReactionToPath()
        {
            // Reaccion al camino tomado
            if(_sceneState == State.ReactToPath)
            {
                initReactionToPath();
               
                // Comportamiento especial del camino
                if (selectedPath.especificBehaviour.GetPersistentEventCount() > 0)
                {
                    selectedPath.especificBehaviour.Invoke();
                    if(specialPath) _sceneState = State.ShowFeedBack;   //SIGUIENTE ESTADO
                }
                else
                {
                    _sceneState = State.ShowFeedBack;   //SIGUIENTE ESTADO
                }
            }
            else if (_sceneState == State.ShowFeedBack)
            {
                // Si no esta el audio ejecutandose se muestra el feedback
                if (!_teacher.GetComponent<AudioSource>().isPlaying && deltaTime > timeToWait) {
                    emoPose.nextInterval();

                    // Feedback final
                    string text = selectedPath.feedbackPath.Replace("alum", alumsName);
                    uiManager.initEndPanel(text, selectedPath.correctPath, timeToResolve);
                    
                    // Fin game
                    _playing = false;
                    playerMotion.unlockCursor();
                    playerMotion.enabled = _playing;
                    playerVrMotion.EnableLinearMovement = _playing;
                    emoPose.saveIntervalsInfo();
                }
            }
        }

        // Inicializaciones especiales de la reaccion al camino
        private void initReactionToPath()
        {
            if (!doPathOptionOnce)
            {
                emoPose.nextInterval();
                // Audio de respuesta de los estudiantes
                if (selectedPath.audio != null)
                {
                    _teacher.GetComponent<AudioSource>().clip = selectedPath.audio;
                    _teacher.GetComponent<AudioSource>().Play();
                }
                // Animaciones de respuesta de los estudiantes
                if (selectedPath.pathClassAnimation != null) PlayAnimationsAtDifferentTimeClass(selectedPath.pathClassAnimation.name);
                if (selectedPath.pathProbAnimation != null) PlayAnimationsAtDifferentTimeProblematic(selectedPath.pathProbAnimation.name);
                doPathOptionOnce = true;
            }
        }

        //----------------------------------------------------------------------------------
        //--------------Metodos para la generalizacion del camino elegido-------------------

        // Metodo para detectar colisiones del teacher con los alumnos
        private void collisionReaction()
        {
            for (int i = 0; i < sceneInfo.paths.Length; i++) {
                // Si has chocado con el alumno liante indicarlo de alguna forma
                if (sceneInfo.paths[i].getClose && teacherCollision == _students[_problematicStudents[0]].name)
                {
                    Debug.Log("Colision con " + teacherCollision);
                    IEnumerator coroutine = waiter(i);
                    StartCoroutine(coroutine);
                    //pathReaction(i);
                }
            }
        }

        IEnumerator waiter(int i)
        {
            deltaTime = 0;

            CSVSerializer.saveData("\n" + "CAMINO " + (i + 1) + "\n");
            //Wait for 10 seconds, (para darle tiempo al usuario a hablar con el alumno)
            yield return new WaitForSeconds(10);
            Debug.Log("Dentro de la coorrutina");
            pathReaction(i);
        }
        

        // Metodo que se llama al detectarse una palabra
        private void pathReaction(int i)
        {
            Debug.Log("CAMINO " + (i+1));
            selectedPath = sceneInfo.paths[i];
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

            if (sceneInfo.nStudents > 30) sceneInfo.nStudents = 30;
            _asientosOcupados = new bool[studentsPositions.childCount];

            _students = new GameObject[sceneInfo.nStudents];
            _studentsSex = new int[sceneInfo.nStudents];

            int deskPos = 0;

            // Instanciamos los alumnos en sus posiciones de manera aleatoria(el prefab).
            for (int i = 0; i < sceneInfo.nStudents && deskPos < 30; i++) {
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

                // Ordenamiento por grupos
                if (sceneInfo.nGroups > 1)
                {
                    if (deskPos == 2 || deskPos == 7 || deskPos == 12 || deskPos == 17 || deskPos == 22 || deskPos == 27) 
                        deskPos++;
                    if (deskPos == 10 || deskPos == 11 || deskPos == 12 || deskPos == 13 || deskPos == 14) 
                        deskPos = 15;
                }

                // Lo colocamos en su pupitre
                Transform pos = studentsPositions.GetComponent<Transform>().GetChild(deskPos);
                pickedStudent.transform.SetPositionAndRotation(pos.position + new Vector3(0, -0.4f, 0), pos.rotation);

                // Lo añadimos al array de estudiantes
                _students[i] = pickedStudent;
                _asientosOcupados[deskPos] = true;
                _studentsSex[i] = sex;
                deskPos++;
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

                        // FEISIMO (para evitar errores de generacion)
                        if (problematic == 4) problematic -= 1;
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
            }   // end estudiantes problematicos

            probIniPos = _students[_problematicStudents[0]].gameObject.transform.position;

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


        // Getters
        public GameObject[] getStudents()
        {
            return _students;
        }
        public GameObject[] getProblematics()
        {
            GameObject[] ps = new GameObject[sceneInfo.problematicStudents];
            for (int i = 0; i < _problematicStudents.Length; i++)
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

        public Vector3 getProblematicIniPos()
        {
            return probIniPos;
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
    }
}