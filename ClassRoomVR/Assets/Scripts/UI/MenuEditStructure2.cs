using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona la edición de la estructura del menú.
    /// </summary>
    public class MenuEditStructure2 : MonoBehaviour
    {
        [SerializeField] private Button _applyButton; // Botón para aplicar cambios
        [SerializeField] private Structure2 _circularStructure; // Estructura circular
        [SerializeField] private Structure2 _filaStructure; // Estructura en fila

        private ClassSettings2 _settings; // Configuraciones del aula

        private void Awake()
        {
            _settings = GameManager2.Instance.GetCurrentSettings();
            _applyButton.onClick.AddListener(OnApplyButtonClick);
        }

        private void OnEnable()
        {
            Debug.Log(_settings.StructureMode);
            UpdateStructureVisibility();
        }

        /// <summary>
        /// Maneja el clic en el botón de aplicar cambios.
        /// </summary>
        private void OnApplyButtonClick()
        {
            MenuTransition2.Instance.GoBackScreen();
            MenuTransition2.Instance.MovePizarra();
        }

        /// <summary>
        /// Actualiza la visibilidad de las estructuras en función del modo de estructura.
        /// </summary>
        private void UpdateStructureVisibility()
        {
            bool isCircular = _settings.StructureMode == StructureMode2.Circular
                || _settings.StructureMode == StructureMode2.U;

            SetStructureVisibility(_circularStructure, isCircular);
            SetStructureVisibility(_filaStructure, !isCircular);
        }

        /// <summary>
        /// Establece la visibilidad de una estructura.
        /// </summary>
        /// <param name="structure">La estructura a actualizar.</param>
        /// <param name="isVisible">Indica si la estructura debe ser visible.</param>
        private void SetStructureVisibility(Structure2 structure, bool isVisible)
        {
            structure?.gameObject.SetActive(isVisible);
        }
    }
}
