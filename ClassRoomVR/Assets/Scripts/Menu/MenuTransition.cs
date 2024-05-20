using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities.Extensions;
using static UnityEngine.XR.Hands.XRHandSubsystemDescriptor;


namespace ClassRoomVR {
    public class MenuTransition : GenericSingleton<MenuTransition>
    {

        [SerializeField] private InputActionProperty helpAction;

        [SerializeField] List<GameObject> menus;
        [SerializeField] Button back;
        [SerializeField] Button start;
        [SerializeField] int index;
        [SerializeField] MenuInicio menu;
        [SerializeField] GameObject player;
        [SerializeField] TextMeshProUGUI textSession; // Reference to the session text

        private void Awake()
        {
            back.onClick.AddListener(GoBackScreen);
            start.onClick.AddListener(GoStart);
            WsClient.Instance.StartConnection();
            _isActive = false;
        }
        private void OnEnable()
        {
            ActiveFirstPage();
            if (helpAction != null)
            {
                helpAction.action.performed += Disenable;
            }
        }

        private void OnDisable()
        {
            if (helpAction != null)
            {
                helpAction.action.performed -= Disenable;
            }
        }

        private void ActiveFirstPage() 
        {
            for (int i = 0; i < menus.Count; i++)
            {
                menus[i].SetActive(false);
            }
            menus[index].SetActive(true);
            menus[index].transform.parent.gameObject.SetActive(true);
            ChangeScreen(0);
        }

       

        public void GoBackScreen()
        {
            menus[index].SetActive(false);
            index--;
            if (index < 0) 
            {
                index = 0;
                menus[index].transform.parent.gameObject.SetActive(false);
                menu.ReturnButton();
                return;
            }
            menus[index].SetActive(true);
            ChangeScreen(index);


        }

        public void GoNextScreen() 
        {
            menus[index].SetActive(false);
            index++;
            menus[index].SetActive(true);
            ChangeScreen(index);

        }

        private void GoStart()
        {
            start.gameObject.SetActive(false);
            back.gameObject.SetActive(false);
            menus[index].SetActive(false);
            if (GameManager.Instance.GetCurrentSettings().name != "Personalizado") { DeskManager.Instance.DestroyChildren(); }
            textSession.SetActive(true);
            textSession.text = WsClient.Instance.session;
            GameManager.Instance.LoadMainScene();
        }

        public void MovePizarra() 
        {
            player.transform.eulerAngles= new Vector3(0,0,0);
        }
        public void MoveClase()
        {
            player.transform.eulerAngles = new Vector3(0, 180, 0);
        }



        public string[] texts;
       [SerializeField] TMPro.TextMeshProUGUI text;
        bool _isActive;

        private void Disenable(InputAction.CallbackContext context)
        {
            if (!_isActive)
            {
                _isActive = true;
                text.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                _isActive = false;
                text.transform.parent.gameObject.SetActive(false);
            }
        }

        public void ChangeScreen(int i)
        {
            text.text = texts[i];
        }

        //public void MovePizarra()
        //{
        //    player.transform.position += new Vector3(0, 0, 5);
        //}
        //public void MoveClase()
        //{
        //    player.transform.position -= new Vector3(0, 0, 5);
        //}
    }
}