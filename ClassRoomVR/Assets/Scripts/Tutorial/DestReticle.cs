using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maneja la destrucción del retículo cuando entra en contacto con el jugador.
/// </summary>
public class DestReticle : MonoBehaviour
{
    /// <summary>
    /// Se llama cuando otro collider entra en el trigger collider del objeto.
    /// </summary>
    /// <param name="other">Collider del objeto que ha entrado en el trigger.</param>
    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el collider tiene la etiqueta "Player"
        if (other.CompareTag("Player"))
        {
            // Destruye el objeto actual
            Destroy(gameObject);
        }
    }
}
