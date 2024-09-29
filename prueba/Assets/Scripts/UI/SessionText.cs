using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Clase que gestiona la visualización del texto de la sesión en la interfaz de usuario.
/// </summary>
public class SessionText : MonoBehaviour
{
    private void Start()
    {
        // Obtiene el componente TextMeshProUGUI y establece su texto con el valor de la sesión actual
        GetComponent<TextMeshProUGUI>().text = WsClient.Instance.Session;
    }
}
