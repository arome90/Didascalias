using ClassRoomVR;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Clase que maneja la interfaz de usuario de reconexión, incluyendo una barra de carga animada.
/// </summary>
public class ReconnectUI : MonoBehaviour
{
    [SerializeField] private Image _loadingBar; // Imagen de la barra de carga
    [SerializeField] private float _fillSpeed = 0.5f; // Velocidad de rotación de la barra de carga

    private void Start()
    {
        Debug.Log("Seting loading bar: " + gameObject);
        GameManager.Instance.SetLoadingBar(gameObject); // Configura la barra de carga en el GameManager
        
    }

    private void Update()
    {
        RotateLoadingBar(); // Rota la barra de carga cada frame
    }

    /// <summary>
    /// Rota la barra de carga en la dirección especificada a una velocidad determinada.
    /// </summary>
    private void RotateLoadingBar()
    {
        _loadingBar.transform.Rotate(Vector3.back * _fillSpeed * Time.deltaTime);
    }
}
