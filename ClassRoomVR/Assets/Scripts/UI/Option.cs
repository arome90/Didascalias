using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Clase que maneja una opción con un valor numérico ajustable mediante botones.
/// </summary>
public class Option : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI optionName; // Texto que muestra el nombre de la opción
    [SerializeField] private TextMeshProUGUI optionValue; // Texto que muestra el valor actual de la opción
    [SerializeField] private TextMeshProUGUI optionMaxValue; // Texto que muestra el valor máximo de la opción

    [SerializeField] private Button addButton; // Botón para incrementar el valor
    [SerializeField] private Button subButton; // Botón para decrementar el valor

    // Variables privadas
    [SerializeField] private float _value; // Valor actual de la opción
    [SerializeField] private float _minValue; // Valor mínimo de la opción
    [SerializeField] private float _maxValue; // Valor máximo de la opción
    [SerializeField] private float _step; // Incremento o decremento del valor

    [HideInInspector] public UnityEvent<float> onValueChanged; // Evento que se dispara al cambiar el valor

    private void Start()
    {
        UpdateOptionValueText();
        addButton.onClick.AddListener(Add);
        subButton.onClick.AddListener(Subtract);
    }

    /// <summary>
    /// Incrementa el valor de la opción en el paso definido, sin exceder el valor máximo.
    /// </summary>
    private void Add()
    {
        _value = Mathf.Min(_value + _step, _maxValue);
        UpdateOptionValueText();
        onValueChanged.Invoke(_value);
    }

    /// <summary>
    /// Decrementa el valor de la opción en el paso definido, sin bajar del valor mínimo.
    /// </summary>
    private void Subtract()
    {
        _value = Mathf.Max(_value - _step, _minValue);
        UpdateOptionValueText();
        onValueChanged.Invoke(_value);
    }

    /// <summary>
    /// Establece el valor máximo de la opción y actualiza el texto que lo muestra.
    /// </summary>
    /// <param name="max">Valor máximo a establecer.</param>
    public void SetMax(float max)
    {
        _maxValue = max;
        optionMaxValue.text = $"(Max: {_maxValue})";
    }

    /// <summary>
    /// Establece el valor mínimo de la opción.
    /// </summary>
    /// <param name="min">Valor mínimo a establecer.</param>
    public void SetMin(float min)
    {
        _minValue = min;
    }

    /// <summary>
    /// Establece el valor de la opción, asegurándose de que esté dentro del rango permitido.
    /// </summary>
    /// <param name="newValue">Nuevo valor a establecer.</param>
    public void SetValue(float newValue)
    {
        _value = Mathf.Clamp(newValue, _minValue, _maxValue);
        UpdateOptionValueText();
    }

    /// <summary>
    /// Actualiza el texto que muestra el valor actual de la opción.
    /// </summary>
    private void UpdateOptionValueText()
    {
        optionValue.text = _value.ToString("0.#");
    }

    /// <summary>
    /// Obtiene el valor actual de la opción.
    /// </summary>
    /// <returns>Valor actual.</returns>
    public float GetValue() => _value;

    /// <summary>
    /// Obtiene el valor máximo de la opción.
    /// </summary>
    /// <returns>Valor máximo.</returns>
    public float GetMax() => _maxValue;

    /// <summary>
    /// Obtiene el valor mínimo de la opción.
    /// </summary>
    /// <returns>Valor mínimo.</returns>
    public float GetMin() => _minValue;
}
