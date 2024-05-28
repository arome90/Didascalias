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
        [SerializeField] private Button editDeskPositionButton;
        [SerializeField] private Option boysOption;
        [SerializeField] private Option girlsOption;

        private int maxStudents;
        
        private void Start()
        {
            settings = GameManager.Instance.GetCurrentSettings();
            girlsOption.SetValue(settings.NumWomen);
            boysOption.SetValue(settings.NumMen);
            structureDropdown.onValueChanged.AddListener(ChangeStructure);
            boysOption.onValueChanged.AddListener(ChangeBoys);
            girlsOption.onValueChanged.AddListener(ChangeGirls);
            editDeskPositionButton.onClick.AddListener(GoEditScreen);
            SetOptions(typeof(StructureMode));
            structureDropdown.value=(int)settings.StructureMode;
            SetMaxValue();
            SetMaxStudents();

        }

        private void GoEditScreen()
        {
            MenuTransition.Instance.GoNextScreen();
            MenuTransition.Instance.MoveClase();
            
        }

        private void ChangeStructure(int value)
        {
            settings.StructureMode = (StructureMode)value;
            DeskManager.Instance.DestroyChildren();
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
        //cambiar TODO
        private void SetMaxValue() 
        {
            switch (settings.StructureMode)
            {
                case StructureMode.Fila:
                    maxStudents = 30;
                    break;
                case StructureMode.Circular:
                    maxStudents = 15;
                    break;
                case StructureMode.U:
                    maxStudents = 8;
                    break;
            }
        }
        private void ChangeBoys(float value)
        {
            Debug.Log("boys");
            settings.NumMen = (int)value;
            SetMaxStudents();
            DeskManager.Instance.DestroyChildren();

        }

        private void ChangeGirls(float value)
        {
            settings.NumWomen = (int)value;
            SetMaxStudents();
            DeskManager.Instance.DestroyChildren();

        }

        private void SetMaxStudents()
        {
            settings.NumStudents = settings.NumMen + settings.NumWomen;
            boysOption.SetMax(maxStudents - settings.NumWomen);
            girlsOption.SetMax(maxStudents - settings.NumMen);
            settings.NumDesks = settings.NumStudents;
        }


        public void SetOptions(Type type)
        {
            var valores = Enum.GetNames(type);
            structureDropdown.AddOptions(new List<string>(valores));
        }

    }

}
