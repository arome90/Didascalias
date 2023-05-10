using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace ClassRoomVR {
    public class MenuSettings : MonoBehaviour
    {
        ClassSettings settings;
        [SerializeField] TMP_Dropdown structure;
        [SerializeField] Button empezar;
        [SerializeField] Button volver;
        [SerializeField] Button editDeskPosition;
        [SerializeField] Option chicos;
        [SerializeField] Option chicas;
        [SerializeField] GameObject backScreen;
        [SerializeField] GameObject editScreen;


        void Start()
        {
            settings = GameManager.Instance.Settings;
            structure.onValueChanged.AddListener(ChangeEstructura);
            chicos.onValueChanged.AddListener(ChangeChicos);
            chicas.onValueChanged.AddListener(ChangeChicas);
            empezar.onClick.AddListener(GameManager.Instance.LoadMainScene);
            volver.onClick.AddListener(GoBackScreen);
            editDeskPosition.onClick.AddListener(GoEditScreen);
        }




        void GoBackScreen()
        {
            backScreen.SetActive(true);
            gameObject.SetActive(false);
        }

        void GoEditScreen()
        {
            editScreen.SetActive(true);
            gameObject.SetActive(false);
        }


        void ChangeEstructura(int value) 
        {
           settings.StructureClass = (StructureMode)value;
        }

        void ChangeChicos(float value)
        {
            settings.men = (int)value;
        }

        void ChangeChicas(float value)
        {
            settings.women = (int)value;
        }
    }

}