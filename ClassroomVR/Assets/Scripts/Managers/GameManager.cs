using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utilities.Extensions;

namespace ClassRoomVR
{
    public class GameManager : MonoBehaviour
    {
        public bool IsPause { get; set; }

        private DataSystem savedData;
        private ScenePackage chosenPackage;
        private VoiceActivation voice;
        private ReconnectUI loadingBar;

        [SerializeField] private ClassSettings currentSettings;
        [SerializeField] private ClassSettings[] availableSettings;
        [SerializeField] private ScenePackage[] availablePackages;
        [SerializeField] private ClassInfo currentClassInfo;
        [SerializeField] private bool isUsingVRHardware = false;
        [SerializeField] private bool isAutoSavingEnabled = false;
        [SerializeField] private bool saveAudio = false;
       // [SerializeField] private bool firebaseAnalytics = true;
       // [SerializeField] private bool unityAnalytics = true;
        private int indexScene;
        private int indexCurrentSett;
        
        public static GameManager Instance { get; private set; }

     

        private void Awake()
        {
            InitializeSingleton();
            InitializeData();
            IsPause = false;
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
           // AnalyticsManager.Start(firebaseAnalytics, unityAnalytics);
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

            ServerMessage.SendInfoInitial();
            SceneTransitionManager.singleton.GoToSceneAsync(indexScene);
            //AnalyticsManager.CustomEvent("Gritar");

        }
        public void SetChosenPackage(int index) => chosenPackage = availablePackages[index];
        public ScenePackage GetPackageAtIndex(int index) => availablePackages[index];

        public void SetScene(int i) => indexScene = i;
        public int GetScene() => indexScene;
        public void SetCurrentSettings(int index) 
        {
            currentSettings = availableSettings[index];
            indexCurrentSett = index;
        }

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

        public int GetIndexCurrentSettings()
        {
            return indexCurrentSett;
        }
        public ClassSettings[] GetAvailableSettings()
        {
            return availableSettings;
        }


        public bool GetSaveAudio() => saveAudio;
        private void Update()
        {
            if (IsPause && Application.internetReachability != NetworkReachability.NotReachable)
            {

                Debug.Log("vuelve la conexion");
                voice.Activate();
                WsClient.Instance.StartConnection();
                loadingBar.SetActive(false);
                Continue();

            }
        }
        public void Pause(bool lostConnection)
        {
            Debug.Log("pause");
            if (!IsPause)
            {
                IsPause = true;
             
                if (lostConnection)
                {                    
                    if (loadingBar != null)
                    {
                        loadingBar.SetActive(true);
                    }
                    
                }
                //Time.timeScale = 0f;
                AudioListener.pause = true;
            }
        }
      
        void WaitConnection()
        {
            Debug.Log("que pasa");
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                Debug.Log("vuelve la coneccion");
                voice.Activate();
                WsClient.Instance.StartConnection();
                loadingBar.SetActive(false);
                Continue();
            }
            else
            {
                Invoke(nameof(WaitConnection), 3.0f);
            }
        }




        //void WaitConnection()
        //{
        //    Debug.Log("espera la coneccion");
        //    if (Application.internetReachability != NetworkReachability.NotReachable)
        //    {
        //        voice.Activate();
        //        Debug.Log("vpñoo");
        //        WsClient.Instance.StartConnection();
        //        ServerMessage.SendInfoInitial();
        //        loadingBar.SetActive(false);
        //        Continue();
        //        //CancelInvoke(nameof(WaitConnection));
        //    }
        //}



        public void Continue()
        {
            AudioListener.pause = false;
          //  Time.timeScale = 1f;
            IsPause = false;
        }

        public void SetVoiceExperience(VoiceActivation voice) 
        {
            this.voice = voice;
        }
        public void SetLoadingBar(ReconnectUI bar) 
        {
            loadingBar = bar;
        }
    }
}