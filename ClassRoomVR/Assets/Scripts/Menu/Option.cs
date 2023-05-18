using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI optionName;
    [SerializeField] private TextMeshProUGUI optionValue;
    [SerializeField] private Button addButton;
    [SerializeField] private Button subButton;

    // Variables
    [SerializeField] private float value;
    [SerializeField] private float minValue;
    [SerializeField] private float maxValue;
    [SerializeField] private float step;

    [HideInInspector] public UnityEvent<float> onValueChanged;

    private void Start()
    {
        optionValue.text = value.ToString("0.##");
        addButton.onClick.AddListener(Add);
        subButton.onClick.AddListener(Sub);
    }

    private void Add()
    {
        value += step;
        if (value > maxValue)
            value = maxValue;
        else
        {
            optionValue.text = value.ToString("0.##");
            onValueChanged.Invoke(value);
        }
    }

    private void Sub()
    {
        value -= step;
        if (value < minValue)
            value = minValue;
        else
        {
            optionValue.text = value.ToString("0.##");
            onValueChanged.Invoke(value);
        }
    }

    public void SetMax(float value)
    {
        maxValue = value;
    }

    public void SetMin(float value)
    {
        minValue = value;
    }

    public void SetValue(float v)
    {
        value = v;
        optionValue.text = value.ToString("0.##");
    }
}
