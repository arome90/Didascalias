using Didascalia.Student;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using static Didascalia.Student.StudentAnimatorController;

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
    public struct BehaviourPattern
    {
        // these are all 0-100 probabilities        // chance that student wilk ...
        public int willLaughtAtOthers;             //  laugh about conflicted students
        public int willTalkWithOthers;             //  trash talk about conflicted students
        public int willTalkWithConflictedOthers;   //  try to talk directly to conflicted students
        public int willLookAtConflictedOthers;     //  look at conflicted students
    }

    public enum MovementAction
    {
        None = 0,
        WalkMaterial = -1,
        Walk = 1,
        Run = 2,
        RunAnxiety = 3,
    }

    public enum LookDirection
    {
        Front,
        Left,
        Right,
        Back
    }

    LookDirection _lookDirection;
    public LookDirection CurrentLookDirection => _lookDirection;

    public static LookDirection CalculateLookDirectionGivenTarget(Transform target, Transform origin)
    {
        Vector3 dir = (target.position - origin.position).normalized;
        return CalculateLookDirection(dir);
    }

    public static LookDirection CalculateLookDirection(Vector3 targetDirection)
    {
        LookDirection dir = LookDirection.Front;

        // front
        if (targetDirection.z > 0 
            && targetDirection.z > Mathf.Abs(targetDirection.x)) { dir = LookDirection.Front; }
        // back
        else if (targetDirection.z < 0 && Mathf.Abs(targetDirection.z) > Mathf.Abs(targetDirection.x)) { dir = LookDirection.Back; }
        // looking right
        else if (targetDirection.x > 0) { dir = LookDirection.Right; }
        // looking left
        else if (targetDirection.x < 0) { dir = LookDirection.Left; }
        // looking front
        else { dir = LookDirection.Front; }

        return dir;
    }

    // Animator _animator;
    Didascalia.Student.StudentAnimatorController _animator;
    internal Didascalia.Student.StudentAnimatorController Animator => _animator;
    NavMeshAgent _agent;

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

        _animator.SetDeskAnimator(_st.Desk.GetComponentInChildren<Animator>());
        otherSt.GetComponent<StudentAnimatorController>().SetDeskAnimator(otherSt.Desk.GetComponentInChildren<Animator>());

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
    private bool _carryingClassMaterial = false;

    public bool IsCarryingMaterial => _carryingClassMaterial;
    public void SetIsCarryingMaterial(bool carrying) 
    { 
        _carryingClassMaterial = carrying; 
        _animator.SetIsCarryingMaterial(_carryingClassMaterial);
    }

    private bool _hasAllMaterialOut = false;

    public bool HasMaterialPlaced => _hasAllMaterialOut;

    public void SetHasMaterialOut(bool hasMaterialPlaced) => _hasAllMaterialOut = hasMaterialPlaced;

    public void PlaceMaterial()
    {
        if (!IsCarryingMaterial) return;
        _animator.PlaceMaterial();
    }

    private Desk _desk => _st.Desk;

    [SerializeField]
    private StudentState _state;

    [Header("Parameters")]

    [SerializeField, Range(0.0001f, 1.0f), Tooltip("Speed with which the student moves out of the desk")]
    private float _exitDeskSpeed = 0.001f;

    [SerializeField, Range(1.0f, 5.0f), Tooltip("Base Walking Speed")]
    private float _baseWalkingSpeed = 1.5f;

#if UNITY_EDITOR
    [Header("Debug")]
    [SerializeField]
    private TextMeshProUGUI _debugStateText = null;
