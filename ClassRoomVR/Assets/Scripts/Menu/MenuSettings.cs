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

        private int maxStudents;
        
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
            SetOptions(typeof(StructureMode));
            structureDropdown.SetValueWithoutNotify((int)settings.StructureMode);
            SetMaxValue();
            SetMaxStudents();

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
            SetMaxValue();
            if (settings.NumMen + settings.NumWomen > maxStudents) 
            {
                settings.NumMen = maxStudents/2;
                settings.NumWomen = maxStudents/2;
                boysOption.SetValue(maxStudents / 2);
                girlsOption.SetValue(maxStudents / 2);
            }
            SetMaxStudents();


        }

        private void SetMaxValue() 
        {
            switch (settings.StructureMode)
            {
                case StructureMode.Fila:
                    maxStudents = 30;
                    break;
                case StructureMode.Circular:
                    maxStudents = 22;
                    break;
                case StructureMode.U:
                    maxStudents = 12;
                    break;

            }
        }
        private void ChangeBoys(float value)
        {
            settings.NumMen = (int)value;
            SetMaxStudents();
        }

        private void ChangeGirls(float value)
        {
            settings.NumWomen = (int)value;
            SetMaxStudents();

        }

        private void SetMaxStudents()
        {
            settings.NumStudents = settings.NumMen + settings.NumWomen;
            boysOption.SetMax(maxStudents - settings.NumWomen);
            girlsOption.SetMax(maxStudents - settings.NumMen);
        }


        public void SetOptions(Type type)
        {
            var valores = Enum.GetNames(type);
            structureDropdown.AddOptions(new List<string>(valores));
        }

    }

}
