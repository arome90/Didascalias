using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class MenuEditStructure : MonoBehaviour
    {
        ClassSettings settings;

        [SerializeField] Button volver;
        [SerializeField] Button aplicar;
        [SerializeField] GameObject nextScreen;
        [SerializeField] Structure circular;
        [SerializeField] Structure fila;

        Structure struActual;
        // Use this for initialization
        void Awake()
        {
            settings = GameManager.Instance.Settings;
            volver.onClick.AddListener(GoNextScreen);
            
        }


        void GoNextScreen()
        {
            nextScreen.SetActive(true);
            gameObject.SetActive(false);
        }



        //Momentaneo
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O)) 
            {
                GameManager.Instance.LoadMainScene();
            }
        }
        private void OnEnable()
        {
            bool circu = settings.StructureClass == StructureMode.Circular
                || settings.StructureClass == StructureMode.U;
            circular.gameObject.SetActive(circu);
            fila.gameObject.SetActive(!circu);
            struActual = circu ? circular : fila;
            aplicar.onClick.AddListener(GoNextScreen);
        }

    }
}