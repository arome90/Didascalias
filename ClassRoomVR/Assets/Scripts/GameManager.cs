using Meta.WitAi;
using Meta.WitAi.Data.Configuration;
using Oculus.Voice;
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
        [Serializable]
        public struct LanguageOption
        {
            public string name;
            public WitConfiguration witApp;
        }
        [SerializeField] private LanguageOption[] _languages;

        private WitConfiguration _currentLanguage;

        public WitConfiguration Language { get { return _currentLanguage; } }

        public LanguageOption[] Languages { get { return _languages; } }

        /// <summary>
        /// Evento llamado cuando se cambia el idioma de la aplicación
        /// </summary>
        public UnityEvent OnLanguageChanged;

        public bool IsPause;
        private bool _connectionLost = false;

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
            if (!InitializeSingleton()) return;
            IsPause = false;
        }

        private void Start()
        {
            _currentLanguage = _languages[0].witApp;
            // Esto significa que crea una instancia en caso de ser nulo.
            OnLanguageChanged ??= new UnityEvent();
        }

        private bool InitializeSingleton()
        {
            if (Instance != null)
            {
                Destroy(this);
                return false;
            }
            Instance = this;
            InitializeData();
            DontDestroyOnLoad(this);
            return true;
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
            WsClient.Instance.Disconnect();
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

        public void ChangeLanguage(WitConfiguration language)
        {
            _currentLanguage = language;
            OnLanguageChanged.Invoke();
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
            if (IsPause && ConnectionIsAvailable() && _connectionLost)
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
            if (lostConnection)
            {
                ToggleLoadingBar(true);
            }
            SceneTransitionManager.Singleton.FadeScreen.Fade(0.0f, 0.8f, Pause);
            _connectionLost = lostConnection;
        }

        private void Pause()
        {
            if (IsPause) return;
            StopTime();
            IsPause = true;
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
            Time.timeScale = 1.0f;
            IsPause = false;
            SceneTransitionManager.Singleton.FadeScreen.Fade(0.8f, 0.0f);
        }

        public void StopTime()
        {
            Debug.Log("TIME HAS STOPPED!!");
            Time.timeScale = 0.0f;
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