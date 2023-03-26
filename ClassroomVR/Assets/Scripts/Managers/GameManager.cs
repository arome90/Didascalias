using Meta.WitAi;
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
                // if (_sceneManager != null) if (chosenPack == null) chosenPack = _packeges[0];
                chosenPack = _packeges[0];
                DontDestroyOnLoad(this);
            }
            else
            {
               // Instance._sceneManager = _sceneManager;
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
        }

        public ScenePackage GetScenePackage(int i)
        {
            return _packeges[i];
        }
        public string getpackName(int i)
        {
            return _packeges[i].name;
        }

        public int getNPacks() { return _packeges.Length; }



        public void SetUIManager(UIManager ui)
        {
            //Presentamos el UIManager al GameManager
            UIManager = ui;
        }

        public void SetPlayer(GameObject pl)
        {
            //Presentamos el Player al GameManager
            player = pl;
        }

        public GameObject GetPlayer()
        {
            return player;      
        }
        public ClassManager GetClassManager()
        {
            return classManager;
        }
        public void setClass(ClassManager cl)
        {
            classManager = cl;
        }


        public VoiceActivation GetVoiceActivation()
        {
            return voice;
        }
        public void SetVoiceActivation(VoiceActivation voi)
        {
            //Presentamos  Wit al GameManager
            voice = voi;
        }

        /// ATRIBUTOS ESTATICOS ///
        public static GameManager Instance;

        /// ATRIBUTOS NO ESTATICOS ///
        //public MySceneManager _sceneManager;
        [SerializeField] ScenePackage[] _packeges;
        [SerializeField] ClassInfo _classInfo;
        [SerializeField] bool VRHardware = false;
        [SerializeField] VoiceActivation voice;
        private ScenePackage chosenPack;
        private Wit wit;
        UIManager UIManager;
        GameObject player;
        [SerializeField] ClassManager classManager;

    }
}