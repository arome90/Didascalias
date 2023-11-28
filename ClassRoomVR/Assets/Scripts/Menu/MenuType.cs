using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClassRoomVR
{
    public class MenuType : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown structureDropdown;
        [SerializeField] private Button editButton;
        [SerializeField] private GameObject editText;

        private void Start()
        {
            structureDropdown.onValueChanged.AddListener(ChangeSetting);
            editButton.onClick.AddListener(GoEditScreen);
        }
        private void OnEnable()
        {
            structureDropdown.options.Clear();
            SetOptions(GameManager.Instance.GetAvailableSettings());
            structureDropdown.SetValueWithoutNotify(GameManager.Instance.GetIndexCurrentSettings());

        }
        private void GoEditScreen()
        {
            GameManager.Instance.SetCurrentSettings(0);
            MenuTransition.Instance.GoNextScreen();
        }
    

        private void ChangeSetting(int value)
        {
            editText.SetActive((value == 0));
            if(GameManager.Instance.GetScene() == 2) 
            {
                value += structureDropdown.options.Count ;
            }
            GameManager.Instance.SetCurrentSettings(value);
        }

        public void SetOptions(ClassSettings[] classes)
        {
            List<string> dropdownOptions = new List<string>();

            int lenght = classes.Length / 2;
            int i = GameManager.Instance.GetScene() == 1 ? 0 : lenght;
            int l = lenght + i;
            for (; i < l; i++) 
            {
                dropdownOptions.Add(classes[i].name);

            }
            structureDropdown.AddOptions(dropdownOptions);
        }
    }
}
