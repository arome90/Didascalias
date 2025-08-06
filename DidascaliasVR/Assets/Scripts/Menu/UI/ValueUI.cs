using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Interfaz de usuario que permite incrementar y decrementar un valor de tipo float
/// Necesita de dos botones y un texto para mostrar el valor
/// </summary>
public class ValueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField,
        Tooltip("Botón que decrementará el valor")] Button _decreaseButton;
    [SerializeField,
        Tooltip("Botón que incrementará el valor")] Button _increaseButton;
    [SerializeField] TextMeshProUGUI _valueText;

    [Header("Parameters")]
    [SerializeField,
        Tooltip("Valor por el que se incrementará el valor al darle al bótón de incremento")] 
    float _increment;
    [SerializeField, 
        Tooltip("Valor (no negativo) por el que decrementará el valor al pulsar el botón de decremento")]
    float _decrement;

    [SerializeField,
        Tooltip("Si queremos que valor numérico se interprete como int en el campo de texto")] 
    bool _valueIsInt = true;

    [SerializeField,
        Tooltip("Valor mínimo")] float _minValue;
    [SerializeField,
        Tooltip("Valor máximo")] float _maxValue;
    [SerializeField, 
        Tooltip("Valor inicial, comprendido entre Min Value y Max Value")] 
    float _initialValue;
    /// <summary>
    /// Valor inicial, comprendido entre Min Value y Max Value
    /// </summary>
    public float InitialValue { get { return _initialValue; } set { _initialValue = value; } }

    [SerializeField,
        Tooltip("Evento llamado cuando se settea el valor. Se incluyen como argumentos el valor nuevo y el anterior")] 
    UnityEvent<float, float> _onValueChanged;

    /// <summary>
    /// Getter del evento OnValueChanged:
    /// Evento llamado cuando se settea el valor. Se incluyen como argumentos el valor nuevo y el anterior
    /// </summary>
    public UnityEvent<float, float> OnValueChanged { get { return _onValueChanged; } }

    /// <summary>
    /// El valor que queremos modificar
    /// </summary>
    float _value = 0.0f;

    /// <summary>
    /// Referencia pública al valor modificado por el componente
    /// </summary>
    public float Value { get { return _value; } }

    public void SetMaxValue(float max)
    {
        _maxValue = max;
        SetValue(_value);
    }
    public float GetMaxValue() { return _maxValue; }

    public void SetMinValue(float min)
    {
        _minValue = min;
        SetValue(_value);
    }
    public float GetMinValue() { return _minValue; }


    private void OnValidate()
    {
        bool decreaseNotFound = _decreaseButton == null;
        bool increaseNotFound = _increaseButton == null;

        // Para encontrar los diferentes botones, estamos asumiendo
        // que son los dos primeros botones que tenemos como hijos de este objeto
        if (decreaseNotFound || increaseNotFound) 
        {
            var buttons = GetComponentsInChildren<Button>();
            if(decreaseNotFound) _decreaseButton = buttons[0];
            if(increaseNotFound) _increaseButton = buttons[1];
        }

        // Para buscar el texto, asumimos que encontramos el valor
        // en el último hijo de este objeto, ya que el primero de todos
        // normalmente suele ser el 'Label' que describe qué es el valor
        if(_valueText == null)
        {
            TextMeshProUGUI text;
            for(int i = 0; i < transform.childCount; ++i)
            {
                text = transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                if(text != null)
                {
                    _valueText = text;
                }
            }
        }

        // Como no sabemos si el botón cuenta ya con el Listener adecuado,
        // probamos a quitarlo de ambos botones, para posteriormente volver a aplicarlo
        // No tengo una solución mejor... Ayuda
        _decreaseButton.onClick.RemoveListener(DecreaseValue);
        _increaseButton.onClick.RemoveListener(IncreaseValue);

        _decreaseButton.onClick.AddListener(DecreaseValue);
        _increaseButton.onClick.AddListener(IncreaseValue);

        // No queremos llamar al evento porque eso supondría
        // una llamada en un OnValidate, que no está permitida
        SetValueNotRaiseEvent(_initialValue);
    }

    private void OnEnable()
    {
        SetValue(_value);
    }

    /// <summary>
    /// Incrementa el valor por Increment
    /// </summary>
    public void IncreaseValue()
    {
        ChangeValue(_increment);
    }

    /// <summary>
    /// Decrementa el valor por Decrement
    /// </summary>
    public void DecreaseValue()
    {
        ChangeValue(-_decrement);
    }

    /// <summary>
    /// Cambia el valor actual por la suma del actual + el argumento recibido
    /// </summary>
    /// <param name="value"> Nuevo valor por el que modificar el valor actuar mediante una suma </param>
    public void ChangeValue(float value)
    {
        SetValue(value + _value);
    }

    /// <summary>
    /// El valor se asigna al argumento recibido en este función, acotado 
    /// por el valor mínimo y máximo del componente
    /// </summary>
    /// <param name="newValue"></param>
    public void SetValue(float newValue)
    {
        float previous = _value;
        SetValueNotRaiseEvent(newValue);

        _onValueChanged?.Invoke(_value, previous);
    }

    /// <summary>
    /// Settea el valor dado, acotado entre el mínimo y el máximo.
    /// También actualiza el texto y lo muestra como entero en caso necesario
    /// 
    /// Esta función solo settea el valor, pero no llama al evento
    /// OnValueChanged. Existe para ser utilizado en OnValidate, Awake o
    /// CheckConsistency, ya que ahí no se pueden llamar a eventos
    /// </summary>
    /// <param name="newValue"> El nuevo valor </param>
    public void SetValueNotRaiseEvent(float newValue)
    {
        _value = Mathf.Clamp(newValue, _minValue, _maxValue);
        
        if (_valueIsInt) _value = Mathf.Round(_value);

        UpdateTextValue();
        UpdateUI();
    }

    /// <summary>
    /// Actualiza el texto según el nuevo valor.
    /// En caso de ser un valor de tipo 'int', se redondea al más cercano
    /// </summary>
    private void UpdateTextValue()
    {
        if (_valueText == null)
            Debug.Log("Value Text is not assgined in game object: " + name);
        else if (_valueIsInt) _valueText.text = ((int)_value).ToString();
        else _valueText.text = (Mathf.Round(_value * 100) / 100.0f).ToString();
    }

    /// <summary>
    /// Actualiza la interfaz de usuario, apagando los botones
    /// que no se puedan utilizar y cambiando el color del texto
    /// </summary>
    private void UpdateUI()
    {
        bool isMaxValue = _value == _maxValue;
        bool isMinvalue = _value == _minValue;
        _increaseButton.interactable = !isMaxValue;
        _decreaseButton.interactable = !isMinvalue;

        if (isMaxValue) _valueText.color = Color.green;
        else if (isMinvalue) _valueText.color = Color.red;
        else _valueText.color = Color.white;
    }
}
