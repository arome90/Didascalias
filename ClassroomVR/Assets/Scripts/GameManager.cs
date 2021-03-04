using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClassRoomVR
{
    public class GameManager : MonoBehaviour
    {

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public ScenePackage getPack()
        {
            return chosenPack;
        }
        public void makeChoice(int i)
        {
            switch (i)
            {
                case 1:
                    chosenPack = pack1;
                    break;
                case 2:
                    chosenPack = pack2;
                    break;
                case 3:
                    chosenPack = pack3;
                    break;
                default: break;
            }
            LoadTestScene();
        }

        public void LoadChoosingScene()
        {
            SceneManager.LoadScene("ChoosePackScene");
        }
        public void LoadTestScene()
        {
            SceneManager.LoadScene("EscenaDePruebas");
        }

        /// ATRIBUTOS ESTATICOS ///
        public static GameManager Instance { get; private set; }

        /// ATRIBUTOS NO ESTATICOS ///


        public ScenePackage pack1;
        public ScenePackage pack2;
        public ScenePackage pack3;


        private ScenePackage chosenPack;


    }
}