using UnityEngine;

/// <summary>
/// Encargado de gestionar que haya un número correcto de estudiantes, acorde al máximo
/// establecido por las class_settings (conseguidas del GameManager
/// </summary>
public class StudentPopulationHandlerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField,
        Tooltip("ValueUI que controla el valor de cuántos chicos hay en la clase")] 
    ValueUI _boysHandler;
    [SerializeField,
        Tooltip("ValueUI que controla el valor de cuántas chicas hay en la clase")] 
    ValueUI _girlsHandler;

    ClassSettings _settings;

    /// <summary>
    /// Avisamos de que faltan referencias en caso de que falten y
    /// añadimos las funciones para ajustar los valores a los ValueUI
    /// </summary>
    private void OnEnable()
    {
        if (!_boysHandler || !_girlsHandler)
        {
            Debug.LogError("Missing References in " + name);
        }

        _settings = ClassManager.Instance.Settings;

        _boysHandler?.SetMaxValue(_settings.MaxStudents);
        _boysHandler?.SetMinValue(0);

        _girlsHandler?.SetMaxValue(_settings.MaxStudents);
        _boysHandler?.SetMinValue(0);

        _boysHandler?.OnValueChanged.AddListener(AdjustBoyValues);
        _girlsHandler?.OnValueChanged.AddListener(AdjustGirlValues);
    }

    /// <summary>
    /// Quitamos los listeners de los ValueUI para ajustar sus valores
    /// </summary>
    private void OnDisable()
    {
        _boysHandler.OnValueChanged.RemoveListener(AdjustBoyValues);
        _girlsHandler.OnValueChanged.RemoveListener(AdjustGirlValues);
    }

    void AdjustBoyValues(float current, float last)
    {
        AdjustValues(current, last, _boysHandler, _girlsHandler);
    }

    void AdjustGirlValues(float current, float last)
    {
        AdjustValues(current, last, _girlsHandler, _boysHandler);
    }

    /// <summary>
    /// Ajustamos el valor de la UI correspondiente en caso de que nos pasemos
    /// del número máximo de estudiantes por clase
    /// </summary>
    /// <param name="current"> valor nuevo </param>
    /// <param name="last"> antiguo valor (no utilizado) </param>
    /// <param name="ui"> valueUI que se ha cambiado </param>
    /// <param name="otherUI"> la otra valueUI </param>
    void AdjustValues(float current, float last, ValueUI ui, ValueUI otherUI)
    {
        float otherValue = otherUI.Value;

        if(current > _settings.MaxStudents)
        {
            current = _settings.MaxStudents;
            ui.SetValue(current);
            
        }
        // aquí hacemos un else if porque al hacer el SetValue
        // se va a volver a llamar al evento OnValueChanged
        // que llamará a esta función de nuevo.
        else if (current + otherValue > _settings.MaxStudents)
        {
            otherUI.SetValue(_settings.MaxStudents - current);
        }
    }
}
