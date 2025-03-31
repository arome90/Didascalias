using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Extensions;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase base abstracta que representa una estructura de aula.
    /// </summary>
    public abstract class Structure2 : MonoBehaviour
    {
        [SerializeField] protected Option numDesks; // Opción de UI para establecer el número de escritorios
        [SerializeField] protected Toggle fillEmptyDesks;
        protected ClassSettings2 settings; // Configuraciones para el aula

        private void SetFillEmptyDesks(bool value)
        {
            settings.FillEmptyDesks = value;
        }

        protected virtual void Start()
        {
            fillEmptyDesks.onValueChanged.AddListener(SetFillEmptyDesks);
        }

        /// <summary>
        /// Método abstracto para configurar la estructura del aula.
        /// </summary>
        public abstract void Set();

        /// <summary>
        /// Método abstracto para recuperar el número máximo de escritorios que la estructura puede acomodar.
        /// </summary>
        /// <returns>El número máximo de escritorios.</returns>
        public abstract int MaxDesk();

        private void OnDisable()
        {
            // Limpia los objetos hijos inactivos bajo parentDesk cuando la estructura está desactivada
            DeskManager2.Instance.DestroyInactiveChildObjects();
        }

        /// <summary>
        /// Actualiza el número de escritorios y aplica cambios en la disposición del aula.
        /// </summary>
        /// <param name="value">El nuevo número de escritorios a establecer.</param>
        protected void UpdateDeskLayout(float value)
        {
            if (settings == null)
            {
                Debug.LogWarning("ClassSettings no está asignado.");
                return;
            }

            settings.NumDesks = Mathf.Clamp((int)value, 0, MaxDesk()); // Asegura que el número de escritorios esté dentro del rango válido
            // Nos muestra una opción para saber si queremos o no rellenar los escritorios adicionales sin estudiantes
            if(settings.NumDesks > settings.NumStudents)
            {
                fillEmptyDesks.gameObject.SetActive(true);
            }
            else
            {
                fillEmptyDesks.gameObject.SetActive(false);
            }
            Set(); // Organiza los escritorios según la configuración actualizada
        }
    }
}
