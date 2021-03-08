using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
namespace ClassRoomVR
{
    public class MenuManager : MonoBehaviour
    {
        public GameObject PlayAndQuit_Obj; //Objeto "MenuPrincipal"
        public GameObject PackTriade_Obj;  //Objeto "PackMenu"
        public GameObject packB; //Objetos PackI


        public void PlayButton() { 
            PlayAndQuit_Obj.SetActive(false);
            PackTriade_Obj.SetActive(true);

            for (int i = 0; i < GameManager.Instance.getNPacks(); i++) {
                createPackButton(i);
            }
        }
        public void QuitButton() {
            Application.Quit();
        }
        public void PackButton(int i) {
            GameManager.Instance.makeChoice(i);
        }

        private void createPackButton(int i) {
            GameObject a = Instantiate(packB);
            a.name = i.ToString();
            a.GetComponent<Button>().onClick.AddListener(delegate { PackButton(i); });
            TextMeshProUGUI text = a.GetComponentInChildren<TextMeshProUGUI>();
            string index = (i+1).ToString();
            text.text = "Pack" + index;
            a.transform.parent = PackTriade_Obj.transform;
        }
    }
}