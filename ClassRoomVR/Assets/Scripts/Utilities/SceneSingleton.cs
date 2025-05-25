using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton genérico para componentes MonoBehaviour que deben ser únicos en una escena,
/// pero pueden ser distintos entre cambios de escena (no persiste entre escenas).
/// </summary>
/// <typeparam name="T">Tipo del componente que implementa el Singleton.</typeparam>
public class SceneSingleton<T> : MonoBehaviour where T : Component
{
    private static T instance;
    private static readonly object lockObject = new object();

    /// <summary>
    /// Propiedad para acceder a la instancia Singleton de la escena.
    /// Si no existe, la busca o la crea.
    /// </summary>
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
    /// Asigna la instancia si aún no existe, o destruye duplicados si aparecen en la misma escena.
    /// </summary>
    public virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Cuando se destruye este objeto, limpia la referencia de la instancia.
    /// Así, al cargar una nueva escena, se podrá crear una nueva instancia.
    /// </summary>
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}