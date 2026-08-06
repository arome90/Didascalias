using Didascalia;
using Didascalia.Student;
using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// <summary>
/// Maneja la configuraci�n de la clase y la aplica seg�n corresponda.
/// Encargada de hacer la disposici�n de los escritorios.
/// </summary>
public class ClassManager : Singleton<ClassManager>
{
    [Header("Class Settings")]
    [SerializeField,
        Tooltip("Configuraci�n del aula actual")] 
    private ClassSettings _settings;

    /// <summary>
    /// Configuraci�n del aula actual
    /// </summary>
    public ClassSettings Settings { get { return _settings; } }

    [Header("Class Objects")]
    [SerializeField,
        Tooltip("Prefab utilizado para los escritorios")] 
    private GameObject _deskPrefab;

    [SerializeField,
        Tooltip("Punto central de la clase, desde la cu�l se generar�n los escritorios.")]
    private Transform _classRoot;

    [SerializeField,
        Tooltip("Punto central de la clase, desde la cu�l se generar�n los escritorios.")]
    private ClassMaterial _classMaterialTransform;
    public void SetClassMaterial(ClassMaterial mat) { _classMaterialTransform = mat; }

    [SerializeField,
        Tooltip("Donde se encuentra la puerta de la clase")]
    private Door _frontDoor;

    [SerializeField,
        Tooltip("Donde se encuentra la puerta de la clase")]
    private Door _backDoor;

    public void SetFrontDoor(Door front) { _frontDoor = front; }
    public void SetBackDoor(Door back) { _backDoor = back; }



    /// <summary>
    /// Lista de los escritorios generados
    /// </summary>
    private List<GameObject> _desks = null;

    private float _deskDistance;

    public float DeskDistance { get { return _deskDistance; } }

    // M�todos p�blicos con los que cambiar las settings
    #region Settings Change

    public void SetBoysNumber(int numBoys)
    {
        _settings.NumBoys = numBoys;
    }

    public void SetBoysNumber(Single numBoys, Single _)
    {
        SetBoysNumber((int)numBoys);
    }

    public void SetGirlsNumber(int numGirls)
    {
        _settings.NumGirls = numGirls;
    }

    public void SetGirlsNumber(Single numGirls, Single _)
    {
        SetGirlsNumber((int)numGirls);
    }

    public void SetNumDeks(int numDesks)
    {
        _settings.NumDesks = numDesks;
    }

    public void SetRows(int rows)
    {
        _settings.Rows = rows;
    }

    public void SetCols(int cols)
    {
        _settings.Cols = cols;
    }

    public void SetRadius(float radius)
    {
        _settings.Radius = radius;
    }

    public void SetShape(ClassSettings.Shape shape)
    {
        _settings.ClassShape = shape;
    }

    public void SetShape(int shape)
    {
        _settings.ClassShape = (ClassSettings.Shape)shape;
    }
    #endregion

    // M�todos p�blicos para UI con los que cambiar las settings
    #region UI_Layout_Generation 

    public void SetRowsAndGenerate(Single rows, Single _)
    {
        SetRows((int)rows);
        GenerateClass();
    }

    public void SetColsAndGenerate(Single cols, Single _)
    {
        SetCols((int)cols);
        GenerateClass();
    }

    public void SetDesksAndGenerate(Single numDesks, Single _)
    {
        SetNumDeks((int)numDesks);
        GenerateClass();
    }

    public void SetRadiusAndGenerate(Single radius, Single _)
    {
        SetRadius(radius);
        GenerateClass();
    }

    public void GenerateClass()
    {
        ArrangeClass(_settings.ClassShape);
    }

    public void ArrangeClass(int type)
    {
        ArrangeClass((ClassSettings.Shape)type);
    }
    #endregion

    [Header("Generation Settings")]
    [SerializeField,
        Tooltip("Distancia desde el centro de la clase hasta cada lateral de la misma. Utilizado para saber c�mmo colocar los escritorios"), 
        Range(2.0f, 40.5f)]
    private float _classWidth = 3.4f;

    /// <summary>
    /// Método PROVISIONAL de pausa de juego.
    /// está fatal hecho
    /// </summary>
    public static void PauseGame()
    {
        Time.timeScale = 0.0f;
    }

    /// <summary>
    /// Método PROVISIONAL de 'unpause' de juego.
    /// está fatal hecho
    /// </summary>
    public static void ResumeGame()
    {
        Time.timeScale = 1.0f;
    }

