using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.AI;
using System.Linq;
using TMPro;
using Unity.Tutorials.Core.Editor;
using Unity.VisualScripting;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que representa a un estudiante en la simulación de aula.
    /// </summary>
    [System.Serializable]
    public class Student : MonoBehaviour
    {
        public enum Actions
        {
            // Distraído
            BALANCEARSE,
            MOVIL,
            LANZAR_OBJETO,
            GIRARSE,
            // Atento
            ABRIR,
            ESCRIBIR,
            TABLET,
            COGER_OBJETO,
            LEVANTAR_MANO,
        }

        private enum AnimationState
        {
            WRITING,
            HAND_RAISED,
            TURNED_AROUND,


            NONE
        }

        private AnimationState _animPlaying = AnimationState.NONE;

        // Variables privadas para referencias de componentes y estado del estudiante
        private FieldOfVision _vision;
        [SerializeField] private FieldOfVision _distracted;
        private FieldOfVision[] _distractedArray;
        [SerializeField] private State _state;
        [SerializeField] private Gender _gender;
        [SerializeField] private bool _problematic = false;
        [SerializeField] private TextMeshProUGUI _studentNameText;
        [SerializeField] private TextMeshProUGUI _attentionText;

        [SerializeField] private AudioClip[] _smallConversationClips;
        [SerializeField] private AudioClip[] _maleLaughterClips;
        [SerializeField] private AudioClip[] _femaleLaughterClips;

        private Desk _desk;
        private Animator _animator;
        private AudioSource _audioSource;
        private AudioSource _loopSource = null;
        private AudioSource _oneShootSource = null;
        private NavMeshAgent _navMeshAgent;
        private BoxCollider _collider;

        [SerializeField] private Transform _target;
        private Vector3 _actualTargetPosition;
        private Dictionary<FieldOfVision, Vector3> _targets;
        
        [SerializeField] private MultiAimConstraint _headConstraint;
        
        private StudentBehavior _behaviour;
        private StudentsController _controller;

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
        public StudentBehavior GetBehavior() => _behaviour;
        public StudentsController GetController() => _controller;
        public State GetState() => _state;

        #endregion

        #region Setters

        public void SetController(StudentsController controller) => _controller = controller;

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
            _behaviour = GetComponent<StudentBehavior>();
            _jaw = GetComponent<JawMove>();
            _state = State.Sitting;
            _distractedArray = System.Enum.GetValues(typeof(FieldOfVision)).Cast<FieldOfVision>()
                .Where(c => (_distracted & c) == c)
                .ToArray();
            var stateAnim = _animator.GetCurrentAnimatorStateInfo(0);
            _animator.Play(stateAnim.fullPathHash, 0, Random.Range(0f, 1f));

            _oneShootSource = gameObject.AddComponent<AudioSource>();
            _oneShootSource.loop = false;
            _oneShootSource.clip = null;
            _oneShootSource.volume = .4f;
            _oneShootSource.spatialBlend = 1.0f;

            _loopSource = gameObject.AddComponent<AudioSource>();
            _loopSource.loop = true;
            _loopSource.clip = null;
            _loopSource.volume = .4f;
            _loopSource.spatialBlend = 1.0f;

            _audioSource.spatialBlend = 1.0f;
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
                { FieldOfVision.Teacher, Vector3.zero },
                { FieldOfVision.Front, transform.forward }
            };
        }

        /// <summary>
        /// Indica que el estudiante está prestando atención.
        /// </summary>
        public void PayAttention()
        {
            _behaviour.SetAttention();
            SetDirection(FieldOfVision.Teacher);
        }

        private void Update()
        {
            if (GameManager.Instance.IsPause) return;

            UpdateTargetPosition();
            if (_attentionText != null)
                _attentionText.text = _behaviour.AttentionLevel.ToString("0.##");
            PerformAction();
        }

        // Variables privadas para comenzar animaciones cuando no se está atendiendo a clase.
        private bool _outOfActionCooldown = true;
        private bool _blockActions = false;
        private bool _performingBadAction = false; // Variable usada para acciones que tienen
                                                   // varias partes o continúan en el tiempo
                                                   // Como "GIRARSE"
        private float _attetionThresholdDistracted = 25f; // Nivel de atención a partir del cual comenzarán acciones
        private float _stopAttentionActionThreshold = 60.0f;
        private float _atenttionActionThreshold = 65.0f;
        // Los valores que NO son de Debug son: 7.5f y 15f
        private float _minCDAction = 7.5f; // Mínimo tiempo entre acciones distraídas
        private float _maxCDAction = 15f; // Máximo tiempo entre acciones distraídas
        private float _actionCD; // Valor aletorio entre mínimo y máximo de cooldown

        public IEnumerator PerformActionCooldown()
        {
            _outOfActionCooldown = false;
            _actionCD = Random.Range(_minCDAction, _maxCDAction);
            yield return new WaitForSeconds(_actionCD);
            _outOfActionCooldown = true;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        void EndGoodAction()
        {
            Desk.MATERIAL_STATE mState = _desk.getState();
            switch (_animPlaying)
            {
                case AnimationState.HAND_RAISED:
                    HandDown();
                    break;
                default: break;
            }

            switch (mState)
            {
                case Desk.MATERIAL_STATE.NOTEBOOK_OPENED:
                    Close(true);
                    break;
                case Desk.MATERIAL_STATE.BOOK_AND_NOTEBOOK_OPENED:
                    Close(true);
                    break;
                default: break;
            }
        }

        void PerformBadAction()
        {
            // Si la clase no es en fila, no se tiene en cuenta la acción "GIRARSE"
            ClassSettings settings = ClassManager.Instance.GetSettings();
            int distractedAction = Random.Range((int)Actions.BALANCEARSE,
                // Si estamos o no en modo fila, se añade la opción de "GIRARSE". Si no, no se añade.
                settings.StructureMode == StructureMode.Fila ?
                (int)Actions.GIRARSE + 1 : (int)Actions.LANZAR_OBJETO + 1);
            if (distractedAction == (int)Actions.MOVIL)
            {
                SetDirection(FieldOfVision.Down);
                // Ponemos un cooldown a la acción realizada antes de realizar cualquier otra
                StartCoroutine(PerformActionCooldown());
                PlayActionAnimation(distractedAction);
            }
            else if (distractedAction == (int)Actions.BALANCEARSE)
            {
                _desk.PlayChairAnimation();
                // Ponemos un cooldown a la acción realizada antes de realizar cualquier otra
                StartCoroutine(PerformActionCooldown());
                PlayActionAnimation(distractedAction);
            }
            else if (distractedAction == (int)Actions.GIRARSE)
            {
                // Aquí vamos a poner que se gire y "bloquee" la acción del estudiante que tenga detrás
                _performingBadAction = true;
                _controller.HandleStudentTurning(this);
            }
        }

        void PerformGoodAction()
        { // Hacer cosas guays, como escribir o así, como si estuvieran tomando apuntes.
          // int attentionAction = Random.Range((int)Actions.ABRIR, (int)Actions.LEVANTAR_MANO + 1);
            if (_animPlaying != AnimationState.NONE) return;
            int attentionAction = Random.Range(0, 101);
            if (attentionAction < 3) // un 3% de las veces, hacemos que el/la alumn@ levante la mano
            {
                RaiseHand();
            }
            else if (!_desk.IsNotebookOpened() && attentionAction < 50)
            {
                Open(true);
            }
            else if (_desk.IsNotebookOpened()
                && attentionAction < 70) // hacemos que el/la alumn@ escriba durante un rato
            {
                StartWriting();
            }
            // No se hace el Cooldown por defecto porque 
            // las demás acciones de esta lista llevan su propio Cooldown
            // cuando deban acabar
            else StartCoroutine(PerformActionCooldown());
        }

        void ChangeToBadExpression()
        {
            int distractedExpression = Random.Range(0, 3);
            Expresiones expression;
            switch (distractedExpression)
            {
                case 0:
                    expression = Expresiones.Enfadado;
                    break;
                case 1:
                    expression = Expresiones.Quejarse;
                    break;
                case 2:
                    expression = Expresiones.Dormido;
                    break;
                default:
                    expression = Expresiones.Dormido;
                    break;
            }
            StartCoroutine(_behaviour.ChangeExpression(expression));
        }

        private bool CanDoBadAction()
        {
            return _outOfActionCooldown && _behaviour.AttentionLevel <= _attetionThresholdDistracted;
        }

        private bool ShouldDismissGoodAction()
        {
            return _behaviour.AttentionLevel < _stopAttentionActionThreshold;
        }

        /// <summary>
        /// Llamado en el Update. Comprueba si un estudiante puede hacer una acción según su nivel de atención
        /// Si su atención es baja, hará una acción distraída, como cambiar su expresión, ver el móvil o balancearse.
        /// Si su atención es alta, hará una acción atenta, como escribir en su libreta.
        /// </summary>
        private void PerformAction()
        {
            if (_blockActions) 
                return;

            // Si nuestro nivel de atención ha disminuído, pararemos
            // aquellas acciones que tengan que ver con un alto nivel de atención
            // como escribir, tener la libreta abierta, la mano levantada, etc.
            if (ShouldDismissGoodAction())
                EndGoodAction();

            if (CanDoBadAction())
            {
                if(_behaviour.AttentionLevel < 35.0f) // acción
                    PerformBadAction();
                else // Expresión
                    ChangeToBadExpression();
            }
            else if (_performingBadAction && (_behaviour.AttentionLevel > 40.0f))
            {
                switch(_animPlaying)
                {
                    case AnimationState.TURNED_AROUND:
                        BackFromTurning();
                        _controller.HandleStudentBackFromTurning(this);
                        break;
                    default: break;
                }
                _performingBadAction = false;
                // Ponemos un cooldown a la acción realizada antes de realizar cualquier otra
                StartCoroutine(PerformActionCooldown());
            }
            // AQUÍ HAY QUE HACER QUE SE VUELVE A SU POSICIÓN CUANDO ESTÁ GIRADO. PROBABLEMENTE CON OTRO
            // THRESHOLD.
            // HABRÍA QUE REFACTORIZAR TODA LA MOVIDIÑA

            else if (_outOfActionCooldown && _behaviour.AttentionLevel > _atenttionActionThreshold)
            {
                PerformGoodAction();
            }
            _outOfActionCooldown = _outOfActionCooldown && _animPlaying == AnimationState.NONE;
        }

        #region Behaviour
        /// <summary>
        /// Genera texto hablado por el estudiante.
        /// </summary>
        /// <param name="text">Texto a hablar.</param>
        public void GenerateText(string text)
        {
            _response.SpeakText(text);
        }

        #region Turn
        /// <summary>
        /// Girarse para hablar con el compañero de detrás
        /// </summary>
        public void Turn(Student other)
        {
            _animator.SetBool("Turned", true);
            _animPlaying = AnimationState.TURNED_AROUND;
            PlayActionAnimation(Actions.GIRARSE);
            StartCoroutine(PlayConversationSounds(other));
        }

        IEnumerator PlayConversationSounds(Student other)
        {
            yield return new WaitForSeconds(1.0f);

            _loopSource.clip = _smallConversationClips[Random.Range(0, _smallConversationClips.Length)];
            _loopSource.loop = true;
            _loopSource.Play();

            while(_animPlaying == AnimationState.TURNED_AROUND)
            {
                yield return new WaitForSeconds(Random.Range(5.0f, 20.0f));
                int id = Laugh();
                if (other.GetGender() == _gender) other.Laugh(id);
                else other.Laugh();
            }

            _loopSource.Stop();
            _loopSource.clip = null;
        }

        public int Laugh(int idNotAvaliable = -1)
        {
            AudioClip[] clips;

            if (_gender == Gender.Men) { clips = _maleLaughterClips; }
            else { clips = _femaleLaughterClips; }

            int length = clips.Length;

            int rand;
            if (idNotAvaliable == -1) rand = Random.Range(0, _maleLaughterClips.Length);
            else rand = Random.Range(idNotAvaliable, idNotAvaliable + length) % length;

            _oneShootSource.clip = clips[rand];
            _oneShootSource.loop = false;
            _oneShootSource.Play();

            return rand;
        } 

        IEnumerator StopTurnAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            BackFromTurning();
        }

        public void LockOnOtherStudentTurn(Student student)
        {
            if(_behaviour.AttentionLevel > 40f)
            {
                // no le atiende y se enfada
                StartCoroutine(_behaviour.ChangeExpression(Expresiones.Enfadado));
                StartCoroutine(student.StopTurnAfterSeconds(2.0f));
            }
            else
            {
                // le atiende
                SetDirection(FieldOfVision.Front);
                LockActions();
            }
        }

        /// <summary>
        /// Volver a la posición de sentado original
        /// </summary>
        public void BackFromTurning()
        {
            _animPlaying = AnimationState.NONE;
            _animator.SetBool("Turned", false);
        }
        #endregion

        #region Raise Hand

        /// <summary>
        /// Se levanta la mano
        /// </summary>
        public void RaiseHand()
        {
            _controller.AddHandRaisedStudent(this);
            _animPlaying = AnimationState.HAND_RAISED;
            PlayActionAnimation(Actions.LEVANTAR_MANO);
            _animator.SetBool("HandRaised", true);
        }

        /// <summary>
        /// Se baja la mano
        /// </summary>
        public void HandDown()
        {
            _controller.RemoveHandRaisedStudent(this);
            _animPlaying = AnimationState.NONE;
            _animator.SetBool("HandRaised", false);
            StartCoroutine(PerformActionCooldown());
        }

        public void HandleCallOnRaisedHand()
        {
            if (_animPlaying != AnimationState.HAND_RAISED) return;
            Didascalia_LocalizationManager.Instance.GetTranslation("handRaisedDoubt", Didascalia_LocalizationManager.TableCollections.AUDIO, out string traduction);
            GenerateText(traduction);

            HandDown();
        }
        #endregion

        #region OpenBook

        bool isNotebook = false;
        bool wantToOpen = true;

        /// <summary>
        /// Método que inicia la animación de abrir.
        /// Para configurar qué se quiere abrir, usamos un parámetro en "Open".
        /// "isNotebook" a true o false dependiendo de si abriremos una
        /// libreta o un libro.
        /// </summary>
        public void Open(bool _isNotebook)
        {
            isNotebook = _isNotebook;
            wantToOpen = true;
            PlayActionAnimation(Actions.ABRIR);
            StartCoroutine(PerformActionCooldown());
        }
        /// <summary>
        /// Lo mismo que Open(bool), pero para cerrar.
        /// </summary>
        public void Close(bool _isNotebook)
        {
            isNotebook = _isNotebook;
            wantToOpen = false;
            PlayActionAnimation(Actions.ABRIR);
        }

        /// <summary>
        /// Abrir o cerrar un libro o libreta.
        /// Se llama desde un evento de animación 
        /// al comenzar las animaciones de 
        /// CerrarLibro y AbrirLibro del Estudiante
        /// </summary>
        public void CloseOrOpenBook()
        {
            if(wantToOpen)
            {
                if (isNotebook) _desk.OpenNoteBook();
                else _desk.OpenBook();
            }
            else
            {
                if (isNotebook) _desk.CloseNoteBook();
                else _desk.CloseBook();
            }
            
            StartCoroutine(PerformActionCooldown());
        }

        public void CloseBook()
        {
            PlayActionAnimation(Actions.ABRIR);
            _desk.CloseBook();
            StartCoroutine(PerformActionCooldown());
        }

        public void CloseNoteBook()
        {
            PlayActionAnimation(Actions.ABRIR);
            _desk.CloseNoteBook();
            StartCoroutine(PerformActionCooldown());
        }
        #endregion

        #region Write
        private void StartWriting()
        {
            _animPlaying = AnimationState.WRITING;
            _animator.SetBool("Writing", true);
            PlayActionAnimation(Actions.ESCRIBIR);
            StartCoroutine(WriteForSeconds(UnityEngine.Random.Range(3.0f, 7.5f)));
        }

        IEnumerator WriteForSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            EndWriting();
        }

        private void EndWriting()
        {
            _animPlaying = AnimationState.NONE;
            _animator.SetBool("Writing", false);
            StartCoroutine(PerformActionCooldown());
        }
        #endregion
        #endregion

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

        private void PlayActionAnimation(int action)
        {
            _animator.SetInteger("Accion", action);
        }

        private void PlayActionAnimation(Actions action)
        {
            PlayActionAnimation((int)action);
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
            while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
                yield return null;

            _state = State.Standing;
            _navMeshAgent.SetDestination(destination);

            while (Distance(transform.position, destination, breakDistance))
                yield return null;

            _rig.layers[0].active = true;
            _navMeshAgent.enabled = false;
            _animator.Play("Standing");
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
            _animator.SetBool("onFoot", false);
            _desk.PlayDeskAnimation(Animaciones.SitRelajado);
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
                _animator.SetBool("onFoot", true);
                _desk.PlayDeskAnimation(Animaciones.Empujar);
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
                _animator.SetBool("onFoot", true);
                _desk.PlayDeskAnimation(Animaciones.Empujar);
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

        public void LockActions()
        {
            _blockActions = true;
        }

        public void UnlockActions()
        {
            _blockActions = false;
        }
    }
}
