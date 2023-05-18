using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClassRoomVR
{
    public class MenuType : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown structureDropdown;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject backScreen;
        [SerializeField] private GameObject editScreen;
        [SerializeField] private TMP_Text startText;

        private void Start()
        {
            structureDropdown.onValueChanged.AddListener(ChangeSetting);
            startButton.onClick.AddListener(GoNextScreen);
            backButton.onClick.AddListener(GoBackScreen);
            SetOptions(GameManager.Instance.GetAvailableSettings());
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

        private void GoBackScreen()
        {
            backScreen.SetActive(true);
            gameObject.SetActive(false);
        }

        private void GoNextScreen()
        {
            if (structureDropdown.value == 0)
            {
                editScreen.SetActive(true);
                gameObject.SetActive(false);
            }
            else
            {
                GameManager.Instance.LoadMainScene();
            }
        }

        private void ChangeSetting(int value)
        {
            GameManager.Instance.SetCurrentSettings(value);
            if (value == 0)
            {
                startText.text = "Editar";
            }
            else
            {
                startText.text = "Empezar";
            }
        }
    }
}