#endif

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

    // TODO: check
    #region Depracated
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnStandUpRequested = new UnityEvent();
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnSitDownRequested = new UnityEvent();
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnExpellingRequested = new UnityEvent();
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnChangePlacesRequested = new UnityEvent();
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnSitTogetherRequested = new UnityEvent();
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnHyperstimulateRequested = new UnityEvent();
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnFrustrateRequested = new UnityEvent();
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnGetMaterialOutRequested = new UnityEvent();
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnFailToPayAttentionRequested = new UnityEvent();
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnGetDistractedRequested = new UnityEvent();
    [Obsolete("StateMachines work via StudentBehaviour and Animator now")]
    public UnityEvent OnYellRequested = new UnityEvent();
    #endregion

    public StudentState State { get { return _state; } }

    private PlayerResolutionToConflict _currentPlayerResolution = PlayerResolutionToConflict.None;

    // this will affect the student's reaction when a conflict is resolved (positively, neutrally or badly)
    BehaviourPattern _behaviourPattern;

    private void Awake()
    {
        _animator = GetComponentInChildren<Didascalia.Student.StudentAnimatorController>();
    }

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _st = GetComponent<Student>();

        _initialSpeed = _agent.speed;

        _agent.enabled = !IsSitting();

        _hasAllMaterialOut = false;

        Didascalia.Utils.Error.DebugbreakFailIf(_animator == null, "Animator component not found", this);
        Didascalia.Utils.Error.DebugbreakFailIf(_agent == null, "NavMeshAgent component not found", this);
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

    public void SetBehaviourPattern(BehaviourPatternModifier modifier)
    {
        _behaviourPattern = new BehaviourPattern();
        _behaviourPattern.willLaughtAtOthers =             modifier._laughAtOthers + UnityEngine.Random.Range(-modifier._laughModifier, modifier._laughModifier + 1);
        _behaviourPattern.willTalkWithOthers =             modifier._talkAboutOthers + UnityEngine.Random.Range(-modifier._talkAboutModifier, modifier._talkAboutModifier + 1);
        _behaviourPattern.willTalkWithConflictedOthers =   modifier._talkWithOthers + UnityEngine.Random.Range(-modifier._talkToModifier, modifier._talkToModifier + 1);
        _behaviourPattern.willLookAtConflictedOthers =     modifier._lookAtOthers + UnityEngine.Random.Range(-modifier._lookAtModifier, modifier._lookAtModifier + 1);
    }

    bool _isADHD =  false;
    public void SetADHD(bool isADHD)
    {
        _isADHD = isADHD;
    }

    bool _isTEA =   false;

    public void SetAutism(bool isTEA)
    {
        _isTEA = isTEA;
        _animator.SetIsTEA(_isTEA);
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
#if UNITY_EDITOR
        if (_debugStateText != null) _debugStateText.SetText(_state.ToString());
#endif
    }

    public void SitDownStudent()
    {
        OnSitDownRequested.Invoke();
    }

    public void ExpelStudent()
    {
        OnExpellingRequested.Invoke();
    }

    public void StopLookingToSide()
    {
        _animator.StopLookingAtSide();

        _lookDirection = LookDirection.Front;
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
        if(_carryingClassMaterial)
            return MovementAnimation_(MovementAction.WalkMaterial, time);
        else
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

        if (_carryingClassMaterial) movementAction = MovementAction.WalkMaterial;

        float goal = 0.0f;
        float agentSpeedGoal = 0.0f;
        switch (movementAction) 
        { 
            case MovementAction.None: { goal = 0.0f; agentSpeedGoal = 0.0f; break; }
            case MovementAction.WalkMaterial: { goal = -1.0f; agentSpeedGoal = 1.0f; break; }
            case MovementAction.Walk: { goal = 1.0f; agentSpeedGoal = 1.0f; break; }
            case MovementAction.Run: { goal = 2.0f; agentSpeedGoal = 2.0f; break; }
            case MovementAction.RunAnxiety: { goal = 3.0f; agentSpeedGoal = 2.0f; break; }
        }

        agentSpeedGoal *= _baseWalkingSpeed;
        float agentSpeed = 0.0f;
        float initialAgentSpeed = _agent.speed;

        float elapsedTime = 0.0f;
        bool done = false;
        while(!done)
        {
            elapsedTime += Time.deltaTime;
            speed = Mathf.Lerp(initialSpeed, goal, elapsedTime / time);

            agentSpeed = Mathf.Lerp(initialAgentSpeed, agentSpeedGoal, elapsedTime / time);
            _agent.speed = agentSpeed;

            _animator.StudentAnimator.SetFloat(hashFloatSpeed, speed);

            yield return null;

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

    public IEnumerator MoveToRandomPoint(MovementAction movementAction)
    {
        // we get a random student just to get a "secure" place to go to. Rework this, maybe (?
        Student st = StudentManager.Instance.TryGetStudentByNameOrGetRandom(null);

        yield return MovingTowardsPoint_(st.Desk.OutOfDeskTransform.position, movementAction);
    }

    public void Expel()
    {
        StopAndClearActionQueue();
        EnqueueAction(Expel_());
    }

    public IEnumerator Expel_()
    {
        yield return MoveToFrontDoor_();
        yield return OpenDoorInside_();
        yield return MovementAnimationAndRotate_(ClassManager.Instance.FrontDoor.OutsideStandingPoint, 0.95f);
        yield return CloseDoorOutside_();
    }

    public IEnumerator OpenDoorInside_()
    {
        if (ClassManager.Instance.FrontDoor.IsOpen) yield break;

        ChangeState(StudentState.OpeningDoor);
        _animator.OpenDoorInside(ClassManager.Instance.FrontDoor);

        yield return new WaitUntil(() => _state == StudentState.StandingOutOfDesk);
    }

    public IEnumerator CloseDoorOutside_()
    {
        if (!ClassManager.Instance.FrontDoor.IsOpen) yield break;

        ChangeState(StudentState.ClosingDoor);
        _animator.CloseDoorOutside(ClassManager.Instance.FrontDoor);

        yield return new WaitUntil(() => _state == StudentState.Expelled);
    }

    IEnumerator OpenDoorOutside_()
    {
        if (ClassManager.Instance.FrontDoor.IsOpen) yield break;

        ChangeState(StudentState.OpeningDoor);
        _animator.OpenDoorOutside(ClassManager.Instance.FrontDoor);

        yield return new WaitUntil(() => _state == StudentState.StandingOutOfDesk);
    }

    IEnumerator CloseDoorInside_()
    {
        if (!ClassManager.Instance.FrontDoor.IsOpen) yield break;

        ChangeState(StudentState.ClosingDoor);
        _animator.CloseDoorInside(ClassManager.Instance.FrontDoor);

        yield return new WaitUntil(() => _state == StudentState.StandingOutOfDesk);
    }

    // external
    public void MoveToFrontDoor(bool restrictive = false)
    {
        if (restrictive) StopAndClearActionQueue();
        EnqueueAction(MoveToFrontDoor_());
        //return StartCoroutine(MoveToFrontDoorCoroutine_());
    }

    // internal
    public IEnumerator MoveToFrontDoor_()
    {
        return MovementAnimationAndRotate_(ClassManager.Instance.FrontDoor.InsideStandingPoint, 0.65f);
    }

    IEnumerator AcquireTargetRotation_(Quaternion rotation, float time)
    {
        Quaternion initialRotation = transform.rotation;

        float elapsedTime = 0.0f;
        while(elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(initialRotation, rotation, elapsedTime / time);

            yield return null;
        }
        transform.rotation = rotation;
        yield return null;
        // Didascalia.Utils.Error.DebugbreakFailUnimplemented("AcquireTargetRotation is not fully implemented, it should be able to be interrupted by other calls to this method or to MoveTo", this);
    }

    public IEnumerator SmoothLookAt_(Transform target, float time)
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

    public IEnumerator MovementAnimationAndRotate_(Transform transform, float rotateTime, MovementAction action = MovementAction.Walk, UnityAction callback = null)
    {
        yield return MovingTowardsPoint_(transform.position);
        yield return AcquireTargetRotation_(transform.rotation, rotateTime);

        if (callback != null)
            callback.Invoke();
    }

    // external
    public Coroutine MoveToAndRotate(Transform transform, float rotateTime, MovementAction action = MovementAction.Walk, UnityAction callback = null)
    {
        return StartCoroutine(MovementAnimationAndRotate_(transform, rotateTime, action, callback));
    }

    IEnumerator MoveToFrontDoorOutsidePoint_()
    {
        ChangeState(StudentState.EnteringClass);
        yield return MovementAnimationAndRotate_(ClassManager.Instance.FrontDoor.OutsideStandingPoint, 0.0f);
    }



    IEnumerator EnterClass_()
    {
        if (_state != StudentState.Expelled) yield break;

        yield return MoveToFrontDoorOutsidePoint_();

        yield return OpenDoorOutside_();

        yield return MoveToFrontDoor_();

        yield return CloseDoorInside_();
    }

    public IEnumerator MoveAndLookToStudent_(Student st)
    {
        yield return MovingTowardsPoint_(st
    .       Desk.GetNearestOutOfDeskPosition(transform).position, MovementAction.Walk);
        yield return SmoothLookAt_(st.transform, 0.6f);
    }

    public IEnumerator MovingTowardsPoint_(Vector3 point, MovementAction movementAction = MovementAction.Walk)
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

    public bool IsSitting() { return IsSittingOnChair() || IsSittingOnFloor(); }
    public bool IsSittingOnChair() { return State == StudentState.SittingOnChair; }
    public bool IsSittingOnFloor() { return State == StudentState.SittingOnFloor; }

    public bool IsStanding() { return IsStandingOnDesk() || IsStandingOutOfDesk() || IsStandingRightOutOfDesk(); }
    public bool IsStandingOnDesk() { return State == StudentState.StandingOnDesk; }
    public bool IsStandingOutOfDesk() { return State == StudentState.StandingOutOfDesk; }
    public bool IsStandingRightOutOfDesk() { return State == StudentState.StandingRightOutOfDesk; }

    public bool IsOutOfClass() { return State == StudentState.Expelled; }

    public bool IsLeavingDesk() { return State == StudentState.LeavingDesk; }

    public void StandUp()
    {
        if (IsSitting())
        {
            StopAndClearActionQueue();
            EnqueueAction(StandUp_());
        }
    }

    public IEnumerator StandUp_()
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

        // to avoid any trouble, we reset the student's look direction after standing up
        _lookDirection = LookDirection.Front;
        _animator.SetLookDirection(_lookDirection);
    }

    public bool IsSittingOnTheirDesk()
    {
        return _state == StudentState.SittingOnChair && _st.Desk == _st.OriginalDesk;
    }

    public bool IsSittingOnOtherDesk()
    {
        return _state == StudentState.SittingOnChair && _st.Desk != _st.OriginalDesk;
    }

    public void GoToFloor()
    {
        if(IsSittingOnFloor()) return;

        StopAndClearActionQueue();
        EnqueueAction(GoToFloor_());
    }

    public IEnumerator GoToFloor_()
    {
        if (IsSittingOnFloor()) { yield break; }
        if (IsSittingOnChair() || IsStandingOnDesk() || IsLeavingDesk()) { yield return LeaveDesk_(); }
        if (IsOutOfClass()) { yield return EnterClass_(); }

        yield return SitOnFloor_();
    }

    public IEnumerator SitOnFloor_()
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

    public IEnumerator SitDown_()
    {
        yield return EnterOriginalDesk_();

        _animator.SitDown();
        yield return new WaitUntil(() => IsSittingOnChair());
        SetLookDirection(LookDirection.Front);
    }

    public IEnumerator EnterOriginalDesk_()
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

        _agent.enabled = false;

        ChangeState(StudentState.EnteringDesk);
        _animator.EnterDesk();

        Vector3 initialPos = _desk.OutOfDeskTransform.position;
        float animProgress = _animator.GetCurrentStudentAnimationProgress();

        yield return new WaitUntil(() => { animProgress = _animator.GetCurrentStudentAnimationProgress(); 
            return animProgress < 1.0f; });

        while (_state == StudentState.EnteringDesk)
        {
            animProgress = _animator.GetCurrentStudentAnimationProgress();
            transform.position = Vector3.Lerp(initialPos, SitSpot.position, animProgress);
            yield return null;
        }
        
        // _animator.GetDeskMaterialOut();

        yield return new WaitUntil(() => !_carryingClassMaterial);

        // StandingOnDesk completed
    }

    public IEnumerator TalkToSomeoneForTime_(Student st, float talkTime)
    {
        yield return SmoothLookAt_(st.transform, 0.65f);

        StartTalking(false);
        yield return new WaitForSeconds(talkTime);
        StopTalking();
    }

    public void StartTalking(bool onlyMoveMouth = false)
    {
        _animator.StartTalking(onlyMoveMouth, IsSittingOnChair());
    }

    public void StopTalking() =>_animator.StopTalking();

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

    public IEnumerator SitOnNewPlace_(Desk newPlace)
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

    public IEnumerator LeaveDesk_()
    {
        if (!IsSittingOnChair() && !IsStandingOnDesk()) yield break;

        yield return StandUp_();

        ChangeState(StudentState.LeavingDesk);
        _animator.ExitDesk();


        Vector3 initialPos = transform.position;
        float animProgress = 0.0f;

        float speed = _agent.speed;
        _agent.speed = 0.0f;
        if(_agent.enabled) _agent.SetDestination(_desk.OutOfDeskTransform.position);

        while (_state == StudentState.LeavingDesk)
        {
            animProgress = _animator.GetCurrentStudentAnimationProgress();
                
            transform.position = Vector3.Lerp(initialPos, _desk.OutOfDeskTransform.position, animProgress);
            yield return null;
        }

        _agent.speed = speed;
        _agent.enabled = true;
        // leaving desk completed
    }
    #endregion
    public void SetIsJustifying(bool isJustifying)
    {
        _animator.SetIsJustifying(isJustifying);
        _animator.SetHighAnxiety(isJustifying);

        if (isJustifying) StartTalking(false);
        else StopTalking();
    }

    public void SetIsCrying(bool isCrying)
    {
        _animator.SetIsCrying(isCrying);
        _animator.SetHighAnxiety(isCrying);
    }

    #region Player Actions
    //public IEnumerator WaitForPlayerAction()
    //{
    //    ListenToPlayerResolution();

    //    yield return new WaitUntil(() => _currentPlayerResolution != PlayerResolutionToConflict.None);
    //}

    //private void ListenToPlayerResolution()
    //{
    //    Player.Instance.OnPlayerResolution.RemoveListener(OnPlayerResolution);
    //    _currentPlayerResolution = PlayerResolutionToConflict.None;

    //    Player.Instance.OnPlayerResolution.AddListener(OnPlayerResolution);
    //    Player.StartListeningForPlayerResolution();
    //}

    //private void OnPlayerResolution(PlayerResolutionToConflict res)
    //{
    //    _currentPlayerResolution = res;
    //}
    #endregion

    #region Target
    Transform _target = null;

    public void SetTarget(Transform target) => _target = target;

    public void LookAtTarget(Transform newTarget = null)
    {
        if (newTarget != null) SetTarget(newTarget);
        if (_target != null)
        {
            _lookDirection = CalculateLookDirectionGivenTarget(_target, transform);
            _animator.SetLookDirection(_lookDirection);
        }
    }

    public void SetLookDirection(LookDirection lookDir)
    {
        _lookDirection = lookDir;
       _animator.SetLookDirection(_lookDirection);
    }

    #endregion

    // CONFLICTS

    #region StandUp
    //public void StandUpConflict()
    //{
    //    if (!IsSittingOnChair()) return;

    //    StopAndClearActionQueue();
    //    EnqueueAction(StandUpConflict_());
    //}

    //public IEnumerator StandUpConflict_()
    //{
    //    ListenToPlayerResolution();
    //    yield return StandUp_();

    //    while (_currentPlayerResolution == PlayerResolutionToConflict.None)
    //        yield return WaitForPlayerAction();

    //    if (_currentPlayerResolution == PlayerResolutionToConflict.Positive) yield return SitDown_();
    //    else if (_currentPlayerResolution == PlayerResolutionToConflict.Neutral)
    //    {
    //        ListenToPlayerResolution();
    //        yield return BotherSomeone_(null);

    //        while ( _currentPlayerResolution == PlayerResolutionToConflict.None 
    //            ||  _currentPlayerResolution == PlayerResolutionToConflict.Neutral)
    //            yield return WaitForPlayerAction();

    //        if (_currentPlayerResolution == PlayerResolutionToConflict.Negative)
    //        {
    //            StopBotheringTarget();
    //            yield return Expel_();
    //        }
    //        else if (_currentPlayerResolution == PlayerResolutionToConflict.Positive)
    //        {
    //            StopBotheringTarget();
    //            yield return SitDown_();
    //        }
    //    }
    //    else if (_currentPlayerResolution == PlayerResolutionToConflict.Negative)
    //    {
    //        SetIsJustifying(true);
    //        StartTalking();

    //        yield return WaitForPlayerAction();

    //        if (_currentPlayerResolution == PlayerResolutionToConflict.Positive) yield return SitDown_();

    //        // conflict failed 
    //        else
    //        {
    //            SetIsJustifying(false);
    //            StopTalking();
    //            yield return Expel_();
    //        }
    //    }
    //}
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
    //public void ChangeSitsWithRandomStudentConflict()
    //{
    //    Student farStudent = StudentManager.Instance.GetStudentFarFromOtherStudent(_st);

    //    ChangeSitWithGivenStudentConflict(farStudent);
    //}

    //public void ChangeSitWithGivenStudentConflict(Student farStudent)
    //{
    //    StopAndClearActionQueue();
    //    EnqueueAction(SitNextToRandomStudentConflict_(farStudent));
    //}
    #endregion

    #region Draw Conflict
    //public void DrawDistacted()
    //{
    //    // only for adhd sitting kids
    //    if (!_isADHD
    //        || !IsSittingOnChair()
    //        || !_hasAllMaterialOut)
    //        return;

    //    StopAndClearActionQueue();
    //    EnqueueAction(DrawDistacted_());
    //}
    #endregion

    #region BotherOtherStudents

    public void SetAnnoyed(bool annoyed, Transform target)
    {
        SetTarget(target);
        _animator.SetAnnoyed(annoyed, _target);
    }

    public void SetIsBothering(bool isBothering) => _animator.SetIsBothering(isBothering);

    public void StopBotheringTarget()
    {
        if (_target != null)
        {
            Student st = _target.GetComponent<Student>();
            if (st != null) st.Behaviour.SetAnnoyed(false, transform);
        }

        SetIsBothering(false);
        StopTalking();
    }

    public IEnumerator BotherSomeone_(Student st = null)
    {
        if (st == null) { st = StudentManager.Instance.GetNearestStudent(_st); }

        SetTarget(st.transform);

        if (IsSittingOnChair())
        {
            // this only takes effect while sitting, so no need to do it otherwise
            _lookDirection = CalculateLookDirectionGivenTarget(st.transform, transform);
            _animator.SetLookDirection(_lookDirection);
        }

        // if standing, move to them
        if (IsStanding())
        {
            yield return MovingTowardsPoint_(st.Desk.OutOfDeskTransform.position, MovementAction.Walk);
            yield return SmoothLookAt_(st.transform, 1.0f);
        }

        _animator.SetIsBothering(true);
        StartTalking(true);

        st.Behaviour.SetAnnoyed(true, transform);
    }


    #endregion

    public void SetIsWriting(bool isWriting)
    {
        if (HasMaterialPlaced) _animator.SetWriting(isWriting);
    }
    #region Material Gone Wrong

    public void TriggerStandUpWhileWrongMaterial() => _animator.SetDeskTriggerParameter(StudentAnimatorController.HashFromTriggerParameter(TriggerStudentParameter.StopMatFailStandUp));
    public void SetIsWrongMaterial(bool isWrong)
    {
        if (_isADHD)
        {
            if (isWrong)    _animator.TDAH_GetMaterialOutWrong();
            else            _animator.TDAH_ResetGetMaterialOutWrong();
        }
    }

    public IEnumerator TakeClassMaterial()
    {
        yield return LeaveDesk_();
        yield return MovementAnimationAndRotate_(ClassManager.Instance.ClassMaterialTransform, 0.4f, MovementAction.Walk);
        
        _animator.TDAH_GetClassMaterial();
        yield return new WaitUntil(() => _carryingClassMaterial);

        yield return SitDown_();

        SortMaterial();
    }

    private void SortMaterial()
    {
        _animator.SortMaterial();
    }

    #endregion

    public void SetAnxiety(bool anxious)
    {
        if (_isTEA) _animator.TEA_SetAnxiety(anxious);
        else        _animator.SetAnxiety(anxious);
    }
    public void SetIsOff(bool isOff)
    {
        if (_isTEA) _animator.TEA_IsOff(isOff);
        else        _animator.SetIsOff(isOff);
    }

    public void SetHighAnxiety(bool highAnxiety)
    {
        if (_isTEA) _animator.TEA_SetHighAnxiety(true);
        else        _animator.SetHighAnxiety(true);
    }

    #region Hyperstimulate
    public void SetIsHyperstimulated(bool isHyperstimulated)
    {
        if (isHyperstimulated)  _animator.TEA_StartHyperstimulation();
        else                    _animator.TEA_StopHyperstimulation();
    }

    //public void Hyperstimulate()
    //{
    //    if (    !_isTEA
    //        ||  !IsSittingOnChair())
    //        return;

    //    StopAndClearActionQueue();
    //    // OnHyperstimulateRequested.Invoke();
    //    EnqueueAction(Hyperstimulate_());
    //}

    #endregion

    public void SetIsDistracted(bool isDistracted)
    {
        if (_isTEA)
        {
            _animator.TEA_SetIsDistracted(isDistracted);
        }
        else if (HasMaterialPlaced)
        {
            _animator.SetIsDrawing(isDistracted);
        }
    }

    #region Get Distracted TEA

    //public void GetDistractedTEA()
    //{
    //    if (    !_isTEA
    //        ||  !IsSittingOnChair())
    //        return;

    //    // OnGetDistractedRequested.Invoke();
    //    StopAndClearActionQueue();
    //    EnqueueAction(GetDistracted_());
    //}

    

    #endregion

    // CONFLICT REACTIONS FROM OTHER STUDENTS
    #region Reactions

    public void ReactToPositivelyResolvedConflict(Student conflictedStudent)
    {
        // only looking at conflicted student
        int max = _behaviourPattern.willLookAtConflictedOthers;

        int chance = UnityEngine.Random.Range(0, max + 1);

        // looking at conflicted student
        if (chance < _behaviourPattern.willLookAtConflictedOthers)
        {
            LookAtTarget(conflictedStudent.transform);
        }
    }

    public void ReactToNeutrallyResolvedConflict(Student conflictedStudent)
    {
        // look or talk with conflicted student
        int max =   _behaviourPattern.willLookAtConflictedOthers +
                    _behaviourPattern.willTalkWithConflictedOthers;

        int chance = UnityEngine.Random.Range(0, max + 1);

        // talk with conflicted student
        if (chance < _behaviourPattern.willTalkWithConflictedOthers)
        {
            LookAtTarget(conflictedStudent.transform);
            StartTalking();
            return;
        }

        chance -= _behaviourPattern.willTalkWithOthers;

        // looking at conflicted student
        if (chance < _behaviourPattern.willLookAtConflictedOthers)
        {
            LookAtTarget(conflictedStudent.transform);
        }
    }

    public void ReactToBadlyResolvedConflict(Student conflictedStudent)
    {
        int max =   _behaviourPattern.willTalkWithOthers + 
                    _behaviourPattern.willLaughtAtOthers + 
                    _behaviourPattern.willLookAtConflictedOthers + 
                    _behaviourPattern.willTalkWithConflictedOthers;

        int chance = UnityEngine.Random.Range(0, max + 1);

        // laugh   
        if (chance < _behaviourPattern.willLaughtAtOthers)
        {
            LookAtTarget(conflictedStudent.transform);
            Laugh();
            return;
        }

        chance -=_behaviourPattern.willLaughtAtOthers;

        // talk with another student about the situation
        if (chance < _behaviourPattern.willTalkWithOthers)
        {
            // get another student different from us and from conflicted one to talk with
            List<Student> others = new List<Student>(); others.Add(conflictedStudent); others.Add(_st);
            LookAtTarget(StudentManager.Instance.GetStudentDifferentFromGiven(others).transform);
            StartTalking();
            return;
        }

        chance -= _behaviourPattern.willTalkWithOthers;

        // talk with conflicted student
        if (chance < _behaviourPattern.willTalkWithConflictedOthers)
        {
            LookAtTarget(conflictedStudent.transform);
            StartTalking();
            return;
        }

        chance -= _behaviourPattern.willTalkWithOthers;

        // looking at conflicted student
        if (chance < _behaviourPattern.willLookAtConflictedOthers)
        {
            LookAtTarget(conflictedStudent.transform);
        }
    }

    #region Laugh
    public void Laugh()
    {
        _animator.SetIsLaughing(true);
        if (UnityEngine.Random.Range(0, 2) == 0) _animator.SetIsPointing(true);
    }
    #endregion
    #endregion

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
        if (_agent.enabled) _agent.SetDestination(_agent.transform.position);
        _agent.speed = _initialSpeed;
        _animator.StudentAnimator.SetFloat(Didascalia.Student.StudentAnimatorController.HashFloatSpeed, 0.0f);
    }
    #endregion
}
