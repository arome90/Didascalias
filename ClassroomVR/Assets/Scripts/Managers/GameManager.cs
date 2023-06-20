using Meta.WitAi;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClassRoomVR
{
    public class GameManager : MonoBehaviour
    {
        private GameObject player;
        private DataSystem savedData;
        private ScenePackage chosenPackage;
        //Managers
        //private UIManager uiManager;
        private ClassManager classManager;
        private VoiceActivation voiceActivation;

        [SerializeField] private ClassSettings currentSettings;
        [SerializeField] private ClassSettings[] availableSettings;
        [SerializeField] private ScenePackage[] availablePackages;
        [SerializeField] private ClassInfo currentClassInfo;
        [SerializeField] private bool isUsingVRHardware = false;
        [SerializeField] private bool isAutoSavingEnabled = false;
        [SerializeField] private bool saveAudio = false;

        public static GameManager Instance;

        [SerializeField] bool firebaseAnalytics = true;
        [SerializeField] bool unityAnalytics = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                chosenPackage = availablePackages[0];

                if (isAutoSavingEnabled)
                {
                    savedData = SaveSystem.LoadData();
                    InitData();
                }
                AnalyticsManager.Start(firebaseAnalytics, unityAnalytics);
                DontDestroyOnLoad(this);
            }
            else
            {
                DestroyImmediate(this);
            }
        }

        public ScenePackage GetChosenPackage()
        {
            return chosenPackage;
        }

        public ClassInfo GetCurrentClassInfo()
        {
            return currentClassInfo;
        }

        public bool IsUsingVRHardware()
        {
            return isUsingVRHardware;
        }

        public void LoadMainMenu()
        {
            //SceneManager.LoadScene("Menu");
            SceneTransitionManager.singleton.GoToSceneAsync(0);

        }

        public void LoadMainScene()
        {
            //SceneManager.LoadScene("Class_GameScene");
            SceneTransitionManager.singleton.GoToSceneAsync(1);

        }

        public void SetChosenPackage(int index)
        {
            chosenPackage = availablePackages[index];
        }

        public ScenePackage GetPackageAtIndex(int index)
        {
            return availablePackages[index];
        }

        //public void SetUIManager(UIManager ui)
        //{
        //    uiManager = ui;
        //}

        public void SetPlayer(GameObject playerObj)
        {
            player = playerObj;
        }

        public GameObject GetPlayer()
        {
            return player;
        }

        public ClassManager GetClassManager()
        {
            return classManager;
        }

        public void SetClassManager(ClassManager classMgr)
        {
            classManager = classMgr;
        }

        public VoiceActivation GetVoiceActivation()
        {
            return voiceActivation;
        }

        public void SetVoiceActivation(VoiceActivation voice)
        {
            voiceActivation = voice;
        }

        public void SetCurrentSettings(int index)
        {
            currentSettings = availableSettings[index];
        }

        private void InitData()
        {
            if (savedData != null)
            {
                currentSettings.NumStudents = savedData.numStudents;
                currentSettings.Age = savedData.Age;
                currentSettings.StructureMode = savedData.StructureMode;
                currentSettings.Mode = savedData.Mode;
                currentSettings.NumMen = savedData.MenCount;
                currentSettings.NumWomen = savedData.WomenCount;
            }
            else
            {
                savedData = new DataSystem();
            }
        }

        private void OnApplicationQuit()
        {
            if (isAutoSavingEnabled)
            {
                SaveState();
            }
        }
        public void SaveState()
        {
            savedData.numStudents = currentSettings.NumStudents;
            savedData.Age = currentSettings.Age;
            savedData.StructureMode = currentSettings.StructureMode;
            savedData.Mode = currentSettings.Mode;
            savedData.MenCount = currentSettings.NumMen;
            savedData.WomenCount = currentSettings.NumWomen;
            SaveSystem.SaveData(savedData);
        }

        public ClassSettings GetCurrentSettings()
        {
            return currentSettings;
        }

        public ClassSettings[] GetAvailableSettings()
        {
            return availableSettings;
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) 
            {
                Application.Quit();
            }
        }

        public bool GetSaveAudio()
        {
            return saveAudio;
        }
    }
}