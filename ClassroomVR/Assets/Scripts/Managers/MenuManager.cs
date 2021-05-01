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
        [Header("Objectos Vr")]
        public GameObject canvasVR;
        public GameObject cameraRig;
        public GameObject UIHelpers;
        public GameObject PackTriade_ObjVR;
        public GameObject packBVR; //Objetos PackIpara VR

        [Header("Objectos Normales")]
        public GameObject MainCamera;
        public GameObject canvasNormal;
        public GameObject PackTriade_Obj;  //Objeto "PackMenu"
        public GameObject packB; //Objetos PackI

        private void Start()
        {
            if (GameManager.Instance.getVR()) enableCanvasVR();
            else enableCanvasNormal();
        }
        public void enableCanvasVR()
        {
            canvasVR.SetActive(true);
            cameraRig.SetActive(true);
            UIHelpers.SetActive(true);
            MainCamera.SetActive(false);
            canvasNormal.SetActive(false);
        }
        public void enableCanvasNormal()
        {
            MainCamera.SetActive(true);
            canvasNormal.SetActive(true);
            canvasVR.SetActive(false);
            cameraRig.SetActive(false);
            UIHelpers.SetActive(false);      
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
                a = Instantiate(packB, PackTriade_Obj.transform);
                a.name = i.ToString();
                a.GetComponent<Button>().onClick.AddListener(delegate { PackButton(i); });
                TextMeshProUGUI text = a.GetComponentInChildren<TextMeshProUGUI>();
                string index = (i + 1).ToString();
                text.text = GameManager.Instance.getpackName(i);
                a.transform.localScale = new Vector3(1, 1, 1);
                a.gameObject.transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                a = Instantiate(packBVR, PackTriade_ObjVR.transform);
                a.gameObject.transform.position = new Vector3(0, 0, 0);
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
}