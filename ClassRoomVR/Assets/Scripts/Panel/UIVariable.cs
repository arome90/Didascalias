using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace ClassRoomVR
{
    public class UIVariable : MonoBehaviour
    {
        [SerializeField] Slider statusSlider;
        [SerializeField] TMPro.TextMeshProUGUI text;
        [SerializeField] TMPro.TextMeshProUGUI percetext;
        public void SetStatus(float f)
        {
            statusSlider.value = Mathf.Clamp(f, 0f, 100f);
        }
        public void SetStatusText(string t)
        {
            text.text = t;
        }

        public void SetStatusPerText(string t)
        {
            percetext.text = t;
        }
    }
}