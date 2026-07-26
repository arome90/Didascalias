using Didascalia.Student;
using Meta.WitAi.TTS.Integrations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum StudentState
{
    SittingOnChair,
    SittingOnFloor,

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
    ClosingDoor,
}


/// <summary>
/// Contiene todos los m�todos que se encargan del comportamiento del estudiante.
/// Todos ellos son llamados en la m�quina de estados del prefab de Student
/// </summary>
public class StudentBehaviour : MonoBehaviour
{

    public enum MovementAction
    {
        None = 0,
        Walk = 1,
        Run = 2,
        RunAnxiety = 3,
    }


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

    public void ChangeDeskWithStudent(StudentBehaviour other, bool changeOriginalDeskToo)
    {
        Transform thisParent = SitSpot;
        transform.parent = other.transform.parent;
        other.transform.parent = thisParent;

        Student otherSt = other.GetComponent<Student>();

        Desk thisDesk = _st.Desk;
        _st.Desk = otherSt.Desk;
        otherSt.Desk = thisDesk;

        if (changeOriginalDeskToo)
        {
            _st.OriginalDesk = _st.Desk;
            otherSt.OriginalDesk = otherSt.Desk;
        }
    }

    public void ChangeDesk(Desk newDesk, bool changeOriginalDeskToo)
    {
        transform.parent = newDesk.StudentPosition;
        _st.Desk = newDesk;

        if (changeOriginalDeskToo)
        {
            _st.OriginalDesk = _st.Desk;
        }
    }

    private float _initialSpeed = 1.0f;

    private Desk _desk => _st.Desk;

    [SerializeField]
    private StudentState _state;

    [Header("Parameters")]

    [SerializeField, Range(0.0001f, 1.0f), Tooltip("Speed with which the student moves out of the desk")]
    private float _exitDeskSpeed = 0.001f;

    [Header("Debug")]
    [SerializeField]
    private TextMeshProUGUI _debugText = null;

    [Header("Events")]
    public UnityEvent OnStandUpChair = new UnityEvent();
    public UnityEvent OnStandUpFloor = new UnityEvent();
    public UnityEvent OnExitDesk = new UnityEvent();
    public UnityEvent OnEnterDesk = new UnityEvent();
    public UnityEvent OnSitDownChair = new UnityEvent();
    public UnityEvent OnSitDownFloor = new UnityEvent();
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

    private PlayerResolutionToConflict _currentPlayerResolution = PlayerResolutionToConflict.None;

    private void Start()
    {
        _animator = GetComponentInChildren<Didascalia.Student.StudentAnimatorController>();
        _agent = GetComponent<NavMeshAgent>();
        _st = GetComponent<Student>();
        _tts = GetComponent<TTSWit>();

        _initialSpeed = _agent.speed;

        Didascalia.Utils.Error.DebugbreakFailIf(_animator == null, "Animator component not found", this);
        Didascalia.Utils.Error.DebugbreakFailIf(_agent == null, "NavMeshAgent component not found", this);
        Didascalia.Utils.Error.DebugbreakFailIf(_tts == null, "TTS component not found", this);
        Didascalia.Utils.Error.DebugbreakFailIf(_st == null, "Student component not found", this);

        // these events happen on the action's end.
        OnStandUpChair.AddListener(ChangeStateOnStandUpChair);
        OnStandUpFloor.AddListener(ChangeStateOnStandUpFloor);
        OnExitDesk.AddListener(ChangeStateOnExitDesk);
        OnEnterDesk.AddListener(ChangeStateOnEnterDesk);
        OnSitDownChair.AddListener(ChangeStateOnSitDownChair);
        OnSitDownFloor.AddListener(ChangeStateOnSitDownFloor);
        OnOpenDoor.AddListener(ChangeStateOnDoorInteraction);
        OnCloseDoor.AddListener(ChangeStateOnDoorInteraction);
        OnExpel.AddListener(ChangeStateOnExpel);
        ChangeState(StudentState.SittingOnChair);
    }

    public void SetTEA(bool isTEA)
    {
        _animator.SetIsTEA(isTEA);
    }

    private void OnDestroy()
    {
        OnStandUpChair.RemoveListener(ChangeStateOnStandUpChair);
        OnStandUpFloor.RemoveListener(ChangeStateOnStandUpFloor);
        OnExitDesk.RemoveListener(ChangeStateOnExitDesk);
        OnEnterDesk.RemoveListener(ChangeStateOnEnterDesk);
        OnSitDownChair.RemoveListener(ChangeStateOnSitDownChair);
        OnSitDownFloor.RemoveListener(ChangeStateOnSitDownFloor);
        OnOpenDoor.RemoveListener(ChangeStateOnDoorInteraction);
        OnCloseDoor.RemoveListener(ChangeStateOnDoorInteraction);
        OnExpel.RemoveListener(ChangeStateOnExpel);
    }

