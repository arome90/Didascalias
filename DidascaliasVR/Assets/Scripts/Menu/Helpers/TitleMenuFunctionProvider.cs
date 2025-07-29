using UnityEngine;

/// <summary>
/// Contiene las funciones necesarias del menú inicial para los botones del mismo
/// </summary>
public class TitleMenuFunctionProvider : MonoBehaviour
{

    /// <summary>
    /// Salida de la aplicación mediante Application.Quit.
    /// No realiza ninguna otra gestión.
    /// 
    /// TODO: Añadir guardado de variables aquí
    /// </summary>
    public void ExitApplication()
    {
        Application.Quit();
    }
}
