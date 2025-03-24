using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

namespace ClassRoomVR
{
    public class StudentsController : MonoBehaviour
    {
        // Modo de conversación actual
        private TalkMode _mode;
        private Actions _res;

        // Propiedad para acceder a las resoluciones
        public Actions Resolutions
        {
            get { return _res; }
            set { _res = value; }
        }

        // Diccionario de estudiantes
        private Dictionary<string, Student> _students;
        private List<Student> _raisedHandStudents;

        // Jugador
        private GameObject _player;

        // Posiciones en el aula (serializadas)
        [SerializeField] private Transform _frontSide;
        [SerializeField] private Transform _backCorner;
        [SerializeField] private Transform _door;

        // Acciones disruptivas (serializadas)
        [SerializeField] private DisruptiveAction[] actions;

        // Texto de la interfaz de usuario (serializado)
        [SerializeField] private TMPro.TextMeshProUGUI text;

        // Propiedades para acceder a las posiciones en el aula
        public Transform FrontSide => _frontSide;
        public Transform BackCorner => _backCorner;
        public Transform Door => _door;

        // Método para inicializar los parámetros (jugador y estudiantes)
        public void SetParameters(GameObject player, Dictionary<string, Student> students)
        {
            _player = player;
            _students = students;
            foreach (var item in _students)
            {
                item.Value.SetController(this);
            }
            studentList = null;
        }

        public void AddHandRaisedStudent(Student student)
        {
            if(!_raisedHandStudents.Contains(student)) 
                _raisedHandStudents.Add(student);
        }
        public void RemoveHandRaisedStudent(Student student)
        {
            if (_raisedHandStudents.Contains(student))
                _raisedHandStudents.Remove(student);
        }

        // Método para cambiar a dos estudiantes de lugar
        public void ChangeDesk(Student student1, Student student2)
        {
            var position1 = student1.GetDesk();
            var position2 = student2.GetDesk();
            StartCoroutine(student2.ChangeDesk(position1));
            StartCoroutine(student1.ChangeDesk(position2));
        }

        // Método para buscar un estudiante por nombre (manejo de diacríticos)
        public bool TryGetStudent(string name, out Student student)
        {
            student = null;
            if (_students.ContainsKey(name))
            {
                student = _students[name];
                return true;
            }
            return false;
        }

        // Método para hacer que los estudiantes no problemáticos salgan del aula
        public void GoOut()
        {
            int i = 0;
            foreach (Student student in _students.Values.Where(s => !s.IsProblematicStudent()))
            {
                i++;
                StartCoroutine(WaitAndExit(student, i));
            }
        }

        // Corutina para esperar y luego hacer que un estudiante salga del aula
        IEnumerator WaitAndExit(Student student, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            student.MoveTo(_door.position, 0.5f);
        }

        // Maneja la acción de sentarse para los estudiantes
        public void HandleSit(List<Student> studentList)
        {
            foreach (var student in studentList)
            {
                student.SitBack();
            }
        }

        // Maneja la acción de moverse para los estudiantes
        public void HandleMove(List<Student> studentList, string place = null)
        {
            Transform position = Place(place);
            if (position == null) return;
            studentList.ForEach(student => student.MoveTo(position.position, 1.5f));
        }

        // Maneja el cambio de lugar de los estudiantes
        public void HandleChange(List<Student> studentList)
        {
            if (studentList.Count > 1)
            {
                ChangeDesk(studentList[0], studentList[1]);
            }
        }

        // Maneja la acción de posponer
        public void HandlePostpone()
        {
            Debug.Log("Postpone situation");
            _mode = TalkMode.Good;
        }

        // Maneja la expulsión de estudiantes
        public void HandleExpel(List<Student> studentList)
        {
            studentList.ForEach(student => student.MoveTo(_door.position, 0.5f));
        }