    private void ChangeStateOnStandUpChair()
    {
        ChangeState(StudentState.StandingOnDesk);
    }

    private void ChangeStateOnStandUpFloor()
    {
        ChangeState(StudentState.StandingOutOfDesk);
    }

    private void ChangeStateOnExitDesk()
    {
        ChangeState(StudentState.StandingOutOfDesk);
    }

    private void ChangeStateOnEnterDesk()
    {
        ChangeState(StudentState.StandingOnDesk);
    }

    private void ChangeStateOnSitDownChair()
    {
        ChangeState(StudentState.SittingOnChair);
    }

    private void ChangeStateOnSitDownFloor()
    {
        ChangeState(StudentState.SittingOnFloor);
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
        if (_debugText != null) _debugText.SetText(_state.ToString());
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
        return MovementAnimation_(MovementAction.Walk, time);
    }

    IEnumerator StopMoving_(float time)
    {
        return MovementAnimation_(0, time);
    }

    IEnumerator MovementAnimation_(MovementAction movementAction, float time)
    {
        int hashFloatSpeed = Didascalia.Student.StudentAnimatorController.HashFloatSpeed;
        float speed = _animator.StudentAnimator.GetFloat(hashFloatSpeed);
        float initialSpeed = speed;

        float goal = 0.0f;
        switch (movementAction) 
        { 
            case MovementAction.None: goal = 0.0f; break;
            case MovementAction.Walk: goal = 1.0f; break;
            case MovementAction.Run: goal = 2.0f; break;
            case MovementAction.RunAnxiety: goal = 3.0f; break; 
        }

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

    public void MoveTo(Transform transform, MovementAction movementAction = MovementAction.Walk, bool restrictive = false)
    {
        if (restrictive) 
        { 
            StopAndClearActionQueue();
            EnqueueAction(MovingTowardsPoint_(transform.position, movementAction));
        }
        else StartCoroutine(MovingTowardsPoint_(transform.position, movementAction)); 
    }

    IEnumerator MoveToRandomPoint(MovementAction movementAction)
    {
        // we get a random student just to get a "secure" place to go to. Rework this, maybe (?
        Student st = StudentManager.Instance.TryGetStudentByNameOrGetRandom(null);

        yield return MovingTowardsPoint_(st.Desk.OutOfDeskTransform.position, movementAction);
    }

    public void Expel()
    {
        StopAndClearActionQueue();

        EnqueueAction(MoveToFrontDoor_());
        EnqueueAction(OpenDoorInside_());
        EnqueueAction(MovementAnimationAndRotate_(ClassManager.Instance.FrontDoor.OutsideStandingPoint, 0.95f));
        EnqueueAction(CloseDoorOutside_());
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

    // external
    public void MoveToFrontDoor(bool restrictive = false)
    {
        if (restrictive) StopAndClearActionQueue();
        EnqueueAction(MoveToFrontDoor_());
        //return StartCoroutine(MoveToFrontDoorCoroutine_());
    }

    // internal
    IEnumerator MoveToFrontDoor_()
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

    IEnumerator SmoothLookAt_(Transform target, float time)
    {
        // Seguridad: si no hay target o el tiempo es cero, salimos para evitar bucles infinitos o errores
        if (target == null || time <= 0f) yield break;

        Quaternion initialRotation = transform.rotation;
        float elapsedTime = 0.0f;

        while (elapsedTime < time)
        {
            // 1. Calculamos la dirección hacia el objetivo en este frame
            Vector3 direction = target.position - transform.position;

            // Evitamos un error visual si el target está exactamente en nuestra misma posición
            if (direction != Vector3.zero)
            {
                // 2. Calculamos la rotación objetivo usando LookRotation
                Quaternion targetRot = Quaternion.LookRotation(direction);

                // 3. Incrementamos el tiempo transcurrido
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / time); // Nos aseguramos de que no pase de 1

                // 4. Interpolamos suavemente
                transform.rotation = Quaternion.Slerp(initialRotation, targetRot, t);
            }

            // Esperamos al siguiente frame (ciclo Update)
            yield return null;
        }

        // Al finalizar el bucle, nos aseguramos de mirar perfectamente al objetivo por última vez
        Vector3 finalDirection = target.position - transform.position;
        if (finalDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(finalDirection);
        }
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

        yield return MoveToFrontDoor_();

        yield return CloseDoorInside_();
    }

    IEnumerator MovingTowardsPoint_(Vector3 point, MovementAction movementAction = MovementAction.Walk)
    {
        yield return EnterClass_();

        float speed = _agent.speed;
        _agent.speed = 0.0f;
        // if we're not standing out of desk...
        // then we wait for leaving desk animation to finish
        yield return LeaveDesk_();

        // if we're standing on floor, we should be standing
        yield return StandUp_();

        _agent.SetDestination(point);
        _agent.speed = speed;
        ChangeState(StudentState.Walking);

        yield return MovementAnimation_(movementAction, 0.65f);

        yield return new WaitUntil(() => !_agent.pathPending);
        yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance * 2);

        yield return StopMoving_(0.95f);
        ChangeState(StudentState.StandingOutOfDesk);
    }
    #endregion

