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
            SetOptions(GameManager.Instance.GetAvailableSettings());
        }
        private void GoEditScreen()
        {
            GameManager.Instance.SetCurrentSettings(0);
            MenuTransition.Instance.GoNextScreen();
        }
    

        private void ChangeSetting(int value)
        {
            GameManager.Instance.SetCurrentSettings(value);
            editText.SetActive((value == 0));
        }

        public void SetOptions(ClassSettings[] classes)
        {
            List<string> dropdownOptions = new List<string>();
            foreach (ClassSettings cl in classes)
            {
                dropdownOptions.Add(cl.name);
            }

            structureDropdown.AddOptions(dropdownOptions);
        }
    }
}
