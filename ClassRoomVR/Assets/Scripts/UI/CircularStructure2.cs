using MathNet.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona la disposición circular de escritorios.
    /// </summary>
    public class CircularStructure2 : Structure
    {
        [SerializeField] private Option _radiusOption;
        [SerializeField] private Option _degreesOption;

        private bool _isUStructure;
        private int _lastOption;
        private float _deskWithOffset;

        protected override void Start()
        {
            base.Start();
            numDesks.onValueChanged.AddListener(UpdateDeskLayout);
            _radiusOption.onValueChanged.AddListener(UpdateRadius);
            _degreesOption.onValueChanged.AddListener(UpdateDegrees);
        }

        /// <summary>
        /// Configura la disposición de los escritorios en función de los ajustes.
        /// </summary>
        public override void Set()
        {
            ValidateAndAdjustDeskLayout();

            Debug.Log("Número de escritorios: " + settings.NumDesks);
            if (_isUStructure)
            {
                DeskManager2.Instance.CreateCircle(settings.NumDesks, settings.Radius);
            }
            else
            {
                DeskManager2.Instance.CreateCircle(settings.NumDesks, settings.Radius, settings.Degrees);
            }

            _lastOption = 0;
        }

        /// <summary>
        /// Valida y ajusta la disposición de los escritorios si es necesario.
        /// </summary>
        private void ValidateAndAdjustDeskLayout()
        {
            if (settings.NumDesks > MaxDesk(settings.Radius, settings.Degrees))
            {
                AdjustSettings();
            }
        }

        /// <summary>
        /// Ajusta los ajustes de disposición de los escritorios en función de las opciones actuales.
        /// </summary>
        private void AdjustSettings()
        {
            switch (_lastOption)
            {
                case 0:
                    AdjustRadiusOrDegreesOrDesks();
                    break;

                case 1:
                    AdjustNumDesksOrDegreesOrRadius();
                    break;

                case 2:
                    AdjustNumDesksOrRadiusOrDegrees();
                    break;
            }

            UpdateUIValues();
            Set();
        }

        private void AdjustRadiusOrDegreesOrDesks()
        {
            if (settings.Radius < _radiusOption.GetMax())
            {
                settings.Radius += 0.1f;
            }
            else if (settings.Degrees < _degreesOption.GetMax() && !_isUStructure)
            {
                settings.Degrees += 10;
            }
            else
            {
                settings.NumDesks--;
            }
        }

        private void AdjustNumDesksOrDegreesOrRadius()
        {
            if (settings.NumDesks > numDesks.GetMin())
            {
                settings.NumDesks--;
            }
            else if (settings.Degrees < _degreesOption.GetMax() && !_isUStructure)
            {
                settings.Degrees += 10;
            }
            else
            {
                settings.Radius += 0.1f;
            }
        }

        private void AdjustNumDesksOrRadiusOrDegrees()
        {
            if (settings.NumDesks > numDesks.GetMin())
            {
                settings.NumDesks--;
            }
            else if (settings.Radius < _radiusOption.GetMax())
            {
                settings.Radius += 0.1f;
            }
            else
            {
                settings.Degrees += 10;
            }
        }

        /// <summary>
        /// Actualiza los valores de la interfaz de usuario.
        /// </summary>
        private void UpdateUIValues()
        {
            numDesks.SetValue(settings.NumDesks);
            _degreesOption.SetValue(settings.Degrees);
            _radiusOption.SetValue(settings.Radius);
        }

        private void OnEnable()
        {
            settings = GameManager2.Instance.GetCurrentSettings();
            int maxDesks = MaxDesk();
            Debug.Log("Numero máximo de escritorios en círculo: " + maxDesks);
            maxDesks = settings.StructureMode == StructureMode2.U
                ? maxDesks / 2
                : maxDesks;
            Debug.Log("Numero máximo de escritorios en setting actual: " + maxDesks);

            numDesks.SetMax(maxDesks);
            numDesks.SetMin(settings.NumDesks);

            settings.Radius = Mathf.Min(settings.Radius, _radiusOption.GetMax());

            if (settings.StructureMode == StructureMode2.U)
            {
                _degreesOption.gameObject.SetActive(false);
                settings.Degrees = 180f;
                _isUStructure = true;
            }
            else
            {
                _degreesOption.gameObject.SetActive(true);
                settings.Degrees = 360f;
                _isUStructure = false;
            }

            UpdateUIValues();
            Set();
        }

        private void UpdateRadius(float value)
        {
            _lastOption = 1;
            settings.Radius = value;
            Set();
        }

        private void UpdateDegrees(float value)
        {
            _lastOption = 2;
            settings.Degrees = value;
            Set();
        }

        private void OnDisable()
        {
            _isUStructure = false;
        }

        /// <summary>
        /// Calcula el número máximo de escritorios en función de las dimensiones de la clase.
        /// </summary>
        /// <returns>El número máximo de escritorios.</returns>
        public override int MaxDesk()
        {
            Renderer deskCollider = DeskManager2.Instance.GetDeskCollider();
            Vector3 classroomDimensions = DeskManager2.Instance.GetComponent<BoxCollider>().size;
            Vector3 deskDimensions = Vector3.Scale(deskCollider.bounds.size, deskCollider.transform.lossyScale);

            _deskWithOffset = deskDimensions.x * DeskManager2.Instance.DeskOffsetO;

            float maxRadius = Mathf.Min(
                (classroomDimensions.x - deskDimensions.z * 3f) / 2f,
                (classroomDimensions.z - deskDimensions.z * 3f) / 2f
            );
            maxRadius = maxRadius.Round(2);
            _radiusOption.SetMax(maxRadius);

            float angleOccupied = Mathf.Atan(_deskWithOffset / (2 * maxRadius)) * Mathf.Rad2Deg * 2;
            return Mathf.FloorToInt(360 / angleOccupied);
        }

        /// <summary>
        /// Calcula el número máximo de escritorios en función del radio y los grados dados.
        /// </summary>
        /// <param name="radius">El radio del círculo.</param>
        /// <param name="degrees">Los grados del ángulo del círculo.</param>
        /// <returns>El número máximo de escritorios.</returns>
        private int MaxDesk(float radius, float degrees)
        {
            float angleOccupied = Mathf.Atan(_deskWithOffset / (2 * radius)) * Mathf.Rad2Deg * 2;
            return Mathf.FloorToInt(degrees / angleOccupied);
        }
    }
}
