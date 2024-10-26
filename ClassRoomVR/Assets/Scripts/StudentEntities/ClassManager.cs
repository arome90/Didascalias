using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona la clase de estudiantes en la realidad virtual.
    /// Hereda de <see cref="GenericSingleton{ClassManager}"/>.
    /// </summary>
    public class ClassManager : GenericSingleton<ClassManager>
    {
        [SerializeField] private Transform[] _targetsHead;
        [SerializeField] private AudioClip _beforeClassBell;
        [SerializeField] private AudioClip _afterClassBell;
        [SerializeField] private GameObject _player;
        [SerializeField] private Student _body;
        [SerializeField] private bool _generateOnStart;

        private Transform _studentsPositions;
        private bool[] _asientosOcupados;
        private Dictionary<string, Student> _students;
        private StudentsController _studentsController;
        private ClassInfo _classInfo;
        private List<List<string>> _names;
        private ClassSettings _settings;

        /// <summary>
        /// Método llamado al iniciar el script. Configura el entorno y, si es necesario, genera la clase.
        /// </summary>
        public override void Awake()
        {
            _settings = GameManager.Instance.GetCurrentSettings();
            _studentsController = GetComponent<StudentsController>();
            if (_generateOnStart)
            {
                Generate();
                GameManager.Instance.GetVoiceActivation().Activate();
            }
        }

        /// <summary>
        /// Genera los estudiantes y configura el aula.
        /// </summary>
        public void Generate()
        {
            SetupClassroom();
            GenerateStudents();
            StartClass();
        }

        /// <summary>
        /// Configura el aula y los datos iniciales.
        /// </summary>
        private void SetupClassroom()
        {
            _studentsPositions = DeskManager.Instance.gameObject.transform;
            if (_studentsPositions.childCount == 0)
            {
                SetupDesks();
            }

            _asientosOcupados = new bool[_studentsPositions.childCount];
            _students = new Dictionary<string, Student>();
            _classInfo = GameManager.Instance.GetCurrentClassInfo();
            _names = new List<List<string>>();

            List<ClassInfo.NamesLanguage> a = _classInfo.GetNames();
                
            _names.Add(a[(int)Didascalia_LocalizationManager.CurrentLanguage].femaleNames);
            _names.Add(a[(int)Didascalia_LocalizationManager.CurrentLanguage].maleNames);
        }

        /// <summary>
        /// Inicia la clase configurando el controlador de estudiantes y reproduciendo el sonido de inicio de clase.
        /// </summary>
        private void StartClass()
        {
            _studentsController.SetParameters(_player, _students);
            PlayBellSound(_beforeClassBell);

            if (GameManager.Instance.GetSaveAudio())
            {
                AudioRecorder.StartRecording();
            }
        }

        /// <summary>
        /// Configura los escritorios del aula según el modo de estructura.
        /// </summary>
        private void SetupDesks()
        {
            switch (_settings.StructureMode)
            {
                case StructureMode.Circular:
                    DeskManager.Instance.CreateCircle(_settings.NumStudents, _settings.Radius, _settings.Degrees);
                    break;
                case StructureMode.U:
                    DeskManager.Instance.CreateCircle(_settings.NumStudents, _settings.Radius);
                    break;
                default:
                    DeskManager.Instance.CreateRegularLayout(_settings.NumStudents, _settings.Rows, _settings.Columns);
                    break;
            }
        }

        /// <summary>
        /// Guarda la grabación de audio si corresponde al cerrar la aplicación.
        /// </summary>
        private void OnApplicationQuit()
        {
            if (GameManager.Instance.GetSaveAudio())
            {
                AudioRecorder.SaveRecording();
            }
        }

        /// <summary>
        /// Genera los estudiantes de acuerdo con la configuración proporcionada.
        /// </summary>
        private void GenerateStudents()
        {
            int deskPos = 0;

            switch (_settings.Mode)
            {
                case GenerateMode.Gender:
                    GenerateStudentsByGender(ref deskPos);
                    break;
                case GenerateMode.Personalized:
                    GeneratePersonalizedStudents(ref deskPos);
                    break;
            }

            while(_settings.FillEmptyDesks && deskPos < _studentsPositions.childCount)
            {
                GenerateRandomStudent(ref deskPos);
            }
        }

        /// <summary>
        /// Crea un nuevo estudiante y lo agrega al diccionario.
        /// </summary>
        /// <param name="name">Nombre del estudiante.</param>
        /// <param name="gender">Género del estudiante.</param>
        /// <returns>Instancia del estudiante creado.</returns>
        private Student CreateStudent(string name, Gender gender)
        {
            Student pickedStudent = Instantiate(_body, transform);
            pickedStudent.SetParameters(_player.transform, name, gender);
            _students.Add(name, pickedStudent);
            return pickedStudent;
        }

        /// <summary>
        /// Ubica al estudiante en el escritorio adecuado.
        /// </summary>
        /// <param name="deskPos">Posición del escritorio en el que se ubicará el estudiante.</param>
        /// <param name="student">Estudiante a ubicar.</param>
        private void PlaceStudent(ref int deskPos, Student student)
        {
            DeskManager.Instance.GetFreeDesk(ref deskPos);
            Desk desk = _studentsPositions.GetChild(deskPos).GetComponent<Desk>();
            Transform seatPosition = desk.transform.GetChild(0);
            student.transform.SetPositionAndRotation(seatPosition.position, seatPosition.parent.rotation);
            student.transform.Translate(-new Vector3(0f, 0f, 0.15f), Space.Self);
            student.SetDesk(desk);
            student.SetTargets(_targetsHead);
            _asientosOcupados[deskPos] = true;
            deskPos++;
        }

        /// <summary>
        /// Genera un estudiante aleatorio.
        /// </summary>
        /// <param name="deskPos">Posición del escritorio donde se ubicará el estudiante.</param>
        private void GenerateRandomStudent(ref int deskPos)
        {
            int gender = Random.Range(0, 2);
            GenerateStudent(ref deskPos, (Gender)gender);
        }

        /// <summary>
        /// Genera estudiantes por género.
        /// </summary>
        /// <param name="deskPos">Posición del escritorio en el que se ubicará el estudiante.</param>
        /// <param name="gender">Género de los estudiantes a generar.</param>
        /// <param name="numberOfStudents">Número de estudiantes a generar.</param>
        private void GenerateStudentsByGender(ref int deskPos)
        {
            int generatedMen = 0;
            int generatedWomen = 0;
            for (int i = 0; i < _settings.NumStudents; ++i)
            {
                Gender gender;
                if (_settings.NumWomen > generatedWomen && _settings.NumMen > generatedMen)
                {
                    gender = (Gender)Random.Range(0, 2);
                }
                else if (_settings.NumWomen <= generatedWomen)
                {
                    gender = Gender.Men;
                }
                else
                {
                    gender = Gender.Women;
                }
                GenerateStudent(ref deskPos, gender);
                if (gender == Gender.Women)
                {
                    ++generatedWomen;
                }
                else
                {
                    ++generatedMen;
                }
            }
        }

        /// <summary>
        /// Genera estudiantes personalizados según la configuración.
        /// </summary>
        /// <param name="deskPos">Posición del escritorio en el que se ubicará el estudiante.</param>
        private void GeneratePersonalizedStudents(ref int deskPos)
        {
            Debug.Log("Num Students Generated: " + _settings.Students.Length);
            foreach (var studentInfo in _settings.Students)
            {
                GenerateStudent(ref deskPos, studentInfo.Gender, studentInfo.Name);
            }
        }

        /// <summary>
        /// Selecciona un nombre aleatorio para el estudiante.
        /// </summary>
        /// <param name="gender">Género del estudiante.</param>
        /// <returns>Nombre seleccionado.</returns>
        private string PickRandomName(Gender gender)
        {
            int genderInt = (int)gender;
            int index = Random.Range(0, _names[genderInt].Count);
            string name = _names[genderInt][index];
            _names[genderInt].RemoveAt(index);
            return name;
        }

        /// <summary>
        /// Genera un estudiante y lo ubica en el escritorio adecuado.
        /// </summary>
        /// <param name="deskPos">Posición del escritorio en el que se ubicará el estudiante.</param>
        /// <param name="gender">Género del estudiante.</param>
        /// <param name="name">Nombre del estudiante (opcional).</param>
        private void GenerateStudent(ref int deskPos, Gender gender, string name = null)
        {
            Student student = CreateStudent(name ?? PickRandomName(gender), gender);
            PlaceStudent(ref deskPos, student);
        }

        /// <summary>
        /// Obtiene todos los estudiantes en forma de array.
        /// </summary>
        /// <returns>Array de estudiantes.</returns>
        public Student[] GetStudents()
        {
            return _students.Values.ToArray();
        }

        /// <summary>
        /// Obtiene el controlador de estudiantes.
        /// </summary>
        /// <returns>Instancia de <see cref="StudentsController"/>.</returns>
        public StudentsController GetStudentsController()
        {
            return _studentsController;
        }

        /// <summary>
        /// Reproduce el sonido de la campana.
        /// </summary>
        /// <param name="clip">Clip de audio a reproducir.</param>
        private void PlayBellSound(AudioClip clip)
        {
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
