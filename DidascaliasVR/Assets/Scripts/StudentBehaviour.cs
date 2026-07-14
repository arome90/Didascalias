using Meta.WitAi.TTS.Integrations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum StudentState
{
    Sitting,
    StandingOnDesk,
    StandingRightOutOfDesk,
    StandingOutOfDesk,
    LeavingDesk,
    EnteringDesk,
    Walking,
    Expelling,
    Expelled,
    EnteringClass,
    OpeningDoor,
    ClosingDoor
}

/// <summary>
/// Contiene todos los m�todos que se encargan del comportamiento del estudiante.
/// Todos ellos son llamados en la m�quina de estados del prefab de Student
/// </summary>
public class StudentBehaviour : MonoBehaviour
{
    // Animator _animator;
    Didascalia.Student.StudentAnimatorController _animator;
    internal Didascalia.Student.StudentAnimatorController Animator => _animator;
    NavMeshAgent _agent;
    [System.Obsolete(
        "A TTSWit component is not to be attached each GameObject that wants to speak but\n"
        + "only one shall be contained per scene in a 'configuration global wit tts object'.\n"
        + "Each Student should have attached instead: WitSpeaker. From which we can issue calls to the scene's TTSWit to `.Speak()`"
    )]
    TTSWit _tts;
    Student _st;

    public Transform SitSpot { get { return _st.Desk.StudentPosition; } }

    public void ChangeSitSpotWithStudent(StudentBehaviour other)
    {
        Transform thisParent = SitSpot;
        transform.parent = other.transform.parent;
        other.transform.parent = thisParent;
    }

    private Desk _desk => _st.Desk;

    [SerializeField]
    private StudentState _state;

    [Header("Parameters")]

    [SerializeField, Range(0.0001f, 1.0f), Tooltip("Speed with which the student moves out of the desk")]
    private float _exitDeskSpeed = 0.001f;

    [Header("Events")]
    public UnityEvent OnStandUp = new UnityEvent();
    public UnityEvent OnExitDesk = new UnityEvent();
    public UnityEvent OnEnterDesk = new UnityEvent();
    public UnityEvent OnSitDown = new UnityEvent();
    public UnityEvent OnOpenDoor = new UnityEvent();
    public UnityEvent OnCloseDoor = new UnityEvent();
    public UnityEvent OnExpel = new UnityEvent();
    public UnityEvent OnStandUpRequested = new UnityEvent();
    public UnityEvent OnSitDownRequested = new UnityEvent();
    public UnityEvent OnExpellingRequested = new UnityEvent();
    public UnityEvent OnChangePlacesRequested = new UnityEvent();
    public UnityEvent OnSitTogetherRequested = new UnityEvent();

    [System.Obsolete("Yell is not implemented yet. Remove this attribute when it is implemented")]
    public UnityEvent OnYellRequested = new UnityEvent();

    public UnityEvent OnHyperstimulateRequested = new UnityEvent();
    public UnityEvent OnFrustrateRequested = new UnityEvent();
    public UnityEvent OnGetMaterialOutRequested = new UnityEvent();
    public UnityEvent OnFailToPayAttentionRequested = new UnityEvent();
    public UnityEvent OnGetDistractedRequested = new UnityEvent();

    public StudentState State { get { return _state; } }
    private void Start()
    {
        _animator = GetComponentInChildren<Didascalia.Student.StudentAnimatorController>();
        _agent = GetComponent<NavMeshAgent>();
        _st = GetComponent<Student>();
        _tts = GetComponent<TTSWit>();

        Didascalia.Utils.Error.DebugbreakFailIf(_animator == null, "Animator component not found", this);
        Didascalia.Utils.Error.DebugbreakFailIf(_agent == null, "NavMeshAgent component not found", this);
        Didascalia.Utils.Error.DebugbreakFailIf(_tts == null, "TTS component not found", this);
        Didascalia.Utils.Error.DebugbreakFailIf(_st == null, "Student component not found", this);

        // these events happen on the action's end.
        OnStandUp.AddListener(ChangeStateOnStandUp);
        OnExitDesk.AddListener(ChangeStateOnExitDesk);
        OnEnterDesk.AddListener(ChangeStateOnEnterDesk);
        OnSitDown.AddListener(ChangeStateOnSitDown);
        OnOpenDoor.AddListener(ChangeStateOnDoorInteraction);
        OnCloseDoor.AddListener(ChangeStateOnDoorInteraction);
        OnExpel.AddListener(ChangeStateOnExpel);
        ChangeState(StudentState.Sitting);
    }

    private void OnDestroy()
    {
        OnStandUp.RemoveListener(ChangeStateOnStandUp);
    }

