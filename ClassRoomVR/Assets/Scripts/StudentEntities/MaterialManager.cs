using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Componente que activa un objeto aleatorio de una lista y desactiva los demás.
/// </summary>
public class MaterialManager : MonoBehaviour
{
    // [SerializeField] private List<GameObject> _mustMaterials;

    /// <summary>
    /// Lista de objetos de tipo "cases" que pueden ser activados aleatoriamente.
    /// </summary>
    [SerializeField] private List<GameObject> _cases;

    /// <summary>
    /// Lista de objetos de tipo "books" que pueden ser activados aleatoriamente.
    /// </summary>
    [SerializeField] private List<GameObject> _books;
    [SerializeField] private List<GameObject> _notebooks;

    private Book _chosenBook;
    private Book _chosenNoteBook;
    // private GameObject _chosenCase;

    private void Start()
    {
        // Activa un objeto aleatorio de la lista de casos y de la lista de libros.
        /*_chosenCase = */ActivateRandomObject(_cases);
        _chosenBook = ActivateRandomObject(_books).GetComponent<Book>();
        _chosenNoteBook = ActivateRandomObject(_notebooks).GetComponent<Book>();

        //foreach(GameObject go in _mustMaterials)
        //{
        //    go.SetActive(true);
        //}
    }

    public Book GetBook() {  return _chosenBook; }
    public Book GetNotebook() {  return _chosenNoteBook; }

    /// <summary>
    /// Activa un objeto aleatorio de la lista proporcionada y desactiva los demás.
    /// </summary>
    /// <param name="objects">Lista de objetos a considerar para la activación.</param>
    private GameObject ActivateRandomObject(List<GameObject> objects)
    {
        GameObject ret = null;
        if (objects == null || objects.Count == 0)
        {
            Debug.LogWarning("La lista de objetos está vacía o es nula.");
            return ret;
        }

        // Selecciona un índice aleatorio dentro del rango de la lista.
        int randomIndex = Random.Range(0, objects.Count);

        // Itera sobre la lista y activa el objeto en el índice aleatorio, destruyendo los demás
        for (int i = 0; i < objects.Count; i++)
        {
            GameObject obj = objects[i];
            if (obj == null)
            {
                Debug.LogWarning("Se encontró un objeto nulo en la lista.");
                continue;
            }
            if (i == randomIndex)
            {
                obj.SetActive(true);
                ret = obj;
            }
            else Destroy(obj);
        }

        return ret;
    }
}
