using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
namespace ClassRoomVR
{
    public class MenuManager : MonoBehaviour
    {
        public GameObject PackTriade_Obj;  //Objeto "PackMenu"
        public GameObject packB; //Objetos PackI

        public void PlayButton()
        {
            for (int i = 0; i < GameManager.Instance.getNPacks(); i++)
            {
                createPackButton(i);
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

        public void PackButton(int i)
        {
            GameManager.Instance.makeChoice(i);
        }

        //El unico problema es que los 3 botones de pack se crean con scala 10000
        //Y en la z -99999 , por lo demas va perfect
        private void createPackButton(int i)
        {
            GameObject a = Instantiate(packB, PackTriade_Obj.transform);
            a.name = i.ToString();
            a.GetComponent<Button>().onClick.AddListener(delegate { PackButton(i); });
            TextMeshProUGUI text = a.GetComponentInChildren<TextMeshProUGUI>();
            string index = (i + 1).ToString();
            text.text = GameManager.Instance.getpackName(i);
            a.transform.localScale = new Vector3(1, 1, 1);
            a.gameObject.transform.localScale = new Vector3(1, 1, 1);

        }
    }
}