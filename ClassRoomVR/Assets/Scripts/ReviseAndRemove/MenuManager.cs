using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
namespace ClassRoomVR
{
    public class MenuManager : MonoBehaviour
    {
        //[SerializeField] GameObject PackTriade_Obj;  //Objeto "PackMenu"
        //[SerializeField] GameObject packB; //Objetos PackI
        [SerializeField] GameObject positionPlayer;
        [SerializeField] GameObject player;
        Vector3 playerInit;


        private void Start()
        {
            if (player != null)
            {
                playerInit = player.transform.position;
            }
        }
        public void PlayButton()
        {
            if (player != null) 
            {
                player.transform.position = positionPlayer.transform.position + Vector3.down/2.0f;
                player.transform.rotation = Quaternion.Euler(Vector3.zero);

            }
            //for (int i = 0; i < GameManager.Instance.getNPacks(); i++)
            //{
            //    createPackButton(i);
            //}
        }

        public void ReturnButton()
        {
            if (player != null)
            {

                player.transform.position = playerInit;
                player.transform.rotation = Quaternion.Euler(Vector3.down*90.0f);

            }
        }
       
        public void QuitButton()
        {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }

        /*public void GuiaButton()
        {
            Application.Guia();
        }*/

        //public void PackButton(int i)
        //{
        //    GameManager.Instance.makeChoice(i);
        //}

        ////El unico problema es que los 3 botones de pack se crean con scala 10000
        ////Y en la z -99999 , por lo demas va perfect
        //private void createPackButton(int i)
        //{
            
        //    GameObject a = Instantiate(packB, PackTriade_Obj.transform);
        //    a.name = i.ToString();
        //    a.GetComponent<Button>().onClick.AddListener(delegate { PackButton(i); });
        //    TextMeshProUGUI text = a.GetComponentInChildren<TextMeshProUGUI>();
        //    string index = (i + 1).ToString();
        //    text.text = GameManager.Instance.getpackName(i);
        //    a.transform.localScale = new Vector3(1, 1, 1);
        //    a.gameObject.transform.localScale = new Vector3(1, 1, 1);


        //}
    }
}