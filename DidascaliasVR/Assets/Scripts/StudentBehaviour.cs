using System;
using System.Collections;
using Meta.WitAi.TTS.Integrations;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum StudentState
{
    Sitting,
    Standing,
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

    public Transform SitSpot { get { return transform.parent; } }

    public void ChangeSitSpotWithStudent(StudentBehaviour other)
    {
        Transform thisParent = SitSpot;
        transform.parent = other.transform.parent;
        other.transform.parent = thisParent;
    }

    [SerializeField]
    private StudentState _state;

    public UnityEvent OnStandUp = new UnityEvent();
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
        ChangeState(StudentState.Sitting);
    }

    private void OnDestroy()
    {
        OnStandUp.RemoveListener(ChangeStateOnStandUp);
    }

    private void ChangeStateOnStandUp()
    {
        ChangeState(StudentState.Standing);
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
        float speed = _animator.Animator.GetFloat(hashFloatSpeed);
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

            _animator.Animator.SetFloat(hashFloatSpeed, speed);

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

    IEnumerator MovementAnimationAndRotate(Transform transform, float rotateTime)
    {
        yield return MoveTo(transform);
        yield return AcquireTargetRotation(transform.rotation, rotateTime);
    }
    public Coroutine MoveToAndRotate(Transform transform, float rotateTime)
    {
        return StartCoroutine(MovementAnimationAndRotate(transform, rotateTime));
    }

    IEnumerator MovingTowardsPoint(Vector3 point)
    {
        float speed = _agent.speed;
        _agent.speed = 0.0f;
        _agent.SetDestination(point);

        if(_state == StudentState.Sitting) StandUp();
        yield return new WaitUntil(() => _state != StudentState.Sitting);

        StartWalking(0.65f);
        _agent.speed = speed;

        yield return new WaitUntil(() => !_agent.pathPending);
        yield return new WaitUntil(() => _agent.remainingDistance <= _agent.stoppingDistance * 2);

        StopWalking(0.95f);
        ChangeState(StudentState.Standing);
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
        _animator.UnsetOnFoot();
    }
    public void StandUp()
    {
        OnStandUpRequested.Invoke();
        if (_animator.Animator.GetBehaviour<OnStandUp>() == null)
        {
            Didascalia.Utils.Error.DebugbreakFailMessage("OnStandUp behaviour not found in animator", this);
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
