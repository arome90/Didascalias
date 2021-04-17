using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
namespace ClassRoomVR
{
    public class MenuManager : MonoBehaviour
    {
        //public GameObject PlayAndQuit_Obj; //Objeto "MenuPrincipal"
        public GameObject PackTriade_Obj;  //Objeto "PackMenu"
        public GameObject packB; //Objetos PackI
        public GameObject packBVR; //Objetos PackIpara VR
        public GameObject PackTriade_ObjVR;
        public GameObject cameraRig;


        private void Start()
        {
            if (GameManager.Instance.getVR()) cameraRig.SetActive(true);
            else cameraRig.SetActive(false);
        }
        public void PlayButton() { 
            //PlayAndQuit_Obj.SetActive(false);
            //PackTriade_Obj.SetActive(true);

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

        //El unico problema es que los 3 botones de pack se crean con scala 10000
        //Y en la z -99999 , por lo demas va perfect
        private void createPackButton(int i) {

            //diferenciar entre VR y no VR

            GameObject a;
            if (!GameManager.Instance.getVR())
            {
                a = Instantiate(packB);
                a.name = i.ToString();
                a.GetComponent<Button>().onClick.AddListener(delegate { PackButton(i); });
                TextMeshProUGUI text = a.GetComponentInChildren<TextMeshProUGUI>();
                string index = (i + 1).ToString();
                text.text = "Pack" + index;
                a.transform.localScale = new Vector3(1, 1, 1);
                a.gameObject.transform.localScale = new Vector3(1, 1, 1);
                a.transform.parent = PackTriade_Obj.transform;
            }
            else
            {
                a = Instantiate(packBVR);
                a.transform.parent = PackTriade_ObjVR.transform;
                a.gameObject.transform.position = new Vector3(0, 0, 0);
                a.name = i.ToString();
                a.GetComponent<Button>().onClick.AddListener(delegate { PackButton(i); });
                TextMeshProUGUI text = a.GetComponentInChildren<TextMeshProUGUI>();
                string index = (i + 1).ToString();
                text.text = "Pack" + index;
                a.transform.localScale = new Vector3(1, 1, 1);
                a.gameObject.transform.localScale = new Vector3(1, 1, 1);
              
            }
        }
    }
}