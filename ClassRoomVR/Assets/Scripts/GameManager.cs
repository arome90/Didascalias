using Meta.WitAi.Data.Configuration;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Settings;
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

        private int lastSettingsUsed;
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (!InitializeSingleton()) return;
            IsPause = false;
        }

        private void Start()
        {
            int language = PlayerPrefs.GetInt("Language", 0);
            ChangeLanguage(language);

            ClassSettings[] settings = availableSettings;
            int index = 0;
            foreach (var setting in settings)
            {
                if (setting.name == "Personalizado")
                {
                    break;
                }
                index++;
            }
            Instance.SetCurrentSettings(index);

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
            lastSettingsUsed = index;
            currentSettings = availableSettings[index];
            indexCurrentSett = index;
        }

        /// <summary>
        /// Se seleccionan las últimas opciones de clase usadas que no fueran la del tutorial.
        /// </summary>
        public void SetLastUsedSettings()
        {
            currentSettings = availableSettings[lastSettingsUsed];
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
            int i = 0;
            StartCoroutine(SetLocale(localeID));
            OnLanguageChanged.Invoke();
        }

        private IEnumerator SetLocale(int localeID)
        {
            canChange = false;
            PlayerPrefs.SetInt("Language", localeID);
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