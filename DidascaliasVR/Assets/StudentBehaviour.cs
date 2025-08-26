using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem.OnScreen;

public enum StudentState
{
    Sitting,
    Standing,
    Walking,
    Expelling,
    Expelled
}

/// <summary>
/// Contiene todos los métodos que se encargan del comportamiento del estudiante.
/// Todos ellos son llamados en la máquina de estados del prefab de Student
/// </summary>
public class StudentBehaviour : MonoBehaviour
{
    Animator _animator;
    NavMeshAgent _agent;

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
    public UnityEvent OnSitDownRequested = new UnityEvent();
    public UnityEvent OnExpellingRequested = new UnityEvent();
    public UnityEvent OnChangePlacesRequested = new UnityEvent();

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.O))
        {
            OnStandUp.Invoke();
        }
        if(Input.GetKeyUp(KeyCode.P))
        {
            OnSitDownRequested.Invoke();
        }
        if (Input.GetKeyUp(KeyCode.I))
        {
            OnExpellingRequested.Invoke();
        }
    }

    public StudentState State { get { return _state; } }
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

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
        _animator.SetBool("OnFoot", false);
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
        float speed = _animator.GetFloat("Speed");
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

            _animator.SetFloat("Speed", speed);

            yield return new WaitForEndOfFrame();

            done = speed == goal;
        }
    }

    public void MoveTo(Transform transform)
    {
        StartCoroutine(MovingTowardsPoint(transform.position));
    }

    IEnumerator MovingTowardsPoint(Vector3 point)
    {
        float speed = _agent.speed;
        _agent.speed = 0.0f;
        _agent.SetDestination(point);

        if(_state == StudentState.Sitting) StartStandUpAnimation();
        while (_state == StudentState.Sitting)
        {
            yield return null;
        }

        StartWalking(0.65f);
        _agent.speed = speed;

        while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance * 2)
        {
            yield return new WaitForEndOfFrame();
        }

        StopWalking(0.95f);
        ChangeState(StudentState.Standing);
    }
    #endregion

    #region StandUp
    public void StartStandUpAnimation()
    {
        _animator.SetBool("OnFoot", true);
    }
    #endregion
}
