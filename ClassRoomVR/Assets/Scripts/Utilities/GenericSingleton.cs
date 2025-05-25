using UnityEngine;


/// <summary>
/// Clase base para implementar el patrón Singleton genérico en componentes MonoBehaviour.
/// Garantiza que solo exista una instancia de T durante toda la ejecución.
/// </summary>
/// <typeparam name="T">Tipo del componente Singleton (debe heredar de MonoBehaviour).</typeparam>
public class GenericSingleton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    private static readonly object lockObject = new object();

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<T>();

                        if (instance == null)
                        {
                            GameObject obj = new GameObject { name = typeof(T).Name };
                            instance = obj.AddComponent<T>();
                        }
                    }
                }
            }
            return instance;
        }
    }
    /// <summary>
    /// Asegura que solo exista una instancia. Si ya existe, destruye la nueva.
    /// Marca el objeto como DontDestroyOnLoad para mantenerlo entre escenas.
    /// </summary>
    public virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}