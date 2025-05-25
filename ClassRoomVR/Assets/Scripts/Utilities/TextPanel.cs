using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Text;
namespace ClassRoomVR
{
    /// <summary>
    /// Clase simple para encapsular un string.
    /// </summary>
    public class StringWrapper
    {
        public string Value;
    }

    /// <summary>
    /// Panel de texto que se actualiza periódicamente mostrando información dinámica,
    /// utilizando métodos configurados desde el Inspector de Unity.
    /// </summary>
    public class TextPanel : MonoBehaviour
    {
        /// <summary>
        /// Intervalo de tiempo (en segundos) entre cada actualización del panel de texto.
        /// </summary>
        public float time = 2.0f;

        /// <summary>
        /// Clase que almacena información sobre cada método que se mostrará en el panel.
        /// </summary>
        [System.Serializable]
        public class MethodInfoWrapper
        {
            /// <summary>
            /// Nombre del método (solo para mostrarlo como prefijo en el panel de texto).
            /// </summary>
            public string methodName;
            /// <summary>
            /// Si añade un salto de linea después de mostrar el valor.
            /// </summary>
            public bool newLine;
            /// <summary>
            /// Evento Unity que se debe asignar desde el Inspector.
            /// Debe recibir un StringWrapper como argumento y modificar su valor.
            /// </summary>
            public UnityEvent<StringWrapper> callbackEvent;
        }

        public List<MethodInfoWrapper> methods = new List<MethodInfoWrapper>();

        public TextMeshProUGUI textMeshPro;

        private void Start()
        {
            // Validación para evitar errores si el componente TextMeshProUGUI no está asignado

            if (textMeshPro == null)
            {
                Debug.LogWarning("TextMeshProUGUI not assigned.");
                return;
            }
            InvokeRepeating(nameof(UpdateTextPanel), 1, time);
        }

        /// <summary>
        /// Actualiza el contenido del panel de texto llamando a los métodos configurados.
        /// </summary>
        public void UpdateTextPanel()
        {
            // Validación de referencia antes de usar
            if (textMeshPro == null)
            {
                Debug.LogWarning("TextMeshProUGUI not assigned.");
                return;
            }

            // StringBuilder para construir el texto
            StringBuilder sb = new StringBuilder();

            // Se crea un solo StringWrapper que se reutiliza y se resetea cada vez
            StringWrapper result = new StringWrapper();

            foreach (var methodInfo in methods)
            {
                // Limpiar el valor antes de usarlo de nuevo
                result.Value = string.Empty;

                try
                {
                    // Invoca el evento que debe modificar el valor de result.Value
                    methodInfo.callbackEvent.Invoke(result);

                    // Añade el nombre del método si no está vacío
                    if (!string.IsNullOrEmpty(methodInfo.methodName))
                        sb.Append(methodInfo.methodName);

                    // Añade el resultado del método
                    sb.Append(result.Value);

                    // Añade un salto de línea si está configurado
                    if (methodInfo.newLine)
                        sb.Append("\n");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error invoking method {methodInfo.methodName}: {e}");
                }
            }

            // Asigna el texto construido al componente TextMeshProUGUI
            textMeshPro.text = sb.ToString();

        }
    }
}