    /// <summary>
    /// Devuelve la posici�n de la puerta de la clase
    /// </summary>
    /// <returns> Posici�n de la puerta de la clase </returns>
    public Door FrontDoor => _frontDoor;
    public Door BackDoor => _backDoor;

    public Transform ClassMaterialTransform => _classMaterialTransform.transform;

    /// <summary>
    /// Llamamos a este m�todo para generar una clase con escritorios seg�n
    /// una forma dada
    /// </summary>
    /// <param name="shape"> La forma de la clase </param>
    public void ArrangeClass(ClassSettings.Shape shape)
    {
        SetShape(shape);
        
        // Casos especiales
        if(_settings.NumDesks == 1)
        {
            CleanClass();

            _desks.Add(Instantiate(_deskPrefab, _classRoot));
            _deskDistance = 0.0f;
            return;
        }
        
        // Casos normales
        switch (shape) {
            case ClassSettings.Shape.Square:
                ArrangeSquareClass();
                break;
            case ClassSettings.Shape.Circular:
                ArrangeCircularClass();
                break;
            case ClassSettings.Shape.U:
                ArrangeUClass();
                break;
        }

        _deskDistance = (_desks[0].transform.position - _desks[1].transform.position).magnitude;
    }

    private void Start()
    {
        if (SceneChanger.Exists)
        {
            SceneChanger.Instance.OnSceneChanged.AddListener(ActivateClassOnSceneChanged);
        }
        else
        {
            Didascalia.Utils.Log.Warning("Voice Activation not found on enable", this);
        }
    }

    private void OnDestroy()
    {
        if (SceneChanger.Exists)
        {   
            SceneChanger.Instance.OnSceneChanged.RemoveListener(ActivateClassOnSceneChanged);
        }
        else
        {
            Didascalia.Utils.Log.Warning("Voice Activation not found on enable", this);
        }
    }

    /// <summary>
    /// Genera la clase y sus estudiantes cuando la escena
    /// cambia de la llamada "Menu" a la de "Clase"
    /// </summary>
    /// <param name="menuScene"> Escena del men� </param>
    /// <param name="classScene"> Escena de la clase </param>
    void ActivateClassOnSceneChanged(string menuScene, Scene classScene)
    {
        if (
            // FIXME: this is a temporary solution, we should find a better way to determine when to generate the class
            (classScene.name == "Class" || classScene.name == "newClass")
            && (menuScene == "Menu" || menuScene == "newMenu")
        ) {
            StartClass();
        }
    }

    /// <summary>
    /// Quita los escritorios de la clase y deja vac�a la lista de escritorios
    /// </summary>
    public void CleanClass()
    {
        // Cleaning
        if (_desks != null)
        {
            foreach (GameObject obj in _desks) { Destroy(obj); }
            _desks.Clear();
        }
        else
        {
            _desks = new List<GameObject>();
        }
    }

    /// <summary>
    /// Genera una clase con escritorios en forma cuadrada
    /// </summary>
    void ArrangeSquareClass()
    {
        CleanClass();
        int rows = _settings.Rows;
        int cols = _settings.Cols;

        // 5 es el n�mero m�ximo de columnas y filas
        // Me gustar�a ver c�mo incorporarlo de otra forma
        // que no sea a mano
        float generalOffset = _classWidth / 5.0f;

        Vector3 startingPoint = _classRoot.position + 
            new Vector3(cols *generalOffset, 0, rows * generalOffset);

        float xDeskOffset = _classWidth / 2;
        float zDeskOffset = _classWidth / 2;

        for (int i = 0; i < rows && _desks.Count < _settings.NumDesks; ++i)
        {
            Vector3 currentPoint = startingPoint;
            for (int j = 0; j < cols && _desks.Count < _settings.NumDesks; ++j) 
            {
                AddDesk(currentPoint, Quaternion.identity);
                currentPoint += Vector3.right * -xDeskOffset; 
            }
            startingPoint += Vector3.forward * -zDeskOffset;
        }
        Didascalia.Utils.Log.Warning("outdated: Square disposition of desks is outdated", this);
    }

