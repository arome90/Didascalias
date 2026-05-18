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
        Tooltip("Bot�n que decrementar� el valor")] Button _decreaseButton;
    [SerializeField,
        Tooltip("Bot�n que incrementar� el valor")] Button _increaseButton;
    [SerializeField] TextMeshProUGUI _valueText;

    [Header("Parameters")]
    [SerializeField,
        Tooltip("Valor por el que se incrementar� el valor al darle al b�t�n de incremento")] 
    float _increment;
    [SerializeField, 
        Tooltip("Valor (no negativo) por el que decrementar� el valor al pulsar el bot�n de decremento")]
    float _decrement;

    [SerializeField,
        Tooltip("Si queremos que valor num�rico se interprete como int en el campo de texto")] 
    bool _valueIsInt = true;

    [SerializeField,
        Tooltip("Valor m�nimo")] float _minValue;
    [SerializeField,
        Tooltip("Valor m�ximo")] float _maxValue;
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
    /// Referencia p�blica al valor modificado por el componente
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

    public (Button decrease, Button increase) FindButtonsFromChildren(Button currentDecrease = null, Button currentIncrease = null)
    {
        Button decrease = currentDecrease;
        Button increase = currentIncrease;
        uint next = 0;
        var buttons = GetComponentsInChildren<Button>();
        if (decrease == null)
        {
            Didascalia.Utils.Error.DebugbreakFailUnless(
                next < buttons.Length, "ValueUI: No se han encontrado suficientes botones hijos en el objeto: " + name, this
            );
            decrease = buttons[next];
        }
        ++next;
        if (increase == null)
        {
            Didascalia.Utils.Error.DebugbreakFailUnless(
                next < buttons.Length, "ValueUI: No se han encontrado suficientes botones hijos en el objeto: " + name, this
            );
            increase = buttons[next];
        }
        ++next;
        _ = next;

        _decreaseButton = decrease;
        _increaseButton = increase;

        return (decrease, increase);
    }
    public void AssignButtonsFromChildren() {
        var (decrease, increase) = FindButtonsFromChildren(_decreaseButton, _increaseButton);
        _decreaseButton = decrease;
        _increaseButton = increase;
    }

    public TextMeshProUGUI FindValueTextFromChildren(TextMeshProUGUI currentValueText = null) {
        TextMeshProUGUI text = currentValueText;
        if (text != null) {
            return text;
        }

        foreach (Transform child in transform) {
            if (child.TryGetComponent(out TextMeshProUGUI found)) {
                text = found;
            }
        }
        Didascalia.Utils.Error.DebugbreakFailUnless(
            text != null, "ValueUI: No se ha encontrado un TextMeshProUGUI hijo en el objeto: " + name, this
        );
        return text;
    }
    public void AssignValueTextFromChildren() {
        _valueText = FindValueTextFromChildren(_valueText);
    }
    public void AssignButtonListeners() {
        Didascalia.Utils.Error.DebugbreakFailUnless(
            _decreaseButton != null && _increaseButton != null, "ValueUI: No se han asignado los botones de incremento y decremento en el objeto: " + name, this
        );
        _decreaseButton.onClick.RemoveListener(DecreaseValue);
        _increaseButton.onClick.RemoveListener(IncreaseValue);

        _decreaseButton.onClick.AddListener(DecreaseValue);
        _increaseButton.onClick.AddListener(IncreaseValue);
    }
    private void OnValidate()
    {
        AssignButtonsFromChildren();

        // Para buscar el texto, asumimos que encontramos el valor
        // en el �ltimo hijo de este objeto, ya que el primero de todos
        // normalmente suele ser el 'Label' que describe qu� es el valor
        AssignValueTextFromChildren();

        // Como no sabemos si el bot�n cuenta ya con el Listener adecuado,
        // probamos a quitarlo de ambos botones, para posteriormente volver a aplicarlo
        // No tengo una soluci�n mejor... Ayuda
        AssignButtonListeners();

        // No queremos llamar al evento porque eso supondr�a
        // una llamada en un OnValidate, que no est� permitida
        SetValueNotRaiseEvent(_initialValue);
    }

    private void Awake() {
        AssignButtonsFromChildren();

        // Para buscar el texto, asumimos que encontramos el valor
        // en el �ltimo hijo de este objeto, ya que el primero de todos
        // normalmente suele ser el 'Label' que describe qu� es el valor
        AssignValueTextFromChildren();

        // Como no sabemos si el bot�n cuenta ya con el Listener adecuado,
        // probamos a quitarlo de ambos botones, para posteriormente volver a aplicarlo
        // No tengo una soluci�n mejor... Ayuda
        AssignButtonListeners();

        // No queremos llamar al evento porque eso supondr�a
        // una llamada en un OnValidate, que no est� permitida
        SetValueNotRaiseEvent(_initialValue);        
    }

    private void OnEnable() {
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
    /// El valor se asigna al argumento recibido en este funci�n, acotado 
    /// por el valor m�nimo y m�ximo del componente
    /// </summary>
    /// <param name="newValue"></param>
    public void SetValue(float newValue)
    {
        float previous = _value;
        SetValueNotRaiseEvent(newValue);

        _onValueChanged?.Invoke(_value, previous);
    }

    /// <summary>
    /// Settea el valor dado, acotado entre el m�nimo y el m�ximo.
    /// Tambi�n actualiza el texto y lo muestra como entero en caso necesario
    /// 
    /// Esta funci�n solo settea el valor, pero no llama al evento
    /// OnValueChanged. Existe para ser utilizado en OnValidate, Awake o
    /// CheckConsistency, ya que ah� no se pueden llamar a eventos
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
    /// Actualiza el texto seg�n el nuevo valor.
    /// En caso de ser un valor de tipo 'int', se redondea al m�s cercano
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
