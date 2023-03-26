using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderSetting : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI text;

    public void ChangeText() 
    {
        text.text = ((int)slider.value).ToString();        
    }

    public void UpdateSli(Slider other)
    {
        slider.maxValue = 30 - other.value;
        Debug.Log(slider.maxValue);

    }

}