    /// <summary>
    /// Genera una clase con escritorios en forma circular, mirando hacia el centro
    /// </summary>
    void ArrangeCircularClass()
    {
        CleanClass();

        float angleIncrement = Mathf.Deg2Rad * (360.0f / _settings.NumDesks);

        for(int i = 0; i < _settings.NumDesks; ++i)
        {
            Vector3 currentPosition = _classRoot.position +
                Vector3.right * _settings.Radius * Mathf.Cos(angleIncrement * i) +
                Vector3.forward * _settings.Radius * Mathf.Sin(angleIncrement * i);

            Quaternion rot = Quaternion.LookRotation(_classRoot.position - currentPosition);

            AddDesk(currentPosition, rot);
        }
        Didascalia.Utils.Log.Warning("outdated: Circular disposition of desks is outdated", this);
    }

    /// <summary>
    /// Genera una clase con escritorios en forma de U, mirando hacia la pizarra
    /// </summary>
    void ArrangeUClass()
    {
        CleanClass();

        int desksInCircle = (Mathf.Min(_settings.NumDesks, _settings.MaxDesksInSemiCircle));
        if(desksInCircle == _settings.MaxDesksInSemiCircle && _settings.NumDesks % 2 == 0)
        {
            // restamos 1 para que no quede un lado m�s largo que otro en la U
            //
            // ||
            // ||     ||       <== Para evitar esto
            //   || ||
            desksInCircle--;
        }

        float angleIncrement = Mathf.Deg2Rad * (180.0f / (desksInCircle - 1));

        Vector3 currentPosition = _classRoot.position;

        for (int i = 0; i < desksInCircle; ++i)
        {
            currentPosition = _classRoot.position +
                Vector3.right * _settings.Radius * Mathf.Cos(angleIncrement * i) -
                Vector3.forward * _settings.Radius * Mathf.Sin(angleIncrement * i);

            AddDesk(currentPosition, Quaternion.identity);
        }

        int desksInLine = _settings.NumDesks - desksInCircle;

        for(int i = 0; i < desksInLine; ++i)
        {
            Vector3 dir;
            if (i % 2 == 0) dir = Vector3.right * _settings.Radius; 
            else dir = -Vector3.right *_settings.Radius;

            currentPosition = _classRoot.position + dir;
            currentPosition += Vector3.forward * (_settings.Radius / 2.0f) * (i/2 + 1);
            
            AddDesk(currentPosition, Quaternion.identity);
        }
        Didascalia.Utils.Log.Warning("outdated: U disposition of desks is outdated", this);
    }

    /// <summary>
    /// A�ade un escritorio a la clase
    /// Lo hace hijo del Class Root
    /// </summary>
    /// <param name="position"> Posici�n del escritorio </param>
    /// <param name="rot"> Rotaci�n del escritorio </param>
    void AddDesk(Vector3 position, Quaternion rot)
    {
        _desks.Add(Instantiate(_deskPrefab, position, rot));
        _desks[_desks.Count - 1].transform.parent = _classRoot;
    }

    /// <summary>
    /// Agrega a los estudiantes en cada escritorio segun las Class Settings
    /// </summary>
    void AddStudentsToDesks()
    {
        List<Student> students = StudentManager.Instance.GenerateStudents();
        int i = 0;
        foreach (Student st in students)
        {
            // setting the proper desk animator
            // we must search the animator prior to setting the student's position, since the deskAnimator
            // would the the student animator instead. that's sad
            Animator deskAnimator = _desks[i].GetComponent<Animator>();
            if (deskAnimator == null) deskAnimator = _desks[i].GetComponentInChildren<Animator>();
            st.GetComponent<StudentAnimatorController>().SetDeskAnimator(deskAnimator);

            st.Desk = _desks[i].GetComponent<Desk>();
            st.OriginalDesk = st.Desk;

            // setting position
            st.transform.parent = st.Desk.StudentPosition;
            st.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            // setting up navigation/pathfinding
            NavMeshAgent agent = st.GetComponent<NavMeshAgent>();
            agent.Warp(st.transform.position);
            agent.enabled = true;

            i++; // since there are always more desks than students, this is fine :)
        }

        FindAnyObjectByType<NavMeshSurface>().BuildNavMesh();
    }

    public void ResolveConflicts()
    {
        StudentManager.Instance.ResolveConflicts();
    }

    public void RemoveAllConflicts()
    {
        StudentManager.Instance.RemoveAllConflicts();
    }

    public void SendData(string action, string type, List<string> alumnxs)
    {
        EventData d = new EventData(action, type, alumnxs);
        GameDataManager.Instance.SendData(d);
    }

