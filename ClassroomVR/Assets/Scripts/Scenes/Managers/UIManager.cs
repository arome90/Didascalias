using System;
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
        [Header("Objectos Normales")]
        public GameObject canvasNormal;
        public GameObject eventSystem;
        public Text textContexto;
        public GameObject textOpciones;
        public GameObject panelFinal;
        //vr    
        [Header("Objectos Vr")]
        public GameObject canvasVR;
        public GameObject UIHelpers;
        public Text textContextoVR;
        public GameObject textOpcionesVR;
        public GameObject panelFinalVR;

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
        public void panelContexto(string s ) {
            if (GameManager.Instance.getVR())
            {
                textContextoVR.text = s;
            }
            else
            {
                textContexto.text = s;
            }
        }
        //Cuando se activa uno se desactiva el otro//true contesto false opciones
        public void swapPanels(bool b)
        {     
            if (GameManager.Instance.getVR())
            {
                textContextoVR.transform.parent.gameObject.SetActive(b);
                textOpcionesVR.SetActive(!b);
            }
            else {
                textContexto.transform.parent.gameObject.SetActive(b);
                textOpciones.SetActive(!b);
            }
        }

        public void endPanel()
        {
            if (GameManager.Instance.getVR())
            {
                textOpcionesVR.SetActive(false);
            }
            else
            {
                textOpciones.SetActive(false);
                panelFinal.SetActive(true);
            }
        }


        public void panelOpciones(string s)
        {
            GameObject aux = new GameObject();
            Text t = aux.AddComponent<Text>();
            if (GameManager.Instance.getVR())
            {
                textOpcionesVR.SetActive(true);
                aux.transform.parent = textOpcionesVR.transform;         
            }
            else
            {
                textOpciones.SetActive(true);
                aux.transform.parent = textOpciones.transform;        
            }
            t.text = s;
            t.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            t.color = Color.black;
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