        // Maneja el llamado de atención a un estudiante
        public void HandleCall(Student student)
        {
            student.PayAttention();
            student.SetColor(Color.blue);
            student.HandleCallOnRaisedHand();
            StartCoroutine(ReturnColor(student));
        }

        // Corutina para devolver el color original al estudiante después de 5 segundos
        IEnumerator ReturnColor(Student student)
        {
            yield return new WaitForSeconds(3.2f);
            student.SetColor(Color.white);
        }

        // Determina la posición según una descripción textual
        public Transform Place(string place)
        {
            Transform position = null;
            switch (place)
            {
                case "Fondo":
                    position = _backCorner;
                    break;
                case "esquina":
                    position = _frontSide;
                    break;
                case "Fuera":
                    position = _door;
                    break;
                case "Aquí":
                    position = _player.transform;
                    break;
            }
            return position;
        }

        // Obtiene el modo de conversación actual
        public TalkMode GetMode()
        {
            return _mode;
        }

        // Establece el modo de conversación actual
        public void SetMode(TalkMode value)
        {
            _mode = value;
        }

        // Objeto para la acción disruptiva actual
        private GameObject actionObject;
        private DisruptiveAction currentAction;
        private List<Student> studentList;

        // Realiza una acción disruptiva sobre los estudiantes
        public void DoSomethingDisruptive(int index)
        {
            Debug.Log("Trying something disruptive");
            if (currentAction == null && actionObject == null)
            {
                Debug.Log("Choosing new action to perform");
                currentAction = actions[index];
                _res = Actions.None;

                string actionInfo = "Comportamiento disrruptivo!";
                string eventType = "";
                string alumnx = "";
                switch (currentAction.Action)
                {
                    case Actions.Insultar: {
                        Debug.Log("Insultando");
                        eventType = "INSULT";
                        Student s = GetRandomStudentExcluding();
                        alumnx += "\"";
                        alumnx += s.GetName();
                        alumnx += "\"";
                        StartCoroutine(ActionsMethod.Insult(s, currentAction, CreateConflict));
                    }
                        break;
                    case Actions.Separados:
                        Debug.Log("Separándonos");
                        eventType = "TALK";
                        alumnx = "";
                        GetRandomStudentsSeparate();
                        for (int i = 0; i < studentList.Count; i++){
                            alumnx += "\"";
                            alumnx += studentList[i].GetName();
                            alumnx += "\"";
                            if (i < studentList.Count - 1) alumnx += ", ";
                        }
                        StartCoroutine(ActionsMethod.SitTogether(studentList[0], studentList[1], studentList[2], currentAction, CreateConflict));
                        break;
                    case Actions.Levantarse: {
                        Debug.Log("Levantándose");
                        eventType = "STAND_UP";
                        Student s = GetRandomStudentExcluding();
                        alumnx += "\"";
                        alumnx += s.GetName();
                        alumnx += "\"";
                        ActionsMethod.StandUpAndMove(s, currentAction, _frontSide.position, CreateConflict);
                    }
                        break;
                    default:
                        Debug.LogError(currentAction.name + " action is not implemented or its type is missing. Check it!");
                        eventType += "No implementado.";
                        currentAction = null;
                        break;
                }
                InputLogger.Instance.WriteToJson(actionInfo, eventType, alumnx);
            }
        }

        // Crea un conflicto a partir de la acción disruptiva
        private void CreateConflict()
        {
            Debug.Log("Creating a conflict");
            actionObject = Instantiate(currentAction.BehaviorHolder);
            actionObject.GetComponent<Action>().SetParameters(_player, studentList, currentAction, text);
            actionObject = null;
            currentAction = null;

            foreach (var student in studentList)
            {
                student.SetProblematicStudent();
                student.PayAttention();
            }
        }

        // Obtiene un estudiante aleatorio, excluyendo uno ya seleccionado
        private Student GetRandomStudentExcluding()
        {
            Student exclude = studentList?.FirstOrDefault();
            studentList?.Clear();

            List<Student> eligibleStudents = _students.Values
                .Where(s => s != exclude && s.GetState() != State.Standing)
                .ToList();

            studentList = new List<Student> { eligibleStudents[UnityEngine.Random.Range(0, eligibleStudents.Count)] };
            return studentList.First();
        }

