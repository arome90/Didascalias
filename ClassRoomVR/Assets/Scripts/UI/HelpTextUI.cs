using UnityEngine;
using TMPro;

    /// <summary>
    /// Clase que gestiona el texto de ayuda en la interfaz de usuario.
    /// </summary>
    public class HelpText : MonoBehaviour
    {
        [SerializeField] private string[] _texts; // Array de textos de ayuda
        [SerializeField] private TextMeshProUGUI _text; // Componente de texto de la interfaz de usuario
        private bool _isActive; // Estado de visibilidad del texto de ayuda

        /// <summary>
        /// Alterna la visibilidad del texto de ayuda.
        /// </summary>
        private void ToggleTextDisplay()
        {
            _text.gameObject.SetActive(_isActive);
        }

        /// <summary>
        /// Configura la visibilidad del texto de ayuda.
        /// </summary>
        /// <param name="active">Indica si el texto debe estar activo o no.</param>
        public void SetTextDisplay(bool active)
        {
            _isActive = active;
            ToggleTextDisplay();
        }

        /// <summary>
        /// Actualiza el texto de ayuda mostrado.
        /// </summary>
        /// <param name="index">El índice del texto que se debe mostrar.</param>
        public void UpdateText(int index)
        {
            if (index >= 0 && index < _texts.Length)
            {
                _text.text = _texts[index];
            }
            else
            {
                Debug.LogWarning("Índice de texto inválido");
            }
        }
    }

