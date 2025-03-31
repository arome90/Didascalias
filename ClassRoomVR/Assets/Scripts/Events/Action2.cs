using BehaviorDesigner.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Controla las acciones disruptivas utilizando un árbol de comportamiento.
    /// </summary>
    public class Action2 : MonoBehaviour
    {
        [SerializeField] private float _distanceNear = 5.0f; // Distancia considerada cercana
        [SerializeField] private AudioClip _classLaughterClip; // Clip de audio de risas de clase
        [SerializeField] private AudioClip _noiseClip; // Clip de audio de ruido

        private StudentsController2 _studentsController; // Controlador de estudiantes
        private GameObject _player; // Jugador
        private BehaviorTree _behaviorTree; // Árbol de comportamiento
        private List<Student2> _problematicStudents; // Lista de estudiantes problemáticos
        private DisruptiveAction _disruptiveAction; // Acción disruptiva específica
        private TextMeshProUGUI _textMeshPro; // UI para mostrar texto

        private const float MaxOutOfVisionTime = 20.0f; // Tiempo máximo fuera de la visión

        /// <summary>
        /// Inicializa los controladores y ajusta el modo de conversación.
        /// </summary>
        private void Start()
        {
            _studentsController = ClassManager.Instance.GetStudentsController();
            _studentsController.SetMode(TalkMode2.None);
        }

        /// <summary>
        /// Establece los parámetros iniciales del comportamiento disruptivo.
        /// </summary>
        /// <param name="player">El jugador (profesor).</param>
        /// <param name="students">Lista de estudiantes problemáticos.</param>
        /// <param name="action">Acción disruptiva a manejar.</param>
        /// <param name="text">Texto que se mostrará en pantalla.</param>
        public void SetParameters(GameObject player, List<Student2> students, DisruptiveAction action, TextMeshProUGUI text)
        {
            _player = player;
            _problematicStudents = students;
            _disruptiveAction = action;
            _textMeshPro = text;
            _behaviorTree = GetComponent<BehaviorTree>();
            InputLogger.Instance.NewAction();
            _behaviorTree.EnableBehavior();

            if (_disruptiveAction.Laughter)
            {
                Invoke(nameof(PlayLaughter), 2.0f);
            }

            if (_textMeshPro != null)
            {
                _textMeshPro.text = "-1";
            }
        }

        /// <summary>
        /// Ignora al estudiante y comienza la verificación si el profesor deja de observar.
        /// </summary>
        public void Ignore()
        {
            Invoke(nameof(HandleIgnoreTime), _disruptiveAction.ReactionTime);
            _problematicStudents.ForEach(student => StartCoroutine(IgnoreStudent(student)));
        }

        /// <summary>
        /// Invocado si no se seleccionó otro camino antes de que termine el tiempo de reacción.
        /// </summary>
        private void HandleIgnoreTime()
        {
            if ((int)_behaviorTree.GetVariable("Path").GetValue() < 0)
            {
                _behaviorTree.GetVariable("Path").SetValue(3);
                _problematicStudents.ForEach(student => student.SetColor(Color.gray));
                PlayLaughter();
            }
        }

        /// <summary>
        /// Controla el tiempo que el estudiante está fuera de la visión del profesor.
        /// </summary>
        /// <param name="student">El estudiante a verificar.</param>
        /// <returns>Retorna un IEnumerator para la corrutina.</returns>
        private IEnumerator IgnoreStudent(Student2 student)
        {
            float outOfVisionTimer = 0f;

            while ((int)_behaviorTree.GetVariable("Path").GetValue() < 0)
            {
                yield return null;

                if (student.IsStudentInFieldOfVision())
                {
                    outOfVisionTimer = 0f;
                }
                else
                {
                    outOfVisionTimer += Time.deltaTime;

                    if (outOfVisionTimer >= MaxOutOfVisionTime)
                    {
                        _behaviorTree.GetVariable("Path").SetValue(3);
                        _problematicStudents.ForEach(s => s.SetColor(Color.gray));
                        PlayLaughter();
                        yield break;
                    }
                }
            }
        }

        /// <summary>
        /// Método para controlar el camino de acercarse y hablar bien.
        /// </summary>
        public void Approach()
        {
            _problematicStudents.ForEach(student =>
            {
                if (Resolve(student))
                {
                    _behaviorTree.GetVariable("Path").SetValue(1);
                    _problematicStudents.ForEach(s => s.SetColor(Color.green));
                    Debug.Log(_studentsController.GetMode() == TalkMode2.Good ? "Genial" : "Segundo camino");
                }
            });
        }

        /// <summary>
        /// Resuelve la acción disruptiva del estudiante si se cumplen las condiciones.
        /// </summary>
        /// <param name="student">El estudiante a resolver.</param>
        /// <returns>Retorna true si se resolvió, false si no.</returns>
        private bool Resolve(Student2 student)
        {
            return (Vector3.Distance(student.transform.position, _player.transform.position) <= _distanceNear
                    && student.IsStudentInFieldOfVision())
                   || ((_studentsController.Resolutions & _disruptiveAction.Action) == _disruptiveAction.Action
                   && _studentsController.GetMode() != TalkMode2.Disrespect);
        }

        /// <summary>
        /// Método para controlar el camino de gritar o falta de respeto.
        /// </summary>
        public void Shout()
        {
            if (_studentsController.GetMode() == TalkMode2.Disrespect)
            {
                _behaviorTree.GetVariable("Path").SetValue(2);
                var audioSource = _player.GetComponent<AudioSource>();
                audioSource.clip = _noiseClip;
                audioSource.Play();
                _problematicStudents.ForEach(student => student.SetColor(Color.red));
            }
        }

        /// <summary>
        /// Reproduce el clip de risas de la clase.
        /// </summary>
        private void PlayLaughter()
        {
            if (_classLaughterClip != null && _disruptiveAction.Laughter)
            {
                var audioSource = _player.GetComponent<AudioSource>();
                audioSource.Stop();
                audioSource.clip = _classLaughterClip;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Termina el conflicto y resetea los estados de los estudiantes problemáticos.
        /// </summary>
        public void Finish()
        {
            UpdateText();
            _problematicStudents.ForEach(student =>
            {
                Debug.Log(student.name);
                student.SetNotProblematicStudent();
            });

            Debug.Log(_behaviorTree.GetVariable("Path").GetValue());
            _studentsController.SetMode(TalkMode2.None);
            Destroy(gameObject, 2f);
            InputLogger.Instance.CompareVelocity();
        }

        /// <summary>
        /// Actualiza el texto en pantalla con el valor de la ruta.
        /// </summary>
        private void UpdateText()
        {
            if (_textMeshPro != null)
            {
                _textMeshPro.text = _behaviorTree.GetVariable("Path").GetValue().ToString();
            }
        }

        /// <summary>
        /// Método invocado cuando el objeto es destruido.
        /// </summary>
        private void OnDestroy()
        {
            _problematicStudents.ForEach(student => student.SetColor(Color.white));

            if (_textMeshPro != null)
            {
                _textMeshPro.text = string.Empty;
            }

            _disruptiveAction = null;
        }
    }
}
