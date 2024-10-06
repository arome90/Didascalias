using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que representa a un estudiante en la simulación de aula.
    /// </summary>
    [System.Serializable]
    public class Student : MonoBehaviour
    {
        // Variables privadas para referencias de componentes y estado del estudiante
        private FieldOfVision _vision;
        [SerializeField] private FieldOfVision _distracted;
        private FieldOfVision[] _distractedArray;
        [SerializeField] private State _state;
        [SerializeField] private Gender _gender;
        [SerializeField] private bool _problematic = false;
        [SerializeField] private TextMeshProUGUI _studentNameText;
        [SerializeField] private TextMeshProUGUI _attentionText;
        private Desk _desk;
        private Animator _animator;
        private AudioSource _audioSource;
        private NavMeshAgent _navMeshAgent;
        private new BoxCollider _collider;
        [SerializeField] private Transform _target;
        private Vector3 _actualTargetPosition;
        private Dictionary<FieldOfVision, Vector3> _targets;
        [SerializeField] private MultiAimConstraint _headConstraint;
        private StudentBehavior _behavior;
        private Transform _player;
        private ResponseStudent _response;
        private JawMove _jaw;
        private RigBuilder _rig;

        #region Getters

        public Desk GetDesk() => _desk;
        public Gender GetGender() => _gender;
        public bool IsProblematicStudent() => _problematic;
        public AudioSource GetAudioSource() => _audioSource;
        public NavMeshAgent GetNavMeshAgent() => _navMeshAgent;
        public StudentBehavior GetBehavior() => _behavior;
        public State GetState() => _state;

        #endregion

        private void Awake()
        {
            // Inicializa referencias y componentes
            _response = GetComponent<ResponseStudent>();
            _rig = GetComponent<RigBuilder>();
            _collider = GetComponent<BoxCollider>();
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _behavior = GetComponent<StudentBehavior>();
            _jaw = GetComponent<JawMove>();
            _state = State.Sitting;
            _distractedArray = System.Enum.GetValues(typeof(FieldOfVision)).Cast<FieldOfVision>()
                .Where(c => (_distracted & c) == c)
                .ToArray();
            var stateAnim = _animator.GetCurrentAnimatorStateInfo(0);
            _animator.Play(stateAnim.fullPathHash, 0, Random.Range(0f, 1f));
        }

        /// <summary>
        /// Establece los parámetros del estudiante.
        /// </summary>
        /// <param name="player">Transform del jugador.</param>
        /// <param name="name">Nombre del estudiante.</param>
        /// <param name="gender">Género del estudiante.</param>
        public void SetParameters(Transform player, string name, Gender gender)
        {
            _player = player;
            transform.name = name;
            _studentNameText.text = name;
            _gender = gender;
        }

        /// <summary>
        /// Marca al estudiante como problemático.
        /// </summary>
        public void SetProblematicStudent()
        {
            _studentNameText.color = Color.red;
            _problematic = true;
        }

        /// <summary>
        /// Establece el escritorio del estudiante.
        /// </summary>
        /// <param name="d">Escritorio al que se asigna el estudiante.</param>
        public void SetDesk(Desk d)
        {
            _desk = d;
        }

        /// <summary>
        /// Establece los objetivos de visión del estudiante.
        /// </summary>
        /// <param name="transforms">Transformaciones de los objetivos.</param>
        public void SetTargets(Transform[] transforms)
        {
            _targets = new Dictionary<FieldOfVision, Vector3>
            {
                { FieldOfVision.Up, transform.up * 2f },
                { FieldOfVision.Right, transform.right },
                { FieldOfVision.Down, transform.up / -2 },
                { FieldOfVision.Left, -transform.right },
                { FieldOfVision.Window, transforms[0].position },
                { FieldOfVision.Door, transforms[1].position },
                { FieldOfVision.Teacher, Vector3.zero }
            };
        }

        /// <summary>
        /// Indica que el estudiante está prestando atención.
        /// </summary>
        public void PayAttention()
        {
            _behavior.SetAttention();
            SetDirection(FieldOfVision.Teacher);
        }

        /// <summary>
        /// Hace que el estudiante se distraiga.
        /// </summary>
        public void GetDistracted()
        {
            Debug.Log(_distractedArray.Length);
            SetDirection(_distractedArray[Random.Range(0, _distractedArray.Length)]);
        }

        private void Update()
        {
            if (GameManager.Instance.IsPause) return;

            UpdateTargetPosition();
            if (_attentionText != null)
                _attentionText.text = _behavior.AttentionLevel.ToString("0.##");
        }

        // Variables privadas para la animación de movimiento
        private float _smoothTime = 0.15f;
        private float _maxSpeed = 2f;
        private Vector3 _currentVelocity;

        private void UpdateTargetPosition()
        {
            OrientStudentNameTowardsPlayer();
            if (_vision == FieldOfVision.Teacher)
            {
                MoveTargetTo(_player.position, 5.0f);
            }
            else if (_state == State.Sitting)
            {
                MoveTargetTo(_actualTargetPosition, _maxSpeed);
            }
        }

        private void MoveTargetTo(Vector3 destination, float speed)
        {
            _target.position = _vision == FieldOfVision.Teacher ?
                Vector3.MoveTowards(_target.position, destination, speed * Time.deltaTime) :
                Vector3.SmoothDamp(_target.position, destination, ref _currentVelocity, _smoothTime, speed, Time.deltaTime);
        }

        private void OrientStudentNameTowardsPlayer()
        {
            Transform nameTransform = _studentNameText.transform.parent;
            nameTransform.LookAt(_player);
            nameTransform.rotation = Quaternion.LookRotation(_player.forward);
        }

        /// <summary>
        /// Establece la dirección de la atención del estudiante.
        /// </summary>
        /// <param name="fieldOfVision">Campo de visión al que se dirige la atención.</param>
        private void SetDirection(FieldOfVision fieldOfVision)
        {
            _vision = fieldOfVision;

            if (_vision == FieldOfVision.Teacher) return;

            if (fieldOfVision == FieldOfVision.Door || fieldOfVision == FieldOfVision.Window)
            {
                _actualTargetPosition = _targets[fieldOfVision];
                return;
            }
            _actualTargetPosition = transform.position + _targets[fieldOfVision] + transform.forward;
        }

        /// <summary>
        /// Reproduce una animación dada.
        /// </summary>
        /// <param name="stateName">Nombre del estado de la animación.</param>
        public void PlayAnimation(string stateName)
        {
            _animator.Play(stateName);
        }

        /// <summary>
        /// Reproduce una acción disruptiva junto con una animación y un clip de audio.
        /// </summary>
        /// <param name="stateName">Nombre del estado de la animación.</param>
        /// <param name="clip">Clip de audio a reproducir.</param>
        public void PlayDisruptiveAction(string stateName, AudioClip clip)
        {
            _animator.Play(stateName);
            _audioSource.clip = clip;
            _audioSource.Play();
            MoveJaw();
        }

        /// <summary>
        /// Marca al estudiante como no problemático.
        /// </summary>
        public void SetNotProblematicStudent()
        {
            _problematic = false;
            _studentNameText.color = Color.white;
            if (_state == State.Standing)
                SitBack();
        }

        /// <summary>
        /// Cambia el color del nombre del estudiante.
        /// </summary>
        /// <param name="color">Nuevo color.</param>
        public void SetColor(Color color)
        {
            _studentNameText.color = color;
        }

        public void MoveJaw()
        {
            StartCoroutine(_jaw.OnCompleteSpeach());
        }

        /// <summary>
        /// Verifica si el estudiante está dentro del campo de visión de la cámara.
        /// </summary>
        /// <returns>Devuelve verdadero si el estudiante está dentro del campo de visión.</returns>
        public bool IsStudentInFieldOfVision()
        {


            Plane[] cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            var bounds = _collider.bounds;
            bounds.center += new Vector3(0, 1f, 0);
            return GeometryUtility.TestPlanesAABB(cameraFrustum, bounds);
        }

        #region Movement

        // Variables privadas para el movimiento
        private Material _material;
        private Shader _shader;

        /// <summary>
        /// Coroutine para completar el movimiento hacia un destino.
        /// </summary>
        /// <param name="destination">Destino al que se mueve el estudiante.</param>
        /// <param name="breakDistance">Distancia mínima para considerar que el movimiento ha terminado.</param>
        /// <param name="onComplete">Acción a ejecutar al completar el movimiento.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator OnCompleteMove(Vector3 destination, float breakDistance, System.Action onComplete = null)
        {
            _studentNameText.transform.parent.localPosition = new Vector3(0, 1.6f, 0);
            while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Movement Blend Tree"))
                yield return null;

            _state = State.Standing;
            _navMeshAgent.SetDestination(destination);
            _animator.SetFloat("Speed", Mathf.Clamp01(_navMeshAgent.velocity.magnitude));

            while (Distance(transform.position, destination, breakDistance))
                yield return null;

            _rig.layers[0].active = true;
            _navMeshAgent.enabled = false;
            _animator.Play("Idle");
            onComplete?.Invoke();
        }

        private bool Distance(Vector3 position, Vector3 destination, float breakDistance)
        {
            Vector2 projectedPoint1 = new Vector2(position.x, position.z);
            Vector2 projectedPoint2 = new Vector2(destination.x, destination.z);
            return Vector2.Distance(projectedPoint1, projectedPoint2) > breakDistance;
        }

        /// <summary>
        /// Coroutine para completar la acción de sentarse en el escritorio.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator OnCompleteSitBack()
        {
            while (Distance(transform.position, _desk.GetStudentPosition(), 0.07f))
            {
                yield return null;
            }

            _navMeshAgent.enabled = false;
            transform.rotation = _desk.transform.rotation;
            _animator.SetBool("OnFoot", false);
            _desk.PlayAnimacionMesa(Animaciones.SitRelajado);
            _studentNameText.transform.parent.localPosition = new Vector3(0, 1.3f, 0);
            Transform pos = _desk.transform.GetChild(0);
            transform.SetPositionAndRotation(pos.position, pos.parent.rotation);
            transform.Translate(-new Vector3(0f, 0f, 0.15f), Space.Self);
            _state = State.Sitting;
            _rig.layers[0].active = true;
            _desk.SetChairActive(true);
        }

        /// <summary>
        /// Hace que el estudiante se siente de vuelta en su escritorio.
        /// </summary>
        public void SitBack()
        {
            _navMeshAgent.enabled = true;
            _rig.layers[0].active = false;
            _navMeshAgent.SetDestination(_desk.GetStudentPosition());
            _animator.Play("Walking");
            StartCoroutine(OnCompleteSitBack());
        }

        /// <summary>
        /// Mueve al estudiante a un destino específico.
        /// </summary>
        /// <param name="destination">Destino al que se mueve el estudiante.</param>
        /// <param name="breakDistance">Distancia mínima para considerar que el movimiento ha terminado.</param>
        /// <param name="onComplete">Acción a ejecutar al completar el movimiento.</param>
        public void MoveTo(Vector3 destination, float breakDistance, System.Action onComplete = null)
        {
            _navMeshAgent.enabled = true;
            _rig.layers[0].active = false;

            if (_state == State.Sitting)
            {
                _desk.SetChairActive(false);
                _animator.SetBool("OnFoot", true);
                _desk.PlayAnimacionMesa(Animaciones.Empujar);
            }
            else
            {
                _animator.Play("Walking");
            }

            StartCoroutine(OnCompleteMove(destination, breakDistance, onComplete));
        }

        /// <summary>
        /// Cambia el escritorio del estudiante.
        /// </summary>
        /// <param name="d">Nuevo escritorio.</param>
        /// <returns>Coroutine.</returns>
        public IEnumerator ChangeDesk(Desk d)
        {
            if (_state == State.Standing)
            {
                yield return new WaitForSeconds(2f);
                _desk = d;
                SitBack();
            }
            else
            {
                _desk.SetChairActive(false);
                _animator.SetBool("OnFoot", true);
                _desk.PlayAnimacionMesa(Animaciones.Empujar);
                _desk = d;
                StartCoroutine(OnCompleteStandChange());
            }
        }

        /// <summary>
        /// Coroutine para completar el cambio de posición al estar de pie.
        /// </summary>
        /// <returns>Coroutine.</returns>
        private IEnumerator OnCompleteStandChange()
        {
            _studentNameText.transform.parent.localPosition = new Vector3(0, 1.6f, 0);
            while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                yield return null;

            SitBack();
        }

        #endregion

        #region Behavior

        /// <summary>
        /// Genera texto hablado por el estudiante.
        /// </summary>
        /// <param name="text">Texto a hablar.</param>
        public void GenerateText(string text)
        {
            _response.SpeakText(text);
        }

        #endregion

        private void Start()
        {
            Invoke(nameof(RandomPose), 2f);
        }

        private void RandomPose()
        {
            float randomTime = Random.Range(6f, 8f);
            if (_changeAnimationCoroutine != null)
            {
                StopCoroutine(_changeAnimationCoroutine);
            }
            _changeAnimationCoroutine = StartCoroutine(ChangeBlendParameter());
            Invoke("RandomPose", randomTime);
        }

        // Variables privadas para el cambio de blend tree
        private int _blendChangeSpeed = 2;
        private Coroutine _changeAnimationCoroutine;

        private IEnumerator ChangeBlendParameter()
        {
            float targetBlendValue = Random.Range(0, 2);
            float currentBlendValue = _animator.GetFloat("Aburrimiento");

            while (!Mathf.Approximately(currentBlendValue, targetBlendValue))
            {
                // Cambiar gradualmente el valor del parámetro del blend tree
                currentBlendValue = Mathf.MoveTowards(currentBlendValue, targetBlendValue, 0.5f * Time.deltaTime);
                _animator.SetFloat("Aburrimiento", currentBlendValue);

                yield return null;
            }
        }
    }
}
