using Meta.WitAi;
using Meta.WitAi.Data.Configuration;
using Oculus.Voice;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Generated.PropertyProviders;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
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
        [SerializeField] private LanguageOption[] _witAppsForLanguages;

        private WitConfiguration _currentWitApp;

        public WitConfiguration Language { get { return _currentWitApp; } }

        /// <summary>
        /// DEBE SEGUIR EL MISMO ORDEN QUE EN LAS OPCIONES DE LOCALIZACIÓN
        /// ESPAÑOL - 0
        /// PORTUGUÉS br - 1
        /// ...
        /// </summary>
        public LanguageOption[] witAppsForLanguages { get { return _witAppsForLanguages; } }

        /// <summary>
        /// Evento llamado cuando se cambia el idioma de la aplicación
        /// </summary>
        public UnityEvent OnLanguageChanged;

        public bool IsPause = false;
        private bool _connectionLost = false;
        private bool _wsConnection = false;
        private bool _wsTryingToConnect = false;

        private DataSystem savedData;
        private VoiceActivation voice;
        private GameObject loadingBar;
        private GameObject loadingBarTxt;
        private GameObject wsTxt;

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
            _currentWitApp = _witAppsForLanguages[0].witApp;
            // Esto significa que crea una instancia en caso de ser nulo.
            OnLanguageChanged ??= new UnityEvent();

            int localeID = PlayerPrefs.GetInt("LocaleKey", 0);
            ChangeLanguage(localeID);
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
            SceneTransitionManager.Singleton.GoToSceneAsync(1);
        }

        public void LoadTutorial()
        {
            currentSettings = availableSettings[availableSettings.Length - 1];
            SceneTransitionManager.Singleton.GoToSceneAsync(3);
        }

        public void LoadMainScene()
        {
            // StartCoroutine(ServerMessage.SendInfoInitial());
            SceneTransitionManager.Singleton.GoToSceneAsync(2);
        }

        public void SetCurrentSettings(int index)
        {
            currentSettings = availableSettings[index];
            indexCurrentSett = index;
        }

        private void OnApplicationQuit()
        {
            LoadManager.DestroyInstance();
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

        private bool canChange = true;
        public void ChangeLanguage(WitConfiguration newWitApp)
        {
            if (!canChange) return;
            _currentWitApp = newWitApp;
            int i = 0;
            while (i < _witAppsForLanguages.Length && _currentWitApp != _witAppsForLanguages[i].witApp) ++i;
            StartCoroutine(SetLocale(i));
            OnLanguageChanged.Invoke();
        }

        public void ChangeLanguage(int localeID)
        {
            if (!canChange) return;
            _currentWitApp = _witAppsForLanguages[localeID].witApp;
            StartCoroutine(SetLocale(localeID));
            OnLanguageChanged.Invoke();
        }

        private IEnumerator SetLocale(int localeID)
        {
            canChange = false;
            yield return LocalizationSettings.InitializationOperation;
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
            Didascalia_LocalizationManager.ChangeLanguage(localeID);
            canChange = true;
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
            /*if (IsPause && ConnectionIsAvailable() && _connectionLost)
            {
                HandleReconnection();
            }
            else */
            if (!_connectionLost && !ConnectionIsAvailable())
            {
                _connectionLost = true;
            }
            else if (_connectionLost && ConnectionIsAvailable())
            {
                _connectionLost = false;
            }
            /////Pause(_connectionLost);
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
            Continue(true);
        }

        public void Pause(bool lostConnection, bool fade)
        {
            //if (lostConnection)
            //{
            //    Debug.Log("ToggleLoadingBar");
            //    ToggleLoadingBar(true);
            //}
            if (fade) { 
                try
                {
                    SceneTransitionManager.Singleton.FadeScreen.Fade(0.0f, 0.8f, Pause);
                }
                catch(Exception ex)
                {
                    Debug.LogError("Fade Failed: " + ex.Message);
                }
            }
            else
            {
                Pause();
            }

            IsPause = true;
            _connectionLost = lostConnection;
            /////ToggleLoadingBar(_connectionLost);
        }

        private void Pause()
        {
            if (IsPause) return;
            StopTime();
            IsPause = true;
            Debug.Log("Game Paused!!");
        }

        private void ToggleLoadingBar(bool visible)
        {
            if (loadingBar != null)
            {
                loadingBar.SetActive(visible);
                loadingBar.GetComponent<Canvas>().enabled = visible;
                loadingBarTxt.SetActive(visible);
                loadingBarTxt.GetComponent<Canvas>().enabled = visible;
            }
        }

        public void WaitConnection()
        {
            if (loadingBar.GetComponent<Canvas>().enabled && Application.internetReachability != NetworkReachability.NotReachable)
            {
                Debug.Log("vuelve la coneccion");
                voice.Activate();
                WsClient.Instance.StartConnection();
                loadingBar.SetActive(false);
                Continue(true);
            }
            else
            {
                Invoke(nameof(WaitConnection), 3.0f);
            }
        }

        public void Continue(bool fade)
        {
            //AudioListener.pause = false;
            Time.timeScale = 1.0f;
            IsPause = false;
            if (fade) {
                try
                {
                    SceneTransitionManager.Singleton.FadeScreen.Fade(0.8f, 0.0f);
                }
                catch (Exception ex) { 
                    Debug.LogError("Fade Failed: " + ex.Message);
                }
            }

            Debug.Log("Continued!");
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
        public void SetLoadingBar(GameObject bar)
        {
            loadingBar = bar;
            loadingBar.SetActive(false);
        }
        public void SetLoadingTxt(GameObject txt)
        {
            loadingBarTxt = txt;
            loadingBarTxt.SetActive(false);
        }
        public void SetWsTxt(GameObject txt)
        {
            wsTxt = txt;
            wsTxt.SetActive(false);
        }
        public void ChangeWsTxt(string s)
        {
            Debug.Log("Cambiar texto: " + s);
            if (wsTxt == null)
            {
                Debug.Log("wsTxt es nulo");
                return;
            }
            Debug.Log("wsTxt no es nulo");
            wsTxt.GetComponent<WsTxt>().SetText(s);
        }
        public void SetWsConnection(bool connection)
        {
            _wsConnection = connection;
        }
        public bool GetWsConnection()
        {
            return _wsConnection;
        }
        public void SetWsTryingToConnect(bool connection)
        {
            //if(!_wsTryingToConnect && connection)//si no t estabas intentando conectar y empiezas empezamos el temporizador

            _wsTryingToConnect = connection;
        }
        public bool GetWsTryingToConnect()
        {
            return _wsTryingToConnect;
        }
    }
}