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
                if (save)
                {
                    data = SaveSystem.LoadData();
                    InitData();
                }
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



        private void InitData()
        {
            
            if (data != null)
            {
                settings.NumStu = data.numStu;
                settings.Edad = data.edad;
                settings.StructureClass = data.structureClass;
                settings.Mode = data.mode;
                //settings.Students = data.students;
                settings.men = data.men;
                settings.women = data.women;
            }
            else
            {
                data = new DataSystem();
            }

        }
        private void OnApplicationQuit()
        {
            if (save)
            {
                SaveState();
            }
        }
        public void SaveState()
        {
            
            data.numStu = settings.NumStu;
            data.edad = settings.Edad;
            data.structureClass = settings.StructureClass;
            data.mode = settings.Mode;
            //data.students = settings.Students;
            data.men = settings.men;
            data.women = settings.women;
            SaveSystem.SaveData(data);
        }

        public void SetDeskFormation(List<bool> d) 
        {
            desks_ = d;
        }

        public List<bool> GetDeskFormation()
        {
            return desks_;
        }
        /// ATRIBUTOS ESTATICOS ///
        public static GameManager Instance;

        /// ATRIBUTOS NO ESTATICOS ///
        //public MySceneManager _sceneManager;
        [SerializeField] ScenePackage[] _packeges;
        [SerializeField] ClassInfo _classInfo;
        [SerializeField] bool VRHardware = false;
        [SerializeField] bool save = false;
        [SerializeField] VoiceActivation voice;
        private ScenePackage chosenPack;
        private Wit wit;
        UIManager UIManager;
        GameObject player;
        List<bool> desks_;

        [SerializeField] ClassManager classManager;
        [SerializeField] ClassSettings settings;
        public ClassSettings Settings => settings;

        DataSystem data;
    }
}