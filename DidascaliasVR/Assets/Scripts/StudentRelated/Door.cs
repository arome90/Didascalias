using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private Transform _insideStandingPoint;
    public Transform InsideStandingPoint => _insideStandingPoint;

    [SerializeField]
    private Transform _outsideStandingPoint;
    public Transform OutsideStandingPoint => _outsideStandingPoint;

    Animator _animator;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void OpenInside()
    {
        _animator.SetTrigger("OpenInside");
    }

    public void CloseInside()
    {
        _animator.SetTrigger("CloseInside");
    }

    public void OpenOutside()
    {
        _animator.SetTrigger("OpenOutside");
    }

    public void CloseOutside()
    {
        _animator.SetTrigger("CloseOutside");
    }
}
