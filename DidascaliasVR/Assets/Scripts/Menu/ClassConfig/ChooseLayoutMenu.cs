using UnityEngine;

/// <summary>
/// Encargada de activar/desactivar las opciones correspondientes
/// al tipo de layout seleccionado para su configuración de filas, columnas,
/// radio, etc.
/// </summary>
public class ChooseLayoutMenu : MonoBehaviour
{
    ClassSettings _settings;

    [SerializeField]
    GameObject _squareConfigurationMenu = null;

    [SerializeField]
    GameObject _circularConfigurationMenu = null;

    [SerializeField]
    GameObject _uConfigurationMenu = null;

    private void OnEnable()
    {
        _settings = ClassManager.Instance.Settings;
        Refresh();
    }

    public void Refresh()
    {
        _squareConfigurationMenu.SetActive(_settings.ClassShape == ClassSettings.Shape.Square);
        _circularConfigurationMenu.SetActive(_settings.ClassShape == ClassSettings.Shape.Circular);
        _uConfigurationMenu.SetActive(_settings.ClassShape == ClassSettings.Shape.U);
    }
}
