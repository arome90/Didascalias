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
    [SerializeField] private double value;
    [SerializeField] private double minValue;
    [SerializeField] private double maxValue;
    [SerializeField] private double step;

    [HideInInspector] public UnityEvent<double> onValueChanged;

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
            onValueChanged.Invoke(value);
            optionValue.text = value.ToString("0.##");

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
            optionValue.text = value.ToString("0.##");
        }
    }

    //void SetText()
    //{ optionValue.text = value.ToString("0.##"); }

    public void SetMax(double v)
    {
        maxValue = v;
        
    }

    public void SetMin(double v)
    {
        minValue = v;
        
    }
   
    public void SetValue(double v)
    {
        value = v;
        if (value < minValue)
            value = minValue;
        else if (value > maxValue)
            value = maxValue;
        optionValue.text = value.ToString("0.##");
    }


    public double GetMax() { return maxValue; }
}
