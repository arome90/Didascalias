using UnityEngine;

public class Book : MonoBehaviour
{
    private Animator _animator;

    private bool _opened = false;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        if(!_animator)
        {
            Debug.LogError("No se ha encontrado el animador asociado al objeto: " + gameObject.name);
        }
        //if (_openedBook == null)
        //{
        //    _openedBook = transform.GetChild(0).gameObject;
        //    if(_openedBook == null) Debug.LogError("No se puede abrir el libro, no tiene GameObject asociado");
        //}
    }

    private void Open(bool active)
    {
        _opened = active;
        _animator.SetBool("Opened", active);
    }

    public void Open()
    {
        Open(true);
    }

    public void Close()
    {
        Open(false);
    }
}
