using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class UIManager : MonoBehaviour
    {
        //-----Publics-------
        //Canvas normal y CanvasVR

        public GameObject canvasVR;
        public GameObject canvasNormal;
        public GameObject eventSystem;

        //vr    
        public GameObject UIHelpers;


        //normal
      
      
        //-----privates-----
        private bool Vr;




        //------Metodos---------
        public void enableCanvasVR()
        {
            canvasVR.SetActive(true);
            canvasNormal.SetActive(false);
            Vr = true;
            UIHelpers.SetActive(true);
            eventSystem.SetActive(false);
        }
        public void enableCanvasNormal()
        {
            canvasVR.SetActive(false);
            canvasNormal.SetActive(true);
            Vr = false;
            UIHelpers.SetActive(false);
            eventSystem.SetActive(true);
        }

        // Start is called before the first frame update
        void Start()
        { 
            if (GameManager.Instance.getVR()) enableCanvasVR();
            else enableCanvasNormal();

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}