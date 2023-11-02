using Meta.WitAi;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ClassRoomVR
{
    public class GameManager : MonoBehaviour
    {
        public bool IsPause { get; set; } = false;

        private GameObject player;
        private DataSystem savedData;
        private ScenePackage chosenPackage;
        private ClassManager classManager;
        private VoiceActivation voiceActivation;

        [SerializeField] private ClassSettings currentSettings;
        [SerializeField] private ClassSettings[] availableSettings;
        [SerializeField] private ScenePackage[] availablePackages;
        [SerializeField] private ClassInfo currentClassInfo;
        [SerializeField] private bool isUsingVRHardware = false;
        [SerializeField] private bool isAutoSavingEnabled = false;
        [SerializeField] private bool saveAudio = false;
        [SerializeField] private bool firebaseAnalytics = true;
        [SerializeField] private bool unityAnalytics = true;

        public static GameManager Instance { get; private set; }

     

        private void Awake()
        {
            InitializeSingleton();
            InitializeData();
        }

        private void InitializeSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                chosenPackage = availablePackages[0];
                InitializeData();
                InitializeAnalytics();
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        private void InitializeData()
        {
            if (isAutoSavingEnabled)
            {
                savedData = SaveSystem.LoadData();
                InitializeSettings();
            }
            else
            {
                savedData = new DataSystem();
            }
        }

        private void InitializeSettings()
        {
            if (savedData != null)
            {
                currentSettings.NumStudents = savedData.NumStudents;
                currentSettings.Age = savedData.Age;
                currentSettings.StructureMode = savedData.StructureMode;
                currentSettings.Mode = savedData.Mode;
                currentSettings.NumMen = savedData.MenCount;
                currentSettings.NumWomen = savedData.WomenCount;
            }
        }

        private void InitializeAnalytics()
        {
            AnalyticsManager.Start(firebaseAnalytics, unityAnalytics);
        }

        public ScenePackage GetChosenPackage() => chosenPackage;
        public ClassInfo GetCurrentClassInfo() => currentClassInfo;
        public bool IsUsingVRHardware() => isUsingVRHardware;

        public void LoadMainMenu()
        {
            //SceneManager.LoadScene("Menu");
            SceneTransitionManager.singleton.GoToSceneAsync(0);

        }

        public void LoadMainScene()
        {
            //SceneManager.LoadScene("Class_GameScene");
            SceneTransitionManager.singleton.GoToSceneAsync(1);
            //AnalyticsManager.CustomEvent("Gritar");


        }
        public void SetChosenPackage(int index) => chosenPackage = availablePackages[index];
        public ScenePackage GetPackageAtIndex(int index) => availablePackages[index];
        public void SetPlayer(GameObject playerObj) => player = playerObj;
        public GameObject GetPlayer() => player;
        public ClassManager GetClassManager() => classManager;
        public void SetClassManager(ClassManager classMgr) => classManager = classMgr;
        public VoiceActivation GetVoiceActivation() => voiceActivation;
        public void SetVoiceActivation(VoiceActivation voice) => voiceActivation = voice;
        public void SetCurrentSettings(int index) => currentSettings = availableSettings[index];

        private void InitData()
        {
            if (savedData != null)
            {
                currentSettings.NumStudents = savedData.NumStudents;
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
            savedData.NumStudents = currentSettings.NumStudents;
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
            Debug.Log(UnityEngine.XR.XRDevice.refreshRate);
            //UnityEngine.XR.InputTracking.trackingLost
           // if(xe)
        }

        public bool GetSaveAudio() => saveAudio;

        private void OnApplicationPause(bool pause)
        {
            Debug.Log(pause + " Puasa");
           
        }

      
        private void OnApplicationFocus(bool focus)
        {
            Debug.Log(focus + " focus");
           
        }

       
        void Pause() 
        {
            AudioListener.pause = true;
            Time.timeScale = 0f;
        }

        void Continue()
        {
            AudioListener.pause = false;
            Time.timeScale = 1f;
        }


    }
}