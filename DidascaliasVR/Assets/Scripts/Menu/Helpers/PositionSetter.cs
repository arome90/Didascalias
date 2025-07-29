using UnityEngine;

/// <summary>
/// Componente cuya única función es settear la posición y rotación de un objeto dado 
/// al transform del objeto que contenga este componente
/// </summary>
public class PositionSetter : MonoBehaviour
{
    /// <summary>
    /// Settea la posición y rotación del objeto recibido a la de este objeto
    /// </summary>
    /// <param name="target"> Objeto a posicionar y rotar </param>
    public void SetPlayerPositionAndRotation(Transform target) {
        target.SetPositionAndRotation(transform.position, transform.rotation);
    }
}