    // revisar
    private void ChangeStateOnStandUp()
    {
        ChangeState(StudentState.StandingOnDesk);
    }

    private void ChangeStateOnExitDesk()
    {
        ChangeState(StudentState.StandingOutOfDesk);
    }

    private void ChangeStateOnEnterDesk()
    {
        ChangeState(StudentState.StandingOnDesk);
    }

    private void ChangeStateOnSitDown()
    {
        ChangeState(StudentState.Sitting);
    }

    private void ChangeStateOnDoorInteraction()
    {
        ChangeState(StudentState.StandingOutOfDesk);
    }

    private void ChangeStateOnExpel()
    {
        ChangeState(StudentState.Expelled);
    }

    public void ChangeState(StudentState newState)
    {
        _state = newState;
    }

    public void SitDownStudent()
    {
        OnSitDownRequested.Invoke();
    }

    public void ExpelStudent()
    {
        OnExpellingRequested.Invoke();
    }

    #region SitDown
    
    public void StartSitDownAnimation()
    {
        UnsetOnFoot();
    }
    #endregion

    #region Walk
    IEnumerator StartWalking_(float time)
    {
        return MovementAnimation_(true, time);
    }

    IEnumerator StopWalking_(float time)
    {
        return MovementAnimation_(false, time);
    }

    IEnumerator MovementAnimation_(bool wantsToWalk, float time)
    {
        int hashFloatSpeed = Didascalia.Student.StudentAnimatorController.HashFloatSpeed;
        float speed = _animator.StudentAnimator.GetFloat(hashFloatSpeed);
        float initialSpeed = speed;

        float goal;
        if (wantsToWalk) goal = 1.0f;
        else goal = 0.0f;

        float elapsedTime = 0.0f;
        bool done = false;
        while(!done)
        {
            elapsedTime += Time.deltaTime;
            speed = Mathf.Lerp(initialSpeed, goal, elapsedTime / time);

            _animator.StudentAnimator.SetFloat(hashFloatSpeed, speed);

            yield return new WaitForEndOfFrame();

            done = speed == goal;
        }
    }

    public void MoveTo(Transform transform, bool restrictive = false)
    {
        if (restrictive) 
        { 
            StopAndClear();
            EnqueueAction(MovingTowardsPoint_(transform.position));
        }
        else StartCoroutine(MovingTowardsPoint_(transform.position)); 
    }

    public void Expel()
    {
        StopAndClear();

        EnqueueAction(MoveToFrontDoorCoroutine_());
        EnqueueAction(OpenDoorInside_());
        EnqueueAction(MovementAnimationAndRotate_(ClassManager.Instance.FrontDoor.OutsideStandingPoint, 0.95f));
        EnqueueAction(CloseDoorOutside_());

        // EnqueueAction(ExpelCoroutine_());

        //StartCoroutine(ExpelCoroutine_());
    }

    IEnumerator OpenDoorInside_()
    {
        ChangeState(StudentState.OpeningDoor);
        _animator.OpenDoorInside(ClassManager.Instance.FrontDoor);

        yield return new WaitUntil(() => _state == StudentState.StandingOutOfDesk);
    }

    IEnumerator CloseDoorOutside_()
    {
        ChangeState(StudentState.ClosingDoor);
        _animator.CloseDoorOutside(ClassManager.Instance.FrontDoor);

        yield return new WaitUntil(() => _state == StudentState.Expelled);
    }

    IEnumerator ExpelCoroutine_()
    {
        yield return MoveToFrontDoorCoroutine_();

        yield return OpenDoorInside_();

        yield return MovementAnimationAndRotate_(ClassManager.Instance.FrontDoor.OutsideStandingPoint, 0.95f);

        yield return CloseDoorOutside_();
    }

    // external
    public void MoveToFrontDoor(bool restrictive = false)
    {
        if (restrictive) StopAndClear();
        EnqueueAction(MoveToFrontDoorCoroutine_());
        //return StartCoroutine(MoveToFrontDoorCoroutine_());
    }

    // internal
    IEnumerator MoveToFrontDoorCoroutine_()
    {
        return MovementAnimationAndRotate_(ClassManager.Instance.FrontDoor.InsideStandingPoint, 0.65f);
    }

    IEnumerator AcquireTargetRotation_(Quaternion rotation, float time)
    {
        Quaternion initialRotation = transform.rotation;

        float elapsedTime = 0.0f;
        var wait = new WaitForEndOfFrame();
        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(initialRotation, rotation, elapsedTime / time);

            yield return wait;
        }

