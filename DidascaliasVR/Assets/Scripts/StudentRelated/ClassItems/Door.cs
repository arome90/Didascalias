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

    bool _isOpen = false;
    public bool IsOpen => _isOpen;

    public void SetOpen(bool open) => _isOpen = open;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();

        if (_isFrontDoor) ClassManager.Instance.SetFrontDoor(this);
        else ClassManager.Instance.SetBackDoor(this);
    }

    public void OpenInside()
    {
        if (!_isOpen) _animator.SetTrigger("OpenInside");
    }

    public void CloseInside()
    {
        if (_isOpen) _animator.SetTrigger("CloseInside");
    }

    public void OpenOutside()
    {
        if (!_isOpen) _animator.SetTrigger("OpenOutside");
    }

    public void CloseOutside()
    {
        if (_isOpen) _animator.SetTrigger("CloseOutside");
    }
}
