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
                //Instance._sceneManager = _sceneManager;
                if (_sceneManager != null) if (chosenPack == null) chosenPack = _packeges[0];
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Instance._sceneManager = _sceneManager;
                DestroyImmediate(gameObject);
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

        public void makeChoice(int i)
        {
            chosenPack = _packeges[i];
            LoadMainScene();
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene("Menu");
        }
        public void LoadMainScene()
        {
            SceneManager.LoadScene("Class_GameScene");
        }

        public int getNPacks() { return _packeges.Length; }

        /// ATRIBUTOS ESTATICOS ///
        public static GameManager Instance { get; private set; }

        /// ATRIBUTOS NO ESTATICOS ///
        public MySceneManager _sceneManager;
        public ScenePackage[] _packeges;
        public ClassInfo _classInfo;
        public bool VRHardware;


        private ScenePackage chosenPack;
    }
}