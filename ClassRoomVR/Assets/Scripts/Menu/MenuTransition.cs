using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace ClassRoomVR {
    public class MenuTransition : GenericSingleton<MenuTransition>
    {

        [SerializeField] List<GameObject> menus;
        [SerializeField] Button back;
        [SerializeField] Button start;
        [SerializeField] int index;
        [SerializeField] MenuInicio menu;
        [SerializeField] GameObject player;
        private void Awake()
        {
            for (int i = 0; i < menus.Count; i++)
            {
                menus[i].SetActive(false);
            }
            menus[index].SetActive(true);
        }

        private void Start()
        {
            back.onClick.AddListener(GoBackScreen);
            start.onClick.AddListener(GoStart);
        }

        public void GoBackScreen()
        {
            menus[index].SetActive(false);
            index--;
            if (index < 0) 
            {
                index = 0;
                menu.ReturnButton();
            }
            menus[index].SetActive(true);
        }

        public void GoNextScreen() 
        {
            menus[index].SetActive(false);
            index++;
            menus[index].SetActive(true);
        }
    
        private void GoStart()
        {
            //Temporal
            if (GameManager.Instance.GetCurrentSettings().name != "Personalizado") { DeskManager.Instance.DestroyChildren(); }
            GameManager.Instance.LoadMainScene();
            start.gameObject.SetActive(false);
        }

        public void MovePizarra() 
        {
            player.transform.eulerAngles= new Vector3(0,0,0);
        }
        public void MoveClase()
        {
            player.transform.eulerAngles = new Vector3(0, 180, 0);
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