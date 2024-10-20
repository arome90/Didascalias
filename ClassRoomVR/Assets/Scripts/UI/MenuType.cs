using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona el menú para seleccionar el tipo de estructura en la interfaz de usuario.
    /// </summary>
    public class MenuType : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown _structureDropdown; // Dropdown para seleccionar el tipo de estructura
        [SerializeField] private Button _editButton; // Botón para acceder a la pantalla de edición
        [SerializeField] private GameObject _editText; // Objeto de texto para mostrar u ocultar

        private void Awake()
        {
            _structureDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            _editButton.onClick.AddListener(OnEditButtonClick);
        }

        private void OnEnable()
        {
            RefreshDropdownOptions();
        }

        /// <summary>
        /// Maneja el clic en el botón de edición y navega a la pantalla de edición.
        /// </summary>
        private void OnEditButtonClick()
        {
            GameManager.Instance.SetCurrentSettings(0); // Establece la configuración actual
            MenuTransition.Instance.GoNextScreen(); // Navega a la siguiente pantalla
        }

        /// <summary>
        /// Maneja el cambio en el valor seleccionado del dropdown y actualiza la visibilidad del texto de edición.
        /// </summary>
        /// <param name="value">Índice del valor seleccionado en el dropdown.</param>
        private void OnDropdownValueChanged(int value)
        {
            _editText.SetActive(value == 0); // Muestra el texto si el valor seleccionado es 0
            GameManager.Instance.SetCurrentSettings(value); // Actualiza la configuración actual según la selección
        }

        /// <summary>
        /// Refresca las opciones del dropdown con las configuraciones disponibles.
        /// </summary>
        private void RefreshDropdownOptions()
        {
            _structureDropdown.ClearOptions(); // Limpia las opciones actuales del dropdown
            var availableSettings = GameManager.Instance.GetAvailableSettings(); // Obtiene las configuraciones disponibles
            var currentIndex = GameManager.Instance.GetIndexCurrentSettings(); // Obtiene el índice de la configuración actual
            _structureDropdown.AddOptions(GetDropdownOptions(availableSettings)); // Añade nuevas opciones al dropdown
            _structureDropdown.value = currentIndex; // Establece el valor seleccionado al índice de la configuración actual
        }

        /// <summary>
        /// Convierte las configuraciones disponibles en opciones para el dropdown.
        /// </summary>
        /// <param name="classes">Arreglo de configuraciones de clase.</param>
        /// <returns>Lista de nombres de opciones para el dropdown.</returns>
        private List<string> GetDropdownOptions(ClassSettings[] classes)
        {
            List<string> dropdownOptions = new List<string>(classes.Length - 1);
            int currentLocale = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
            for (int i = 0; i < classes.Length - 1; i++)
            {
                string _name;
                _name = classes[i].Name.GetLocalizedString();
                    
                dropdownOptions.Add(_name); // Añade el nombre de cada configuración a la lista de opciones
            }
            return dropdownOptions;
        }
    }
}
