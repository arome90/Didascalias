using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente que activa un objeto aleatorio de una lista y desactiva los demás.
/// </summary>
public class GenerateMaterial : MonoBehaviour
{
    /// <summary>
    /// Lista de objetos de tipo "cases" que pueden ser activados aleatoriamente.
    /// </summary>
    [SerializeField] private List<GameObject> _cases;

    /// <summary>
    /// Lista de objetos de tipo "books" que pueden ser activados aleatoriamente.
    /// </summary>
    [SerializeField] private List<GameObject> _books;

    private void Start()
    {
        // Activa un objeto aleatorio de la lista de casos y de la lista de libros.
        ActivateRandomObject(_cases);
        ActivateRandomObject(_books);
    }

    /// <summary>
    /// Activa un objeto aleatorio de la lista proporcionada y desactiva los demás.
    /// </summary>
    /// <param name="objects">Lista de objetos a considerar para la activación.</param>
    private void ActivateRandomObject(List<GameObject> objects)
    {
        if (objects == null || objects.Count == 0)
        {
            Debug.LogWarning("La lista de objetos está vacía o es nula.");
            return;
        }

        // Selecciona un índice aleatorio dentro del rango de la lista.
        int randomIndex = Random.Range(0, objects.Count);

        // Itera sobre la lista y activa el objeto en el índice aleatorio, desactivando los demás.
        for (int i = 0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            if (obj == null)
            {
                Debug.LogWarning("Se encontró un objeto nulo en la lista.");
                continue;
            }

            obj.SetActive(i == randomIndex);
        }
    }
}
