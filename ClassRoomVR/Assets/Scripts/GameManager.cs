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
        public bool IsPause;

        private DataSystem savedData;
        private VoiceActivation voice;
        private ReconnectUI loadingBar;

        [SerializeField] private ClassSettings currentSettings;
        [SerializeField] private ClassSettings[] availableSettings;
        [SerializeField] private ClassInfo currentClassInfo;
        [SerializeField] private bool isAutoSavingEnabled = false;
        [SerializeField] private bool saveAudio = false;
        private int indexCurrentSett;

        public static GameManager Instance { get; private set; }



        private void Awake()
        {
            InitializeSingleton();
            IsPause = false;
        }

        private void InitializeSingleton()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            InitializeData();
            DontDestroyOnLoad(this);
        }

        private void InitializeData()
        {
            if (isAutoSavingEnabled)
            {
                savedData = SaveSystem.LoadData();
                ApplySavedSettings(savedData);
            }
            else savedData = new DataSystem();
        }

        private void ApplySavedSettings(DataSystem data)
        {
            currentSettings.NumStudents = data.NumStudents;
            currentSettings.Age = data.Age;
            currentSettings.StructureMode = data.StructureMode;
            currentSettings.Mode = data.Mode;
            currentSettings.NumMen = data.MenCount;
            currentSettings.NumWomen = data.WomenCount;
        }


        public ClassInfo GetCurrentClassInfo() => currentClassInfo;
        public VoiceActivation GetVoiceActivation() => voice;

        public void LoadMainMenu()
        {
            SceneTransitionManager.Singleton.GoToSceneAsync(0);

        }

        public void LoadTutorial()
        {
            currentSettings = availableSettings[availableSettings.Length - 1];
            SceneTransitionManager.Singleton.GoToSceneAsync(2);
        }

        public void LoadMainScene()
        {
            ServerMessage.SendInfoInitial();
            SceneTransitionManager.Singleton.GoToSceneAsync(1);
        }

        public void SetCurrentSettings(int index)
        {
            currentSettings = availableSettings[index];
            indexCurrentSett = index;
        }

        private void OnApplicationQuit()
        {
            if (isAutoSavingEnabled)
            {
                UpdateSavedData();
                SaveSystem.SaveData(savedData);

            }
        }
        private void UpdateSavedData()
        {
            savedData.NumStudents = currentSettings.NumStudents;
            savedData.Age = currentSettings.Age;
            savedData.StructureMode = currentSettings.StructureMode;
            savedData.Mode = currentSettings.Mode;
            savedData.MenCount = currentSettings.NumMen;
            savedData.WomenCount = currentSettings.NumWomen;
        }

        public ClassSettings GetCurrentSettings()
        {
            return currentSettings;
        }

        public int GetIndexCurrentSettings()
        {
            return indexCurrentSett;
        }
        public ClassSettings[] GetAvailableSettings() => availableSettings;


        public bool GetSaveAudio() => saveAudio;
        private void Update()
        {
            if (IsPause && ConnectionIsAvailable() && IsLoadingBarVisible())
            {
                HandleReconnection();
            }
        }

        private bool ConnectionIsAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        private bool IsLoadingBarVisible()
        {
            return loadingBar != null && loadingBar.GetComponent<Canvas>().enabled;
        }

        private void HandleReconnection()
        {
            Debug.Log("vuelve la conexion");

            if (voice != null)
            {
                voice.Activate();
            }

            WsClient.Instance.StartConnection();
            ToggleLoadingBar(false);
            Continue();
        }
        public void Pause(bool lostConnection)
        {
            if (IsPause) return;
            IsPause = true;

            if (lostConnection)
            {
                ToggleLoadingBar(true);
            }
        }

        private void ToggleLoadingBar(bool visible)
        {
            if (loadingBar != null)
            {
                loadingBar.GetComponent<Canvas>().enabled = visible;
            }
        }

        void WaitConnection()
        {
            if (loadingBar.GetComponent<Canvas>().enabled && Application.internetReachability != NetworkReachability.NotReachable)
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

        public void Continue()
        {
            //AudioListener.pause = false;
            //Time.timeScale = 1f;
            IsPause = false;
        }

        public void SetVoiceExperience(VoiceActivation voice)
        {
            this.voice = voice;
        }
        public void SetLoadingBar(ReconnectUI bar)
        {
            loadingBar = bar;
            loadingBar.SetActive(false);
        }
    }
}