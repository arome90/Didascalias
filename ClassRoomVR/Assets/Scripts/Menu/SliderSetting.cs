using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderSetting : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;

    public void ChangeText()
    {
        text.text = ((int)slider.value).ToString();
    }

    public void UpdateSlider(Slider other)
    {
        slider.maxValue = 30 - other.value;
        Debug.Log(slider.maxValue);
    }
}
