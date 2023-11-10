using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClassRoomVR
{
    public class MenuType : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown structureDropdown;
        //[SerializeField] private Button startButton;
        //[SerializeField] private Button backButton;
        [SerializeField] private Button editButton;
        //[SerializeField] private GameObject backScreen;
        //[SerializeField] private GameObject editScreen;
        [SerializeField] private GameObject editText;

        private void Start()
        {
            structureDropdown.onValueChanged.AddListener(ChangeSetting);
           // startButton.onClick.AddListener(GoNextScreen);
            editButton.onClick.AddListener(GoEditScreen);
            //backButton.onClick.AddListener(GoBackScreen);
            SetOptions(GameManager.Instance.GetAvailableSettings());
        }

        //private void GoBackScreen()
        //{
        //    backScreen.SetActive(true);
        //    gameObject.SetActive(false);
        //}

        //private void GoNextScreen()
        //{
        //    GameManager.Instance.LoadMainScene();
        //}

        private void GoEditScreen()
        {
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
