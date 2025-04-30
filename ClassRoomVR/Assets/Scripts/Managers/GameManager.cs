using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityRenderer;
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

        private DataSystem savedData;
        private VoiceActivation voice;
        private GameObject loadingBar;
        private GameObject loadingBarTxt;

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
            Debug.Log("GM loading MM");
            WsClient.Instance.Disconnect();
            _connectionLost = true;
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
            
            //se ha perdido la conexión
            if (!_connectionLost && !ConnectionIsAvailable())
            {
                SessionAvailable(false);
            }
            /*
            else if (_connectionLost && ConnectionIsAvailable())
            {
                SessionAvailable(true);
            }*/
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

        public void LostSessionConnection()
        {
            Debug.Log("Lost Session Connection");
            _connectionLost = true;
            WsClient.Instance.Disconnect();
            if (SceneManager.GetActiveScene().name != "Menu")
                LoadMainMenu();
        }

        public void SessionAvailable(bool created)
        {
            if (created)
            {
                Debug.Log("Session Created");
                _connectionLost = false;
                ToggleLoadingBar(false);
            }
            else
            {
                Debug.Log("No Session Created");
                _connectionLost = true;
                ToggleLoadingBar(true);
            }
        }

        public void Pause(bool fade)
        {
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
        }

        private void Pause()
        {
            if (IsPause) return;
            StopTime();
            IsPause = true;
            ClassManager.Instance.SetPause(IsPause);
            Debug.Log("Game Paused!!");
        }

        private void ToggleLoadingBar(bool visible)
        {
            if (loadingBarTxt != null)
            {
                Debug.Log("setActive = " + visible);
                Debug.Log(loadingBarTxt.activeSelf);
                loadingBarTxt.SetActive(visible);
                //Debug.Log("canvasEnabled = " + visible);
                //loadingBarTxt.GetComponent<Canvas>().enabled = visible;
            }
            else
                Debug.Log("Loading bar txt is null");

            Debug.Log("Loading toggle" + visible);
            if (loadingBar != null)
            {
                Debug.Log("setActive = " + visible);
                Debug.Log(loadingBar.activeSelf);
                loadingBar.SetActive(visible);
                //Debug.Log("canvasEnabled = " + visible);
                //loadingBar.GetComponent<Canvas>().enabled = visible;
            }
            else
                Debug.Log("Loading bar is null");
        }

        public void Continue(bool fade)
        {
            //AudioListener.pause = false;
            Time.timeScale = 1.0f;
            IsPause = false;
            ClassManager.Instance.SetPause(IsPause);
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

        public bool GetConnection()
        {
            return !_connectionLost;
        }


        #region DEPRICATED
        /*
        public void SetWsTryingToConnect(bool connection)
        {
            _wsTryingToConnect = connection;
        }

        public bool GetWsTryingToConnect()
        {
            return _wsTryingToConnect;
        }
        
        //Session unavailable cumple esta función
        public void SetConnection(bool connection)
        {
            _connectionLost = !connection;
        }


        //No lo utilizamos, sabrememos que se conecta al tener exito en la creacion del ws
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
                
        //No lo utilizamos, si se pierde la conexion cierra la sesion directamente
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

        */
        #endregion
    }
}