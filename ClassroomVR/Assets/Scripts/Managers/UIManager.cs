using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class UIManager : MonoBehaviour
    {
        //-----Publics-------
        // Canvas normal
        [Header("Objectos Normales")]
        public GameObject canvasNormal;
        public GameObject eventSystem;
        // Contexto
        public Text textContexto;
        // Caminos
        public GameObject ObjectOpciones;
        public List<Text> OptionsTexts;
        // Final
        public GameObject panelFinal;
        public Text textFinal;
        public List<GameObject> finalButtons;

        // CanvasVR
        [Header("Objectos Vr")]
        public GameObject canvasVR;
        public GameObject UIHelpers;
        public Text textContextoVR;
        public GameObject ObjectOpcionesVR;
        public List<Text> OptionsTextsVR;
        public GameObject panelFinalVR;
        public Text textFinalVR;
        public List<GameObject> finalButtonsVR;

        //------Metodos---------
        // Start is called before the first frame update
        void Start()
        {
            if (GameManager.Instance.getVR()) enableCanvasVR();
            else enableCanvasNormal();
        }

        // Inicia para VR o normal
        public void enableCanvasVR()
        {
            canvasVR.SetActive(true);
            canvasNormal.SetActive(false);
            UIHelpers.SetActive(true);
            eventSystem.SetActive(false);
        }
        public void enableCanvasNormal()
        {
            canvasVR.SetActive(false);
            canvasNormal.SetActive(true);
            UIHelpers.SetActive(false);
            eventSystem.SetActive(true);
        }

        //-------------------------------PANEL CONTEXTO----------------------------------
        // Inicia el panel del contexto dandole el texto que precisa
        public void panelContexto(string s) {
            if (GameManager.Instance.getVR())
            {
                textContextoVR.text = s;
            }
            else
            {
                textContexto.text = s;
            }
        }

        // Activar/desactivar contexto
        public void setContext(bool b)
        {
            if (GameManager.Instance.getVR())
            {
                textContextoVR.transform.parent.gameObject.SetActive(b);
            }
            else
            {
                textContexto.transform.parent.gameObject.SetActive(b);
            }
        }

        //-------------------------------PANEL FINAL----------------------------------
        // Metodo que muestra en el panel final como ha ido el desarrollo de la escena
        public void initEndPanel(string feedBackText, bool goodPath, float resolveTime) {
            // Info general de la escena

            int t1 = (int)(resolveTime * 100);
            float t2 = (float)t1 / 100;

            string endText = "Tiempo en resolver la situación: " + t2 + " segundos\n";

            if (goodPath) endText += "Has tomado el camino correcto!\n";
            else endText += "La decisión tomada NO ha sido la mas adecuada\n";

            endText += "\n" + feedBackText;

            if (GameManager.Instance.getVR())
            {
                ObjectOpcionesVR.SetActive(false);
                textFinalVR.text = endText;
                panelFinalVR.SetActive(true);
            }
            else
            {
                ObjectOpciones.SetActive(false);
                textFinal.text = endText;
                panelFinal.SetActive(true);
            }
        }

        public void changeEndPanel(string t)
        {
            if (GameManager.Instance.getVR())
            {
                textFinalVR.text = t;
            }
            else
            {
                textFinal.text = t;
            }
        }

        public void showEndButtons()
        {
            if (GameManager.Instance.getVR())
            {
                foreach(GameObject g in finalButtonsVR)
                {
                    g.SetActive(true);
                }
            }
            else
            {
                foreach (GameObject g in finalButtons)
                {
                    g.SetActive(true);
                }
            }
        }

        //-------------------------------PANEL OPCIONES------------------------------
        // Activar/desactivar opciones
        public void setOptions(bool b)
        {
            if (GameManager.Instance.getVR())
            {
                ObjectOpcionesVR.SetActive(b);
            }
            else
            {
                ObjectOpciones.SetActive(b);
            }
        }

        //Idea con un botor activar o desctivar las opciones queda mucho mejor para este formato
        public void initPanelOpciones(string s, string alumnsName)
        {
            string tex = s.Replace("alum", alumnsName);

            if (GameManager.Instance.getVR()) {
                foreach (Text t in OptionsTextsVR) {
                    if (t.text == "vacio") {
                        t.gameObject.SetActive(true);
                        t.text = tex;
                        break;
                    }
                }
            }
            else
            {
                foreach (Text t in OptionsTexts) {
                    if (t.text == "vacio") {
                        t.gameObject.SetActive(true);
                        t.text = tex;
                        break;
                    }
                }
            }
        } // end iniPanelOpciones
    }
}