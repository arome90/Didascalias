using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClassRoomVR
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                //Instance._sceneManager = _sceneManager;
                if (_sceneManager != null) if (chosenPack == null) chosenPack = _packeges[0];
                DontDestroyOnLoad(this);
            }
            else
            {
                Instance._sceneManager = _sceneManager;
                DestroyImmediate(this);
            }
        }

        public ScenePackage getPack()
        {
            return chosenPack;
        }

        public ClassInfo getClass()
        {
            return _classInfo;
        }

        public bool getVR()
        {
            return VRHardware;
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene("Menu");
        }
        public void LoadMainScene()
        {
            SceneManager.LoadScene("Class_GameScene");
        }

        public void makeChoice(int i)
        {
            chosenPack = _packeges[i];
            LoadMainScene();
        }

        public string getpackName(int i)
        {
            return _packeges[i].name;
        }

        public int getNPacks() { return _packeges.Length; }

        /// ATRIBUTOS ESTATICOS ///
        public static GameManager Instance;

        /// ATRIBUTOS NO ESTATICOS ///
        public MySceneManager _sceneManager;
        public ScenePackage[] _packeges;
        public ClassInfo _classInfo;
        //Private para la build
        public  bool VRHardware = true;


        private ScenePackage chosenPack;
    }
}