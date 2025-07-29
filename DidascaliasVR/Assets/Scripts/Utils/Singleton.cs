using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance = null;
    public static T Instance { get { return _instance; } }

    public virtual void Awake()
    {
        if(_instance == null)
        {
            _instance = this as T;

            // Si tenemos un Singleton hijo de algo,
            // hacemos que ese algo persista, para no perder el
            // singleton
            Transform current = transform;
            while(current.parent != null)
            {
                current = current.parent;
            }
            DontDestroyOnLoad(current.gameObject);
        }
        else
        {
            Destroy(this);
        }
    }
}
