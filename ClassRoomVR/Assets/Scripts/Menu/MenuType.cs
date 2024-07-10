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
            structureDropdown.ClearOptions();
            SetOptions(GameManager.Instance.GetAvailableSettings());
            structureDropdown.value = GameManager.Instance.GetIndexCurrentSettings();

        }
        private void GoEditScreen()
        {
            GameManager.Instance.SetCurrentSettings(0);
            MenuTransition.Instance.GoNextScreen();
        }


        private void ChangeSetting(int value)
        {
            editText.SetActive((value == 0));
            GameManager.Instance.SetCurrentSettings(value);
        }

        public void SetOptions(ClassSettings[] classes)
        {
            List<string> dropdownOptions = new List<string>();
            //-1 por el tutorial
            for (int i = 0; i < classes.Length-1; i++)
            {
                dropdownOptions.Add(classes[i].name);

            }
            structureDropdown.AddOptions(dropdownOptions);
        }
    }
}
