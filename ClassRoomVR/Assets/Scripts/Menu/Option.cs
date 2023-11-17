using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI optionName;
    [SerializeField] private TextMeshProUGUI optionValue;
    [SerializeField] private TextMeshProUGUI optionMaxValue;

    [SerializeField] private Button addButton;
    [SerializeField] private Button subButton;

    // Variables
    [SerializeField] private float value;
    public float GetValue() => value;

    [SerializeField] private float minValue;
    [SerializeField] private float maxValue;
    [SerializeField] private float step;

    [HideInInspector] public UnityEvent<float> onValueChanged;

    private void Start()
    {
        optionValue.text = value.ToString("0.#");
        addButton.onClick.AddListener(Add);
        subButton.onClick.AddListener(Sub);
    }

    private void Add()
    {
        Debug.Log("click");
        value += step;
        if (value > maxValue)
            value = maxValue;
        else
        {
            onValueChanged.Invoke(value);
            optionValue.text = value.ToString("0.#");

        }
    }

    private void Sub()
    {
        value -= step;       
        if (value < minValue)
            value = minValue;
        else
        {
            onValueChanged.Invoke(value);
            optionValue.text = value.ToString("0.#");
        }
    }


    public void SetMax(float v)
    {
        maxValue = v;
        optionMaxValue.text = "(Max: " + v.ToString() + ")";
    }

    public void SetMin(float v)
    {
        minValue = v;
        
    }
   
    public void SetValue(float v)
    {
        value = Mathf.Clamp(v, minValue, maxValue);
        optionValue.text = value.ToString("0.#");
    }


    public float GetMax() { return maxValue; }
    public float GetMin() { return minValue; }
}