        // Obtiene estudiantes aleatorios para separarlos
        private List<Student> GetRandomStudentsSeparate()
        {
            var excludedStudents = studentList?.Take(2).ToList();
            studentList?.Clear();

            List<Student> eligibleStudents = _students.Values
                .Where(s => excludedStudents == null || !excludedStudents.Contains(s) && s.GetState() != State.Standing)
                .ToList();

            int randomStudentIndex = UnityEngine.Random.Range(0, eligibleStudents.Count);
            Student student = eligibleStudents[randomStudentIndex];

            randomStudentIndex = randomStudentIndex != eligibleStudents.Count - 1 &&
                                 ((randomStudentIndex + 1) % GameManager.Instance.GetCurrentSettings().Columns != 0)
                                 ? randomStudentIndex + 1
                                 : randomStudentIndex - 1;

            Student secStudent = eligibleStudents[randomStudentIndex];

            List<Student> restStudents = eligibleStudents
                .Where(s => s != student && s != secStudent)
                .ToList();

            var problem = restStudents[UnityEngine.Random.Range(0, restStudents.Count)];

            studentList = new List<Student> { problem, student, secStudent };
            return studentList;
        }

        // Reproduce una oración aleatoria con un estudiante
        public void PlaySentence(string text)
        {
            if(_raisedHandStudents.Count == 0)
            {
                int randomStudentIndex = UnityEngine.Random.Range(0, _students.Count);
                _students.ElementAt(randomStudentIndex).Value.GenerateText(text);
            }
            else
            {
                Student st = _raisedHandStudents[0];
                st.GenerateText(text);
                st.HandDown();
                _raisedHandStudents.Remove(st);
            }
        }

        // Reproduce una oración con todos los estudiantes
        public void PlayAllSentence(string text)
        {
            for (int i = 0; i < _students.Count - 1; i++)
            {
                _students.ElementAt(i).Value.GenerateText(text);
            }
        }

        private void Start()
        {
            //Invoke(nameof(doso), 2);
            _raisedHandStudents = new List<Student>();  
        }

        public void OpenBooks()
        {
            foreach(KeyValuePair<string, Student> st in _students)
            {
                if(st.Value.GetComponent<StudentBehavior>().AttentionLevel > 25.0f)
                {
                    // este false indica que se quiere abrir el libro y no la libreta
                    // refactorización tremendamente necesaria para decidir qué queremos abrir?
                    // por supuesto.
                    st.Value.Open(false);
                }
            }
        }

        // Parejas de estudiantes que están charlando.
        // El estudiante Key está girado y hablando con el estudiando Value que
        // tiene físicamente detrás
        Dictionary<Student, Student> _turningPairs = new Dictionary<Student, Student>();

        public void HandleStudentBackFromTurning(Student student)
        {
            if (_turningPairs.ContainsKey(student))
            {
                _turningPairs[student].UnlockActions();
                _turningPairs.Remove(student);
            }
            else return;
        }

        public void HandleStudentTurning(Student student)
        {
            if (_turningPairs.ContainsKey(student)) return;
            if (student.GetState() != State.Sitting) return;
            ClassSettings settings = ClassManager.Instance.GetSettings();
            List<Student> st = _students.Values.ToList();
            int index = st.IndexOf(student);
            if (index > (_students.Count - 1) - settings.Columns) 
            {
                student.StartCoroutine(student.PerformActionCooldown());
                return; 
            }
            Student targetStudent = st[index + settings.Columns];
            if(targetStudent.GetState() != State.Sitting) { return; }

            student.Turn(targetStudent);
            targetStudent.LockOnOtherStudentTurn(student);
            _turningPairs.Add(student, targetStudent);
        }
    }
}