    #region Conflicts
    #region StandUp
    internal void SetOnFoot()
    {
        _animator.StandUpFromChair();
    }

    internal void UnsetOnFoot()
    {
        _animator.SitDown();
    }

    private bool IsSitting() { return IsSittingOnChair() || IsSittingOnFloor(); }
    private bool IsSittingOnChair() { return State == StudentState.SittingOnChair; }
    private bool IsSittingOnFloor() { return State == StudentState.SittingOnFloor; }

    private bool IsStanding() { return IsStandingOnDesk() || IsStandingOutOfDesk() || IsStandingRightOutOfDesk(); }
    private bool IsStandingOnDesk() { return State == StudentState.StandingOnDesk; }
    private bool IsStandingOutOfDesk() { return State == StudentState.StandingOutOfDesk; }
    private bool IsStandingRightOutOfDesk() { return State == StudentState.StandingRightOutOfDesk; }

    private bool IsOutOfClass() { return State == StudentState.Expelled; }

    private bool IsLeavingDesk() { return State == StudentState.LeavingDesk; }

    public void StandUp()
    {
        if (IsSitting())
        {
            StopAndClearActionQueue();
            EnqueueAction(StandUp_());
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

    IEnumerator StandUp_()
    {
        if (IsSittingOnChair())
        {
            _animator.StandUpFromChair();
            yield return new WaitUntil(() => State == StudentState.StandingOnDesk);
        }
        else if (IsSittingOnFloor())
        {
            _animator.StandUpFromFloor();
            yield return new WaitUntil(() => State == StudentState.StandingOutOfDesk);
        }
    }

    private bool IsSittingOnTheirDesk()
    {
        return _state == StudentState.SittingOnChair && _st.Desk == _st.OriginalDesk;
    }

    private bool IsSittingOnOtherDesk()
    {
        return _state == StudentState.SittingOnChair && _st.Desk != _st.OriginalDesk;
    }

    public void GoToFloor()
    {
        if(IsSittingOnFloor()) return;

        StopAndClearActionQueue();
        EnqueueAction(GoToFloor_());
    }

    IEnumerator GoToFloor_()
    {
        if (IsSittingOnFloor()) { yield break; }
        if (IsSittingOnChair() || IsStandingOnDesk() || IsLeavingDesk()) { yield return LeaveDesk_(); }
        if (IsOutOfClass()) { yield return EnterClass_(); }

        yield return SitOnFloor_();
    }

    IEnumerator SitOnFloor_()
    {
        _animator.GoToFloor();
        yield return new WaitUntil(() => State == StudentState.SittingOnFloor);
    }

    public void SitDown()
    {
        if (IsSittingOnTheirDesk()) return;

        StopAndClearActionQueue();
        EnqueueAction(SitDown_());
        // return StartCoroutine(SitDownCoroutine_());
    }

    IEnumerator SitDown_()
    {
        yield return EnterOriginalDesk_();

        _animator.SitDown();
    }

    IEnumerator EnterOriginalDesk_()
    {
        yield return EnterClass_();
        
        if (_state == StudentState.StandingOnDesk || IsSittingOnTheirDesk()) yield break;

        if (IsSittingOnOtherDesk())
        {
            yield return LeaveDesk_();
        }

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

    public void SitNextToRandomStudentConflict()
    {
        Student farStudent = StudentManager.Instance.GetStudentFarFromOtherStudent(_st);

        SitNextToGivenStudentConflict(farStudent);
    }

    public void SitNextToGivenStudentConflict(Student farStudent)
    {
        StopAndClearActionQueue();
        EnqueueAction(SitNextToRandomStudentConflict_(farStudent));
    }

    IEnumerator SitNextToRandomStudentConflict_(Student farStudent)
    {
        yield return MovingTowardsPoint_(farStudent.Desk.OutOfDeskTransform.position);

        yield return TalkToSomeoneForTime_(farStudent, 1.0f);

        StudentBehaviour targetBehaviour = farStudent.GetComponent<StudentBehaviour>();

        yield return targetBehaviour.LeaveDesk_();

        ChangeDeskWithStudent(targetBehaviour, false);

        targetBehaviour.SitDown();

        yield return SitDown_();
    }

    IEnumerator TalkToSomeoneForTime_(Student st, float talkTime)
    {
        yield return SmoothLookAt_(st.transform, 0.65f);

        _animator.StartTalking();
        yield return new WaitForSeconds(talkTime);
        _animator.StopTalking();
    }

    private void StartTalking()
    {
        _animator.StartTalking();
        if (IsSittingOnChair()) _animator.SetStudentBooleanParameter(StudentAnimatorController.HashIsTalkingFront);
    }

    private void StopTalking()
    {
        _animator.StopTalking();
        if (IsSittingOnChair()) _animator.ResetStudentBooleanParameter(StudentAnimatorController.HashIsTalkingFront);
    }

    /// <summary>
    /// Cambio de sitio obligado por el profesor.
    /// </summary>
    /// <param name="newPlace"></param>
    public void SitOnNewPlace(Desk newPlace)
    {
        // asumimos que ya ha sido cambiado su sitio con otro estudiante
        StopAndClearActionQueue();
        EnqueueAction(SitOnNewPlace_(newPlace));
    }

    IEnumerator SitOnNewPlace_(Desk newPlace)
    {
        yield return LeaveDesk_();

        // vigilar esta línea, porque cuando se hacen cambios simultáneos puede ser 
        // que las cosas no se pasen por copia, si no por referencia y eso
        // podría causar que solo uno de los estudiantes que cambia de sitio
        // reciba realmente su nuevo escritorio
        ChangeDesk(newPlace, false);

        yield return SitDown_();
    }

    public void LeaveDesk()
    {
        // return StartCoroutine(LeaveDesk_());
        StopAndClearActionQueue();
        EnqueueAction(LeaveDesk_());
    }

    //IEnumerator LeaveDesk_()
    //{
    //    if (_state == StudentState.StandingOutOfDesk || _state == StudentState.EnteringClass || _state == StudentState.Expelled) return null;

    //    return LeaveDeskCoroutine_();
    //}

    IEnumerator LeaveDesk_()
    {
        if (!IsSittingOnChair() && !IsStandingOnDesk()) yield break;

        yield return StandUp_();

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

    IEnumerator WaitForPlayerAction()
    {
        _currentPlayerResolution = PlayerResolutionToConflict.None;
        Player.Instance.OnPlayerResolution.AddListener((res) => _currentPlayerResolution = res);
        Player.StartListeningForPlayerResolution();
        yield return new WaitUntil(() => _currentPlayerResolution != PlayerResolutionToConflict.None);
    }

    #region Hyperstimulate

    public void Hyperstimulate()
    {
        // OnHyperstimulateRequested.Invoke();
        EnqueueAction(Hyperstimulate_());
    }

    // THERE'S WORK TO DO
    // BECAUSE THE "GOOD" OPTION CAN BE INTERRUPTED IF THE USER DOES SOMETHING WRONG AND THEREFORE CAN LEAD INTO 
    // THE NEGATIVE RESOLUTION.
    // WE NEED SOME WAY OF CONTROLLING THIS :)
    IEnumerator Hyperstimulate_()
    {
        _animator.TEA_StartHyperstimulation();

        yield return WaitForPlayerAction();

        switch(_currentPlayerResolution)
        {
            case PlayerResolutionToConflict.None:
                break;
            case PlayerResolutionToConflict.Positive:
                StartTalking();
                yield return new WaitForSeconds(5.0f);
                StopTalking();
                break;
            case PlayerResolutionToConflict.Neutral:
                _animator.TEA_Off();

                yield return WaitForPlayerAction();

                break;
            case PlayerResolutionToConflict.Negative:
                // High Anxiety
                int rand = UnityEngine.Random.Range(0, 3);

                if (rand == 0) _animator.TEA_SetHighAnxiety();
                else if (rand == 1)
                {
                    yield return LeaveDesk_();

                    yield return GoToFloor_();

                    _animator.TEA_SetAnxiety();

                    yield return WaitForPlayerAction();
                }
                else
                {
                    yield return LeaveDesk_();

                    yield return MoveToRandomPoint(MovementAction.RunAnxiety);

                    yield return GoToFloor_();

                    _animator.TEA_SetAnxiety();

                    yield return WaitForPlayerAction();
                }
                break;
        }

        _animator.TEA_StopHyperstimulation();

        Debug.Log("Player has resolved!! oleole");
    }


    #endregion
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
    public void StopAndClearActionQueue()
    {
        if (queueProcessor != null)
            StopCoroutine(queueProcessor);

        // Vaciamos la cola de acciones pendientes
        actionQueue.Clear();

        // Reseteamos el estado del procesador
        queueProcessor = null;

        // reanudamos el movimiento por si acaso se quedó colgado al acabar alguna corrutina
        _agent.SetDestination(_agent.transform.position);
        _agent.speed = _initialSpeed;
        _animator.StudentAnimator.SetFloat(Didascalia.Student.StudentAnimatorController.HashFloatSpeed, 0.0f);
    }
    #endregion
}
