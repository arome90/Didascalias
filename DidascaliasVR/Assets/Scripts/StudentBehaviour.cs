using Meta.WitAi.TTS.Integrations;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum StudentState
{
    Sitting,
    StandingOnDesk,
    StandingOutOfDesk,
    LeavingDesk,
    EnteringDesk,
    Walking,
    Expelling,
    Expelled
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


        OnStandUp.AddListener(ChangeStateOnStandUp);
        OnExitDesk.AddListener(ChangeStateOnExitDesk);
        OnEnterDesk.AddListener(ChangeStateOnEnterDesk);
        OnSitDown.AddListener(ChangeStateOnSitDown);
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
    public void StartWalking(float time)
    {
        StartCoroutine(MovementAnimation(true, time));
    }

    public void StopWalking(float time)
    {
        StartCoroutine(MovementAnimation(false, time));
    }

    IEnumerator MovementAnimation(bool wantsToWalk, float time)
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

    public Coroutine MoveTo(Transform transform)
    {
        return StartCoroutine(MovingTowardsPoint(transform.position)); 
    }

    IEnumerator AcquireTargetRotation(Quaternion rotation, float time)
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

    public Coroutine StartAcquireTargetRotation(Quaternion rotation, float time)
    {
        return StartCoroutine(AcquireTargetRotation(rotation, time));
    }

    IEnumerator MovementAnimationAndRotate(Transform transform, float rotateTime, UnityAction callback = null)
    {
        yield return MovingTowardsPoint(transform.position);
        yield return AcquireTargetRotation(transform.rotation, rotateTime);

        if (callback != null)
            callback.Invoke();
    }

    public Coroutine MoveToAndRotate(Transform transform, float rotateTime, UnityAction callback = null)
    {
        return StartCoroutine(MovementAnimationAndRotate(transform, rotateTime, callback));
    }

    IEnumerator MovingTowardsPoint(Vector3 point)
    {
        if (_state == StudentState.Sitting || _state == StudentState.StandingOnDesk) LeaveDesk();

        yield return new WaitUntil(() => _state == StudentState.StandingOutOfDesk);

        float speed = _agent.speed;
        _agent.speed = 0.0f;
        _agent.SetDestination(point);

        // if we're not standing out of desk...
        LeaveDesk();
        // then we wait for leaving desk animation to finish
        yield return new WaitUntil(() => _state == StudentState.StandingOutOfDesk);

        ChangeState(StudentState.Walking);
        StartWalking(0.65f);
        _agent.speed = speed;

        yield return new WaitUntil(() => !_agent.pathPending);
        yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance * 2);

        StopWalking(0.95f);
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

        EnterDesk();

        StartCoroutine(SitDownCoroutine());
    }

    IEnumerator SitDownCoroutine()
    {
        yield return new WaitUntil(() => _state == StudentState.StandingOnDesk);

        _animator.SitDown();
    }

    public void EnterDesk()
    {
        if (_state == StudentState.StandingOnDesk || _state == StudentState.Sitting) return;

        StartCoroutine(EnterDeskCoroutine());
    }

    IEnumerator EnterDeskCoroutine()
    {
        if (_state == StudentState.StandingOutOfDesk)
        {
            yield return MoveToAndRotate(_st.Desk.OutOfDeskTransform, 0.65f);
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
        if (_state == StudentState.StandingOutOfDesk) return;

        StartCoroutine(LeaveDeskCoroutine());
    }

    IEnumerator LeaveDeskCoroutine()
    {
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
}
