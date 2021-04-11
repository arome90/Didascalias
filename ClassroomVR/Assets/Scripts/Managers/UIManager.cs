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
        public Text textFinal;
        //vr    
        [Header("Objectos Vr")]
        public GameObject canvasVR;
        public GameObject UIHelpers;
        public Text textContextoVR;
        public GameObject textOpcionesVR;
        public GameObject panelFinalVR;
        public Text textFinalVR;

        //------Metodos---------
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

        // Inicia el panel del contexto dandole el texto que precisa
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

        // Activar/desactivar opciones
        public void setOptions(bool b)
        {     
            if (GameManager.Instance.getVR())
            {
                textOpcionesVR.SetActive(b);
            }
            else {
                textOpciones.SetActive(b);
                
            }
        }

        // Metodo que muestra en el panel final como ha ido el desarrollo de la escena
        public void endPanel(string feedBackText, bool goodPath, MotionCaptureManager.finalInfo res, float resolveTime, float talkPitch)
        {
            // Calculamos la puntuacion por la emocion detectada mas caracteristica de la escena
            int emoScrore = MotionCaptureManager.emotionValue(res);

            int goodPathScore = 0;
            if (goodPath) goodPathScore = 10;

            // Menor tiempo -> + puntuacion por resolver la situacion rapidamente
            int timeScore = 0;
            //usar el resolveTime

            // Puntuacion por el tono de voz
            int pitchScore = 0;
            // usar el talkPitch

            // Score final
            int finalScore = emoScrore + goodPathScore + timeScore + pitchScore;

            if (GameManager.Instance.getVR())
            {
                textOpcionesVR.SetActive(false);
                // TODO (panel final, texto final)
            }
            else
            {
                textOpciones.SetActive(false);
                textFinal.text = feedBackText;
                panelFinal.SetActive(true);
            }
        }

        public void panelOpciones(string s, string alumnsName)
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
            string tex = s.Replace("alum", alumnsName);
            t.text = tex;
            t.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            t.color = Color.black;
            t.fontSize = 20;
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