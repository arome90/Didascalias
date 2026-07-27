using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private bool _isFrontDoor = true;

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

        if (_isFrontDoor) ClassManager.Instance.SetFrontDoor(this);
        else ClassManager.Instance.SetBackDoor(this);
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
