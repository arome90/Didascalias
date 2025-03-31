using BehaviorDesigner.Runtime.Tasks.Unity.UnityAudioSource;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que representa una estructura de aulas con disposición en filas y columnas.
    /// </summary>
    public class NormalStructure2 : Structure2
    {
        [SerializeField] private Option _rowsOption; // Opción de UI para establecer el número de filas
        [SerializeField] private Option _columnsOption; // Opción de UI para establecer el número de columnas

        protected override void Start()
        {
            base.Start();
            numDesks.onValueChanged.AddListener(UpdateDeskLayout);
            _rowsOption.onValueChanged.AddListener(UpdateRows);
            _columnsOption.onValueChanged.AddListener(UpdateColumns);
        }

        /// <summary>
        /// Configura la disposición de los escritorios en la aula.
        /// </summary>
        public override void Set()
        {
            AdjustMatrix();
            DeskManager2.Instance.CreateRegularLayout(settings.NumDesks, settings.Rows, settings.Columns);
        }

        private void OnEnable()
        {
            settings = GameManager2.Instance.GetCurrentSettings(); // Obtiene la configuración actual del aula
            InitializeSettings();
            Set();
        }

        /// <summary>
        /// Inicializa la configuración de escritorios y opciones de fila y columna.
        /// </summary>
        
        // Él cálculo de mínimos de Rows y Columns no es de lo mejor. 
        // Tenemos el problema de que hay varias configuraciones que nos podrían dar el
        // resultado que queremos, y de esta forma solo nos estamos quedando con una
        // Ejemplo:
        // 3 Escritorios - 1 columna min. - 3 filas min.
        // ó
        // 3 Escritorios - 2 columnas min. - 2 filas min.
        // Deberíamos poder cambiar el mínimo de forma de dinámica según aumentamos/disminuimos las columnas.

        private void InitializeSettings()
        {
            numDesks.SetMax(MaxDesk());
            settings.NumDesks = settings.NumStudents;
            numDesks.SetMin(settings.NumStudents);
            settings.Columns = Mathf.Min((int)_columnsOption.GetMax(), Mathf.FloorToInt(Mathf.Sqrt(settings.NumDesks)));
            float divide = settings.Columns;
            if (divide == 0) divide = 1.0f;
            settings.Rows = Mathf.CeilToInt(settings.NumDesks / divide);

            _rowsOption.SetMin(settings.Rows);
            _columnsOption.SetMin(settings.Columns);
        }

        /// <summary>
        /// Ajusta la matriz de escritorios en función de la configuración actual.
        /// </summary>
        private void AdjustMatrix()
        {
            int totalDesks = settings.Columns * settings.Rows;

            if(settings.NumDesks == 0)
            {
                numDesks.SetMin(0);
                numDesks.SetValue(settings.NumDesks);
                settings.Rows = 0;
                _rowsOption.SetMin(0);
                _rowsOption.SetValue(settings.Rows);
                settings.Columns = 0;
                _columnsOption.SetMin(0);
                _columnsOption.SetValue(settings.Columns);
            }

            if(settings.NumDesks == 1)
            {
                numDesks.SetValue(settings.NumDesks);
                settings.Rows = 1;
                _rowsOption.SetValue(settings.Rows);
                settings.Columns = 1;
                _columnsOption.SetValue(settings.Columns);
            }
            else if (settings.NumDesks > totalDesks)
            {
                if(settings.Rows == 0)
                {
                    settings.Rows++;
                    _rowsOption.SetValue(settings.Rows);
                }
                if (_columnsOption.GetMax() == settings.Columns || settings.Columns > settings.Rows)
                {
                    settings.Rows++;
                    _rowsOption.SetValue(settings.Rows);
                }
                else
                {
                    settings.Columns++;
                    _columnsOption.SetValue(settings.Columns);
                }
            }
            else if (settings.NumDesks <= (settings.Columns - 1) * settings.Rows)
            {
                if (_columnsOption.GetMin() == settings.Columns || (settings.Columns < settings.Rows && settings.Rows > _rowsOption.GetMin()))
                {
                    settings.Rows--;
                    _rowsOption.SetValue(settings.Rows);
                }
                else if(settings.Columns > _columnsOption.GetMin())
                {
                    settings.Columns--;
                    _columnsOption.SetValue(settings.Columns);
                }
            }
            else
            {
                numDesks.SetValue(settings.NumDesks);
                _rowsOption.SetValue(settings.Rows);
                _columnsOption.SetValue(settings.Columns);
            }
        }

        /// <summary>
        /// Actualiza el número de filas en la configuración y ajusta el conteo de escritorios.
        /// </summary>
        /// <param name="value">Nuevo valor de filas.</param>
        private void UpdateRows(float value)
        {
            settings.Rows = (int)value;
            ValidateAndUpdateDeskCount();
            Set();
        }

        /// <summary>
        /// Actualiza el número de columnas en la configuración y ajusta el conteo de escritorios.
        /// </summary>
        /// <param name="value">Nuevo valor de columnas.</param>
        private void UpdateColumns(float value)
        {
            settings.Columns = (int)value;
            ValidateAndUpdateDeskCount();
            Set();
        }

        /// <summary>
        /// Valida y actualiza el conteo de escritorios en función del número de filas y columnas.
        /// </summary>
        private void ValidateAndUpdateDeskCount()
        {
            int difference = (settings.Columns * settings.Rows) - settings.NumDesks;
            if (difference != 0)
            {
                settings.NumDesks += difference;
                numDesks.SetValue(settings.NumDesks);
            }
        }

        /// <summary>
        /// Calcula el número máximo de escritorios que pueden colocarse en la aula.
        /// </summary>
        /// <returns>Número máximo de escritorios.</returns>
        public override int MaxDesk()
        {
            Renderer deskCollider = DeskManager2.Instance.GetDeskCollider();
            Vector3 deskDimensions = Vector3.Scale(deskCollider.bounds.size, deskCollider.transform.lossyScale);
            Vector3 classroomDimensions = DeskManager2.Instance.GetComponent<BoxCollider>().size;

            int maxColumns = Mathf.RoundToInt(classroomDimensions.x / (deskDimensions.x * DeskManager2.Instance.DeskOffsetX));
            int maxRows = Mathf.RoundToInt(classroomDimensions.z / (deskDimensions.z * 2 * DeskManager2.Instance.DeskOffsetZ));

            _columnsOption.SetMax(maxColumns);
            _rowsOption.SetMax(maxRows);

            Debug.Log($"Max Rows: {maxRows}, Max Columns: {maxColumns}");
            return maxRows * maxColumns;
        }
    }
}
