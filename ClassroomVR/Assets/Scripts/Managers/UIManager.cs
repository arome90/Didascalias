using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class UIManager : MonoBehaviour
    {
        //-----Publics-------
        [Header("Objectos Normales")]
        // Contexto
        [SerializeField] Text textContexto;
        // Caminos
        [SerializeField] GameObject ObjectOpciones;
        [SerializeField] List<Text> OptionsTexts;
        // Final
        [SerializeField] GameObject panelFinal;
        [SerializeField] Text textFinal;
        [SerializeField] List<GameObject> finalButtons;

        //------Metodos---------

        void Start()
        {
            GameManager.Instance.SetUIManager(this);

        }


        //-------------------------------PANEL CONTEXTO----------------------------------
        // Inicia el panel del contexto dandole el texto que precisa
        public void panelContexto(string s)
        {
            textContexto.text = s;
        }

        // Activar/desactivar contexto
        public void setContext(bool b)
        {
            textContexto.transform.parent.gameObject.SetActive(b);
        }

        //-------------------------------PANEL FINAL----------------------------------
        // Metodo que muestra en el panel final como ha ido el desarrollo de la escena
        public void initEndPanel(string feedBackText, bool goodPath, float resolveTime)
        {
            // Info general de la escena

            int t1 = (int)(resolveTime * 100);
            float t2 = (float)t1 / 100;

            CSVSerializer.saveData("Tiempo en resolver la situación: " + t2 + " segundos\n");

            string endText = "";
            if (goodPath) endText += "ESTRATEGIA APROPIADA:\n";
            else endText += "ESTRATEGIA POCO APROPIADA:\n";

            endText += "\n" + feedBackText;


            ObjectOpciones.SetActive(false);
            textFinal.text = endText;
            panelFinal.SetActive(true);

        }

        public void changeEndPanel(string t)
        {
            textFinal.text = t;
        }

        public void showEndButtons()
        {

            foreach (GameObject g in finalButtons)
            {
                g.SetActive(true);
            }

        }

        //-------------------------------PANEL OPCIONES------------------------------
        // Activar/desactivar opciones
        public void setOptions(bool b)
        {

            ObjectOpciones.SetActive(b);

        }

        //Idea con un botor activar o desctivar las opciones queda mucho mejor para este formato
        public void initPanelOpciones(string s, string alumnsName)
        {
            string tex = s.Replace("alum", alumnsName);


            foreach (Text t in OptionsTexts)
            {
                if (t.text == "vacio")
                {
                    t.gameObject.SetActive(true);
                    t.text = tex;
                    break;
                }
            }

        } // end iniPanelOpciones
    }
}