    public void RestartClass()
    {
        ScreenFader.Instance.FadeOut(RestartClassOnFadeOut);
    }
    
    public void StartClass() {
        GenerateClass();
        AddStudentsToDesks();
        if (ConnectionManager.Exists) ConnectionManager.Instance.ClassStarted();
    }

    private void RestartClassOnFadeOut()
    {
        RemoveAllConflicts();
        StartClass();

        ScreenFader.Instance.FadeIn();
    }

    public void OnWebEventCalled(ReceivedWebMessage message)
    {
        Didascalia.Utils.Log.Info("WebMessageType: " + message.id, this);
        
        string actionInfo = "Web Event Called";
        string typeInfo = message.id.ToString();
        List<string> alumnxsInfo = new List<string>();
        alumnxsInfo.Add(message.studentName);

        SendData(actionInfo, typeInfo, alumnxsInfo);

        StudentManager.ConflictGenerationResult result = default;
        switch (message.id)
        {
            case WebEventType.Message:
                Didascalia.Utils.Log.Info("Student Name: " + message.studentName, this);
                StudentManager.Instance.MakeStudentTalk(message.studentName, message.data);
                break;
            // TODO: what do we do on restart
            case WebEventType.Restart:
                RestartClass();
                break;
            default:
            {
                StudentManager.ConflictType Unrecognized()
                {    
                    Didascalia.Utils.Error.DebugbreakFailMessage("WebMessageType not recognized: " + message.id, this);
                    return StudentManager.ConflictTypeNonFeasible;
                }
                StudentManager.ConflictType type = message.id switch
                {
                    WebEventType.Disrespect => StudentManager.ConflictType.Disrespect,
                    WebEventType.SitTogether => StudentManager.ConflictType.SitTogether,
                    WebEventType.StandUp => StudentManager.ConflictType.StandUp,

                    WebEventType.Hyperstimulation => StudentManager.ConflictType.Hyperstimulation,
                    WebEventType.Frustration => StudentManager.ConflictType.Frustration,

                    WebEventType.Disorganization => StudentManager.ConflictType.Disorganization,
                    WebEventType.Impulsivity => StudentManager.ConflictType.Impulsivity,
                    WebEventType.Inattention => StudentManager.ConflictType.Inattention,

                    _ => Unrecognized()
                };

                result = StudentManager.Instance.GenerateConflict(type, message.studentName);
                break;
            }
        }

        if (result.Error != StudentManager.ConflictGenerationError.None)
        {
            string OutOfRange()
            {
                Didascalia.Utils.Error.DebugbreakFailMessage($"ConflictGenerationError not recognized: {result.Error}", this);
                return "Unknown Student(s)";
            }
            StudentManager.ConflictType type = result.Descriptor.Type & ~StudentManager.ConflictTypeNonFeasible;
            string studentName = type switch
            {
                StudentManager.ConflictType.Disrespect => result.Descriptor.Disrespect.StudentName,
                StudentManager.ConflictType.StandUp => result.Descriptor.StandUp.StudentName,
                StudentManager.ConflictType.SitTogether => result.Descriptor.SitTogether.StudentName,

                StudentManager.ConflictType.Hyperstimulation => result.Descriptor.Hyperstimulation.StudentName,
                StudentManager.ConflictType.Frustration => result.Descriptor.Frustration.StudentName,
                
                StudentManager.ConflictType.Disorganization => result.Descriptor.Disorganization.StudentName,
                StudentManager.ConflictType.Impulsivity => result.Descriptor.Impulsivity.StudentName,
                StudentManager.ConflictType.Inattention => result.Descriptor.Inattention.StudentName,
                _ => OutOfRange()
            };
            Didascalia.Utils.Log.Warning(
                $"Conflict of type {type} is not feasible for student {studentName} "
                + $"due to error of type: {result.Error}. Conflict will not be generated.",
                this
            );
        }
    }

    void OnDrawGizmos()
    {
        if (_classRoot == null)
        {
            Didascalia.Utils.Log.Warning(
                "Class Root is not assigned. Please assign a Transform to ClassManager's Class Root field.",
                this
            );
        }
        else
        {
            var previousColor = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                _classRoot.position,
                new Vector3(_classWidth, 0.1f, _classWidth)
            );
            Gizmos.color = previousColor;
        }
    }
}
