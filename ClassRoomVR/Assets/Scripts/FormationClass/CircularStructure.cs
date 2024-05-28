using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class CircularStructure : Structure
    {
        [SerializeField] Option radiusOpt;
        [SerializeField] Option gradesOpt;
        // Flag indicating whether it's a U-structure
        bool isUStructure = false;

        // Method called when the script starts
        private void Start()
        {
            numDesks.onValueChanged.AddListener(ChangeObjects);
            radiusOpt.onValueChanged.AddListener(ChangeRadius);
            gradesOpt.onValueChanged.AddListener(ChangeGrades);
        }

        // Method to set up the circular structure
        public override void Set()
        {
            ControlO();
            if (isUStructure)
                DeskManager.Instance.CreateCircle(settings.NumDesks, settings.Radius);
            else
                DeskManager.Instance.CreateCircle(settings.NumDesks, settings.Radius, settings.Degrees);
            //   toggleDesks.CreateToggles(DeskManager.Instance.GetDeskPosition(), DeskManager.Instance.GetDesks());
            lastoption = 0;

        }

        private int lastoption = 0;

        private void ControlO()
        {
            if (settings.NumDesks > MaxDesk(settings.Radius, settings.Degrees))
            {
                AdjustSettings();
            }
        }
        private void AdjustSettings()
        {
            switch (lastoption)
            {
                case 0:
                    if (settings.Radius < radiusOpt.GetMax())
                    {
                        settings.Radius += 0.1f;
                    }
                    else if (settings.Degrees < gradesOpt.GetMax() && !isUStructure) { settings.Degrees += 10; }
                    else settings.NumDesks--;
                    break;

                case 1:
                    if (settings.NumDesks > numDesks.GetMin())
                    {
                        settings.NumDesks--;
                    }
                    else if (settings.Degrees < gradesOpt.GetMax() && !isUStructure) { settings.Degrees += 10; }
                    else settings.Radius += 0.1f;
                    break;

                case 2:
                    if (settings.NumDesks > numDesks.GetMin())
                    {
                        settings.NumDesks--;
                    }
                    else if (settings.Radius < radiusOpt.GetMax()) { settings.Radius += 0.1f; }
                    else settings.Degrees += 10;
                    break;
            }
            numDesks.SetValue(settings.NumDesks);
            gradesOpt.SetValue(settings.Degrees);
            radiusOpt.SetValue(settings.Radius);
            Set();

        }

        // Method called when the script is enabled
        private void OnEnable()
        {
            settings = GameManager.Instance.GetCurrentSettings(); // Get the current classroom settings
            int max = settings.StructureMode == StructureMode.U ? MaxDesk() / 2 : MaxDesk();
            numDesks.SetMax(max);
            // Set initial number of desks to match the number of students
            numDesks.SetMin(settings.NumDesks);
            float radius = Mathf.Min(settings.Radius, radiusOpt.GetMax());
            settings.Radius = radius;
            // Check if the structure mode is U
            if (settings.StructureMode == StructureMode.U)
            {
                gradesOpt.gameObject.SetActive(false);
                settings.Degrees = 180f;
                isUStructure = true;
            }
            else
            {
                gradesOpt.gameObject.SetActive(true);
                settings.Degrees = 360f;
                isUStructure = false;
            }

            numDesks.SetValue(settings.NumDesks);
            gradesOpt.SetValue(settings.Degrees);
            radiusOpt.SetValue(settings.Radius);
            Set();
        }

        // Method to handle radius change
        void ChangeRadius(float value)
        {
            lastoption = 1;
            settings.Radius = value;
            Set();
        }

        // Method to handle grades change
        void ChangeGrades(float value)
        {
            lastoption = 2;
            settings.Degrees = value;
            Set();
        }

        // Method called when the script is disabled
        private void OnDisable()
        {
            isUStructure = false;
        }

        float deskWithOffset;
        public override int MaxDesk()
        {
            Renderer boxCollider = DeskManager.Instance.GetDeskCollider();
            Vector3 aulaDimensions = DeskManager.Instance.GetComponent<BoxCollider>().size;
            Vector3 sillaDimensions = Vector3.Scale(boxCollider.bounds.size, boxCollider.transform.lossyScale);
            deskWithOffset = sillaDimensions.x * DeskManager.Instance.deskOffsetO;
            //Le quitamos lo que ocupa la mesa 3 veces para que puedan pasar los alumnos
            float radioMaximo = (float)System.Math.Round((double)Mathf.Min(aulaDimensions.x - sillaDimensions.z * 3f, aulaDimensions.z - sillaDimensions.z * 3f) / 2f, 1);
            radiusOpt.SetMax(radioMaximo);
            float anguloOcupacion = Mathf.Atan(deskWithOffset / (2 * radioMaximo)) * Mathf.Rad2Deg * 2;
            return Mathf.FloorToInt(360 / anguloOcupacion);
        }

        private int MaxDesk(float radio, float degrees) 
        {
            float anguloOcupacion = Mathf.Atan(deskWithOffset / (2 * radio)) * Mathf.Rad2Deg * 2;
            return Mathf.FloorToInt(degrees / anguloOcupacion);
        }

    }
}