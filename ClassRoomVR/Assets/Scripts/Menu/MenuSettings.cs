using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClassRoomVR
{
    public class MenuSettings : MonoBehaviour
    {
        private ClassSettings settings;
        [SerializeField] private TMP_Dropdown structureDropdown;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button editDeskPositionButton;
        [SerializeField] private Option boysOption;
        [SerializeField] private Option girlsOption;
        [SerializeField] private GameObject backScreen;
        [SerializeField] private GameObject editScreen;


        private void Start()
        {
            settings = GameManager.Instance.GetCurrentSettings();
            girlsOption.SetValue(settings.NumWomen);
            boysOption.SetValue(settings.NumMen);
            structureDropdown.onValueChanged.AddListener(ChangeStructure);
            boysOption.onValueChanged.AddListener(ChangeBoys);
            girlsOption.onValueChanged.AddListener(ChangeGirls);
            startButton.onClick.AddListener(GameManager.Instance.LoadMainScene);
            backButton.onClick.AddListener(GoBackScreen);
            editDeskPositionButton.onClick.AddListener(GoEditScreen);
            structureDropdown.SetValueWithoutNotify((int)settings.StructureMode);
        }




        private void GoBackScreen()
        {
            backScreen.SetActive(true);
            gameObject.SetActive(false);
        }

        private void GoEditScreen()
        {
            editScreen.SetActive(true);
            gameObject.SetActive(false);
        }


        private void ChangeStructure(int value)
        {
            settings.StructureMode = (StructureMode)value;
        }

        private void ChangeBoys(double value)
        {
            settings.NumMen = (int)value;
            SetMaxStudents();
        }

        private void ChangeGirls(double value)
        {
            settings.NumWomen = (int)value;
            SetMaxStudents();

        }

        private void SetMaxStudents()
        {
            settings.NumStudents = settings.NumMen + settings.NumWomen;
            boysOption.SetMax(30 - settings.NumWomen);
            girlsOption.SetMax(30 - settings.NumMen);
        }
    }

}
