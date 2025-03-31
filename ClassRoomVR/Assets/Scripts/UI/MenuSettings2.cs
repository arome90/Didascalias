using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona la configuración del menú de ajustes.
    /// </summary>
    public class MenuSettings2 : MonoBehaviour
    {
        private ClassSettings2 _settings; // Configuraciones del aula
        [SerializeField] private TMP_Dropdown _structureDropdown; // Menú desplegable para seleccionar la estructura
        [SerializeField] private Button _editDeskPositionButton; // Botón para editar la posición de los escritorios
        [SerializeField] private Option _boysOption; // Opción para configurar el número de chicos
        [SerializeField] private Option _girlsOption; // Opción para configurar el número de chicas
        private int _maxStudents; // Número máximo de estudiantes permitido

        private void Start()
        {
            InitializeSettings();
            InitializeDropdown();
            InitializeListeners();
            UpdateStructureAndStudents();
        }

        /// <summary>
        /// Inicializa las configuraciones del aula.
        /// </summary>
        private void InitializeSettings()
        {
            _settings = GameManager2.Instance.GetCurrentSettings();
            _girlsOption.SetValue(_settings.NumWomen);
            _boysOption.SetValue(_settings.NumMen);
        }

        /// <summary>
        /// Inicializa el menú desplegable de estructuras.
        /// </summary>
        private void InitializeDropdown()
        {
            string[] structure = Enum.GetNames(typeof(StructureMode2));
            List<string> options = new List<string>();

            for (int i = 0; i < structure.Length; i++) {
                string traduction = "";
                Didascalia_LocalizationManager.Instance.GetTranslation(structure[i],
                    Didascalia_LocalizationManager.TableCollections.SPANISH, out traduction);

                options.Add(traduction);
            }

            _structureDropdown.AddOptions(options);
            _structureDropdown.value = (int)_settings.StructureMode;
        }

        /// <summary>
        /// Inicializa los listeners para los eventos de los botones y opciones.
        /// </summary>
        private void InitializeListeners()
        {
            _structureDropdown.onValueChanged.AddListener(ChangeStructure);
            _boysOption.onValueChanged.AddListener(value => UpdateStudentCount(value, Gender2.Men));
            _girlsOption.onValueChanged.AddListener(value => UpdateStudentCount(value, Gender2.Women));
            _editDeskPositionButton.onClick.AddListener(() =>
            {
                MenuTransition2.Instance.GoNextScreen();
                MenuTransition2.Instance.MoveClase();
            });
        }

        /// <summary>
        /// Cambia la estructura del aula según la selección del menú desplegable.
        /// </summary>
        /// <param name="value">El valor seleccionado en el menú desplegable.</param>
        private void ChangeStructure(int value)
        {
            _settings.StructureMode = (StructureMode2)value;
            DeskManager2.Instance.DestroyChildren();
            UpdateStructureAndStudents();
        }

        /// <summary>
        /// Actualiza la estructura y el número máximo de estudiantes según la estructura seleccionada.
        /// </summary>
        private void UpdateStructureAndStudents()
        {
            _maxStudents = _settings.StructureMode switch
            {
                StructureMode2.Fila => 30,
                StructureMode2.Circular => 12,
                StructureMode2.U => 6,
                _ => _maxStudents
            };

            if (_settings.NumMen + _settings.NumWomen > _maxStudents)
            {
                DistributeStudentCount();
            }

            SetMaxStudents();
        }

        /// <summary>
        /// Actualiza el número de estudiantes según el género seleccionado.
        /// </summary>
        /// <param name="value">El nuevo número de estudiantes.</param>
        /// <param name="gender">El género de los estudiantes.</param>
        private void UpdateStudentCount(float value, Gender2 gender)
        {
            if (gender == Gender2.Men)
            {
                _settings.NumMen = (int)value;
            }
            else
            {
                _settings.NumWomen = (int)value;
            }
            SetMaxStudents();
            DeskManager2.Instance.DestroyChildren();
        }

        /// <summary>
        /// Configura el número máximo de estudiantes y escritorios.
        /// </summary>
        private void SetMaxStudents()
        {
            _settings.NumStudents = _settings.NumMen + _settings.NumWomen;
            _boysOption.SetMax(_maxStudents - _settings.NumWomen);
            _girlsOption.SetMax(_maxStudents - _settings.NumMen);
            _settings.NumDesks = _settings.NumStudents;
        }

        /// <summary>
        /// Distribuye el número de estudiantes equitativamente entre chicos y chicas.
        /// </summary>
        private void DistributeStudentCount()
        {
            _settings.NumMen = _settings.NumWomen = _maxStudents / 2;
            _boysOption.SetValue(_maxStudents / 2);
            _girlsOption.SetValue(_maxStudents / 2);
        }
    }
}