        transform.rotation = rotation;
        yield return wait;
        // Didascalia.Utils.Error.DebugbreakFailUnimplemented("AcquireTargetRotation is not fully implemented, it should be able to be interrupted by other calls to this method or to MoveTo", this);
    }

    // external
    public Coroutine StartAcquireTargetRotation(Quaternion rotation, float time)
    {
        return StartCoroutine(AcquireTargetRotation_(rotation, time));
    }

    IEnumerator MovementAnimationAndRotate_(Transform transform, float rotateTime, UnityAction callback = null)
    {
        yield return MovingTowardsPoint_(transform.position);
        yield return AcquireTargetRotation_(transform.rotation, rotateTime);

        if (callback != null)
            callback.Invoke();
    }

    // external
    public Coroutine MoveToAndRotate(Transform transform, float rotateTime, UnityAction callback = null)
    {
        return StartCoroutine(MovementAnimationAndRotate_(transform, rotateTime, callback));
    }

    IEnumerator MoveToFrontDoorOutsidePoint_()
    {
        ChangeState(StudentState.EnteringClass);
        yield return MovementAnimationAndRotate_(ClassManager.Instance.FrontDoor.OutsideStandingPoint, 0.0f);
    }

    IEnumerator OpenDoorOutside_()
    {
        ChangeState(StudentState.OpeningDoor);
        _animator.OpenDoorOutside(ClassManager.Instance.FrontDoor);

        yield return new WaitUntil(() => _state == StudentState.StandingOutOfDesk);
    }

    IEnumerator CloseDoorInside_()
    {
        ChangeState(StudentState.ClosingDoor);
        _animator.CloseDoorInside(ClassManager.Instance.FrontDoor);

        yield return new WaitUntil(() => _state == StudentState.StandingOutOfDesk);
    }

    IEnumerator EnterClass_()
    {
        if (_state != StudentState.Expelled) yield break;

        yield return MoveToFrontDoorOutsidePoint_();

        yield return OpenDoorOutside_();

        yield return MoveToFrontDoorCoroutine_();

        yield return CloseDoorInside_();
    }

    IEnumerator MovingTowardsPoint_(Vector3 point)
    {
        yield return EnterClass_();

        float speed = _agent.speed;
        _agent.speed = 0.0f;
        // if we're not standing out of desk...
        // then we wait for leaving desk animation to finish
        yield return LeaveDesk_();

        _agent.SetDestination(point);
        _agent.speed = speed;
        ChangeState(StudentState.Walking);

        yield return StartWalking_(0.65f);

        yield return new WaitUntil(() => !_agent.pathPending);
        yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance * 2);

        yield return StopWalking_(0.95f);
        ChangeState(StudentState.StandingOutOfDesk);
    }
    #endregion

    #region Conflicts
    #region StandUp
    internal void SetOnFoot()
    {
        _animator.SetOnFoot();
    }

    internal void UnsetOnFoot()
    {
        _animator.SitDown();
    }

    public void StandUp()
    {
        if(_state == StudentState.Sitting)
        {
            OnStandUpRequested.Invoke();
            if (_animator.StudentAnimator.GetBehaviour<OnStandUp>() == null)
            {
                Didascalia.Utils.Error.DebugbreakFailMessage("OnStandUp behaviour not found in animator", this);
            } 
        }

        // XXX: We should be able to run this safety check to ensure the event invocation causes
        // transition to 'Stand Up' state but it seems that the transition is not registered in the same frame, so we can't check it here.
        // else if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Stand Up"))
        // {
        //     Didascalia.Utils.Error.DebugbreakFailMessage(
        //         "Current animator state is not 'Stand Up'\n"
        //         + "This is a safety check to ensure the dependency that hooks to the OnStandUpRequested event"
        //         + "properly sets up the transition to the 'Stand Up' state",
        //         this
        //     );
        // }
    }

    public void SitDown()
    {
        if (_state == StudentState.Sitting) return;

        StopAndClear();
        EnqueueAction(SitDownCoroutine_());
        // return StartCoroutine(SitDownCoroutine_());
    }

    IEnumerator SitDownCoroutine_()
    {
        yield return EnterDesk_();

        _animator.SitDown();
    }

    IEnumerator EnterDesk_()
    {
        yield return EnterClass_();
        
        if (_state == StudentState.StandingOnDesk || _state == StudentState.Sitting) yield break;

        if (_state != StudentState.StandingRightOutOfDesk)
        {
            yield return MovementAnimationAndRotate_(_st.Desk.OutOfDeskTransform, 0.65f);
            ChangeState(StudentState.StandingRightOutOfDesk);
        }

        ChangeState(StudentState.EnteringDesk);
        _animator.EnterDesk();

        Vector3 initialPos = _desk.OutOfDeskTransform.position;
        float animProgress = _animator.GetCurrentStudentAnimationProgress();
        yield return new WaitUntil(() => { animProgress = _animator.GetCurrentStudentAnimationProgress(); return animProgress < 1.0f; });

        float speed = _agent.speed;
        _agent.speed = 0.0f;
        _agent.SetDestination(SitSpot.position);
        while (_state == StudentState.EnteringDesk)
        {
            animProgress = _animator.GetCurrentStudentAnimationProgress();
            transform.position = Vector3.Lerp(initialPos, SitSpot.position, animProgress);
            yield return new WaitForEndOfFrame();
        }
        
        _agent.speed = speed;

        // StandingOnDesk completed
    }

    public void LeaveDesk()
    {
        // return StartCoroutine(LeaveDesk_());
        StopAndClear();
        EnqueueAction(LeaveDesk_());
    }

    //IEnumerator LeaveDesk_()
    //{
    //    if (_state == StudentState.StandingOutOfDesk || _state == StudentState.EnteringClass || _state == StudentState.Expelled) return null;

    //    return LeaveDeskCoroutine_();
    //}

    IEnumerator LeaveDesk_()
    {
        if (_state != StudentState.Sitting && _state != StudentState.StandingOnDesk) yield break;

        if (_state == StudentState.Sitting) StandUp();
        // first we wait for stand up animation to finish
        yield return new WaitUntil(() => _state == StudentState.StandingOnDesk);

        ChangeState(StudentState.LeavingDesk);
        _animator.ExitDesk();


        Vector3 initialPos = transform.position;
        float animProgress = 0.0f;

        float speed = _agent.speed;
        _agent.speed = 0.0f;
        _agent.SetDestination(_desk.OutOfDeskTransform.position);
        while (_state == StudentState.LeavingDesk)
        {
            animProgress = _animator.GetCurrentStudentAnimationProgress();
                
            transform.position = Vector3.Lerp(initialPos, _desk.OutOfDeskTransform.position, animProgress);
            yield return new WaitForEndOfFrame();
        }

        _agent.speed = speed;
        // leaving desk completed
    }
    #endregion

    #region Yell
    public void Yell()
    {
        // TTSSpeaker;
        // _tts.AudioSystem.pla

        // XXX: @DavidRainder suggested not a fatal-error but a warning
        // Didascalia.Utils.Error.DebugbreakFailUnimplemented("Yell Animation is not avaliable", this);
        Didascalia.Utils.Log.Warning("Yell Animation is not avaliable", this);
    }
    #endregion

    #region SitTogether
    public void SitTogether()
    {
        OnSitTogetherRequested.Invoke();
    }
    #endregion


    public void Hyperstimulate()
    {
        OnHyperstimulateRequested.Invoke();
    }
    public void Frustrate()
    {
        OnFrustrateRequested.Invoke();
    }
    public void GetMaterialOut()
    {
        OnGetMaterialOutRequested.Invoke();
    }
    public void FailToPayAttention()
    {
        OnFailToPayAttentionRequested.Invoke();
    }
    public void GetDistracted()
    {
        OnGetDistractedRequested.Invoke();
    }

    #endregion

    #region Action Queue

    // Estructura de datos nativa de C# para manejar colas (First In, First Out)
    private Queue<IEnumerator> actionQueue = new Queue<IEnumerator>();

    // Guardamos la referencia de la corrutina principal para poder detenerla
    private Coroutine queueProcessor;

    /// <summary>
    /// Añade una nueva corrutina a la cola. Si la cola estaba inactiva, la inicia.
    /// </summary>
    public void EnqueueAction(IEnumerator action)
    {
        actionQueue.Enqueue(action);

        // Si no hay ninguna cola procesándose en este momento, arrancamos el motor
        if (queueProcessor == null)
        {
            queueProcessor = StartCoroutine(ProcessQueue());
        }
    }

    /// <summary>
    /// Bucle interno que procesa las acciones una a una hasta vaciar la cola.
    /// </summary>
    private IEnumerator ProcessQueue()
    {
        // Mientras haya elementos en la cola...
        while (actionQueue.Count > 0)
        {
            // 1. Sacamos la acción más antigua (la primera que entró)
            IEnumerator nextAction = actionQueue.Dequeue();

            // 2. Iniciamos la acción y ESPERAMOS a que termine por completo
            yield return nextAction;
        }

        // 3. Cuando la cola se vacía, liberamos la referencia
        queueProcessor = null;
    }

    /// <summary>
    /// Detiene la ejecución actual y limpia todo el estado.
    /// </summary>
    public void StopAndClear()
    {
        if (queueProcessor != null)
            StopCoroutine(queueProcessor);

        // Vaciamos la cola de acciones pendientes
        actionQueue.Clear();

        // Reseteamos el estado del procesador
        queueProcessor = null;
    }
    #endregion
}
