using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Meta.WitAi.Data.Configuration;
using TMPro;
using static ClassRoomVR.GameManager;
using static ClassRoomVR.GameManager2;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona el menú de inicio.
    /// </summary>
    public class MenuInicio : MonoBehaviour
    {
        [SerializeField] private Button _enter; // Botón para entrar
        [SerializeField] private Button _tutorial; // Botón para el tutorial
        [SerializeField] private Button _quitButton; // Botón para salir
        [SerializeField] private Button _model1Button; //
        [SerializeField] private Button _model2Button; //
        [SerializeField] private TMP_Dropdown _languageSelector; // Selector de idioma
        [SerializeField] private TMP_Dropdown _languageSelector2; // Selector de idioma

        [SerializeField] private GameObject _nextScreen; // Pantalla siguiente
        [SerializeField] private GameObject _nextScreen2; // Pantalla siguiente
        [SerializeField] private Vector3 _playerDestination; // Destino del jugador
        [SerializeField] private Vector3 _playerInitialPosition; // Posición inicial del jugador
        [SerializeField] private Transform _player; // Transform del jugador

        Dictionary<string, WitConfiguration> _languageDictionary = new Dictionary<string, WitConfiguration>();
        Dictionary<string, WitConfiguration> _languageDictionary2 = new Dictionary<string, WitConfiguration>();

        private void Awake()
        {
            _nextScreen.SetActive(false);
        }

        private void Start()
        {

            AddButtonListeners();

            DontDestroyOnLoad(GameObject.Find("GameManager"));

        }

        /// <summary>
        /// Agrega los listeners a los botones.
        /// </summary>
        private void AddButtonListeners()
        {
            _enter.onClick.AddListener(OnOldVersionClick);
            _tutorial.onClick.AddListener(OnTutorialButtonClick);
            _quitButton.onClick.AddListener(QuitButton);
            _quitButton.onClick.AddListener(QuitButton);
            _model1Button.onClick.AddListener(OnModel1ButtonClicked);
            _model2Button.onClick.AddListener(OnModel2ButtonClicked);
            LanguageSelector();
        }

        /// <summary>
        /// Maneja el clic en el botón de entrar.
        /// </summary>
        private void OnEnterButtonClick()
        {
            DeskManager.Instance.DestroyChildren();

            DontDestroyOnLoad(GameObject.Find("DeskManager"));
            DontDestroyOnLoad(GameObject.Find("GameManager"));
            Destroy(GameObject.Find("SceneTransitionManager2"));
            Destroy(GameObject.Find("DeskManager2"));
            Destroy(GameObject.Find("GameManager2"));


            PlayButton();
            GoNextScreen();
        }
        private void OnOldVersionClick()
        {
            DeskManager2.Instance.DestroyChildren();

            DontDestroyOnLoad(GameObject.Find("DeskManager2"));
            Destroy(GameObject.Find("SceneTransitionManager"));
            Destroy(GameObject.Find("DeskManager"));
            Destroy(GameObject.Find("GameManager"));
            PlayButton();
            _nextScreen2.SetActive(true);
            gameObject.SetActive(false);
            
        }
        /// <summary>
        /// Maneja el clic en el botón de tutorial.
        /// </summary>
        private void OnTutorialButtonClick()
        {
            _tutorial.interactable = false;
            GameManager.Instance.LoadTutorial();
        }
        private void OnModel1ButtonClicked()
        {
            PlayerPrefs.SetInt("TreeModel", 1);
            OnEnterButtonClick();
        }
        private void OnModel2ButtonClicked()
        {
            PlayerPrefs.SetInt("TreeModel",2);
            OnEnterButtonClick();
        }

        /// <summary>
        /// Mueve a la pantalla siguiente.
        /// </summary>
        private void GoNextScreen()
        {
            _nextScreen.SetActive(true);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Maneja el clic en el botón de salir.
        /// </summary>
        public void QuitButton()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LanguageSelector()
        {
            _languageSelector.options.Clear();

            GameManager. LanguageOption[] languages = GameManager.Instance.witAppsForLanguages;
            for (int i = 0; i < languages.Length; ++i)
            {
                _languageDictionary.Add(languages[i].name, languages[i].witApp);

                _languageSelector.options.Add(new TMP_Dropdown.OptionData() { text = languages[i].name });
            }

            ChangeLanguage(_languageSelector);

            _languageSelector.onValueChanged.AddListener(delegate { ChangeLanguage(_languageSelector); });
            _languageSelector.RefreshShownValue();

            _languageSelector2.options.Clear();

            GameManager2.LanguageOption[] languages2 = GameManager2.Instance.witAppsForLanguages;
            for (int i = 0; i < languages2.Length; ++i)
            {
                _languageDictionary2.Add(languages2[i].name, languages2[i].witApp);

                _languageSelector2.options.Add(new TMP_Dropdown.OptionData() { text = languages2[i].name });
            }

            ChangeLanguage2(_languageSelector2);

            _languageSelector2.onValueChanged.AddListener(delegate { ChangeLanguage2(_languageSelector2); });
            _languageSelector2.RefreshShownValue();
        }

        private void ChangeLanguage(TMP_Dropdown dropdown)
        {
            string currentLanguage = dropdown.options[dropdown.value].text;

            GameManager.Instance.ChangeLanguage(_languageDictionary[currentLanguage]);

        }
        private void ChangeLanguage2(TMP_Dropdown dropdown)
        {
            string currentLanguage = dropdown.options[dropdown.value].text;

       
            GameManager2.Instance.ChangeLanguage(_languageDictionary2[currentLanguage]);

        }

        /// <summary>
        /// Mueve al jugador a la posición y rotación especificadas.
        /// </summary>
        public void PlayButton()
        {
            SetPlayerPositionAndRotation(_playerDestination, Quaternion.identity);
        }

        /// <summary>
        /// Regresa al jugador a la posición inicial y rotación especificadas.
        /// </summary>
        public void ReturnButton()
        {
            SetPlayerPositionAndRotation(_playerInitialPosition, Quaternion.Euler(Vector3.up * 90.0f));
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            ReturnButton();
            _nextScreen.SetActive(false);
        }

        /// <summary>
        /// Establece la posición y rotación del jugador.
        /// </summary>
        /// <param name="position">La nueva posición del jugador.</param>
        /// <param name="rotation">La nueva rotación del jugador.</param>
        private void SetPlayerPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if (_player != null)
            {
                _player.position = position;
                _player.rotation = rotation;
            }
        }
    }
}
