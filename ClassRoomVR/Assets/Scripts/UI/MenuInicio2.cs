using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Meta.WitAi.Data.Configuration;
using TMPro;
using static ClassRoomVR.GameManager2;
using System.IO;
using System.Collections;
using Unity.VisualScripting;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona el menú de inicio.
    /// </summary>
    public class MenuInicio2 : MonoBehaviour
    {
        [SerializeField] private Button _enter; // Botón para entrar
        [SerializeField] private Button _tutorial; // Botón para el tutorial
        [SerializeField] private Button _quitButton; // Botón para salir
        [SerializeField] private TMP_Dropdown _languageSelector; // Selector de idioma

        [SerializeField] private GameObject _nextScreen; // Pantalla siguiente
        [SerializeField] private Vector3 _playerDestination; // Destino del jugador
        [SerializeField] private Vector3 _playerInitialPosition; // Posición inicial del jugador
        [SerializeField] private Transform _player; // Transform del jugador

        Dictionary<string, WitConfiguration> _languageDictionary = new Dictionary<string, WitConfiguration>();

        private void Start()
        {
            AddButtonListeners();
            DeskManager2.Instance.DestroyChildren();
            DontDestroyOnLoad(GameObject.Find("DeskManager"));
        }

        /// <summary>
        /// Agrega los listeners a los botones.
        /// </summary>
        private void AddButtonListeners()
        {
            _enter.onClick.AddListener(OnEnterButtonClick);
            _tutorial.onClick.AddListener(OnTutorialButtonClick);
            _quitButton.onClick.AddListener(QuitButton);
            LanguageSelector();
        }

        /// <summary>
        /// Maneja el clic en el botón de entrar.
        /// </summary>
        private void OnEnterButtonClick()
        {
            Instance.SetLastUsedSettings();
            PlayButton();
            GoNextScreen();
        }

        /// <summary>
        /// Maneja el clic en el botón de tutorial.
        /// </summary>
        private void OnTutorialButtonClick()
        {
            _tutorial.interactable = false;
            Instance.LoadTutorial();
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
            LanguageOption[] languages = Instance.witAppsForLanguages;
            for (int i = 0; i < languages.Length; ++i)
            {
                _languageDictionary.Add(languages[i].name, languages[i].witApp);

                _languageSelector.options.Add(new TMP_Dropdown.OptionData() { text = languages[i].name });
            }

            StartCoroutine(ChangeDropdownValue());
        }

        public void OnDestroy()
        {
            StopAllCoroutines();
        }

        IEnumerator ChangeDropdownValue()
        {
            yield return new WaitForSeconds(0.2f);
            _languageSelector.value = (int)Didascalia_LocalizationManager.CurrentLanguage;

            _languageSelector.onValueChanged.AddListener(delegate { ChangeLanguage(_languageSelector); });
            _languageSelector.RefreshShownValue();

        }

        private void ChangeLanguage(TMP_Dropdown dropdown)
        {
            ChangeLanguage(dropdown, dropdown.value);
        }

        private void ChangeLanguage(TMP_Dropdown dropdown, int value)
        {
            string currentLanguage = dropdown.options[value].text;

            Instance.ChangeLanguage(_languageDictionary[currentLanguage]);
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
