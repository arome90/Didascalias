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

        // Method to set up the circular structure
        public override void Set()
        {
            // Extract settings values
            float radius = (float)settings.Radius;
            int numObjects = settings.NumDesks;
            float degrees = (float)settings.Degrees;

            // Destroy previous parent GameObject
            Destroy(parent);
            parent = new GameObject("Toggles");
            parent.transform.SetParent(transform, false);

            // Destroy existing desk objects within the parentDesk
            foreach (Transform child in parentDesk.transform)
            {
                Destroy(child.gameObject);
            }

            // Calculate angle between objects
            float angle = degrees / (numObjects - (isUStructure ? 1.0f : 0.0f));
            var parentTransform = parent.transform;
            var parentDeskTransform = parentDesk.transform;
            for (int i = 0; i < numObjects; i++)
            {
                // Calculate position of the toggle object
                var v = GetVector(angle, i, radius);
                Vector3 position = new Vector3(v.Item1 / 5.0f, -v.Item2 / 5.0f) + parentTransform.position;

                // Instantiate a toggle object
                var toggle = Instantiate(prefab, position, Quaternion.identity, parentTransform);

                // Calculate position of the desk object
                position = new Vector3(v.Item1, 0, -v.Item2) + parentDeskTransform.position;

                // Instantiate a desk object and set its properties
                var d = Instantiate(desk, position, Quaternion.identity, parentDeskTransform);
                d.transform.LookAt(parentDeskTransform);
                d.onCollisionChanged.AddListener(CollisionWithOtherDesk);

                // Add listeners to toggle events
                toggle.onValueChanged.AddListener(delegate { ChangeDesk(toggle); });
                toggleToDeskMap.Add(toggle, d);
            }
        }

        // Method called when collision with other desk occurs
        void CollisionWithOtherDesk()
        {
            foreach (Desk d in toggleToDeskMap.Values)
            {
                d.onCollisionChanged.RemoveListener(CollisionWithOtherDesk);
            }

            // Determine the action based on the last clicked option
            switch (lastOptionClicked)
            {
                case 0:
                    if (settings.Radius < radiusOpt.GetMax())
                    {
                        settings.Radius += 0.1f;
                        Set();
                        radiusOpt.SetValue(settings.Radius);
                    }
                    else if (!isUStructure && settings.Degrees <= 360f)
                    {
                        settings.Degrees += 10;
                        Set();
                        gradesOpt.SetValue(settings.Degrees);
                    }
                    break;
                case 1:
                    if (settings.NumDesks > settings.NumStudents)
                    {
                        settings.NumDesks -= 1;
                        Set();
                        numDesks.SetValue(settings.NumDesks);
                    }
                    else
                    {
                        settings.Radius += 0.1f;
                        Set();
                        radiusOpt.SetValue(settings.Radius);
                    }
                    break;
                case 2:
                    if (settings.NumDesks > settings.NumStudents)
                    {
                        settings.NumDesks -= 1;
                        Set();
                        numDesks.SetValue(settings.NumDesks);
                    }
                    else
                    {
                        settings.Degrees += 10f;
                        Set();
                        gradesOpt.SetValue(settings.Degrees);
                    }
                    break;
            }
        }

        // Method to handle toggle value change
        void ChangeDesk(Toggle toggle)
        {
            toggleToDeskMap[toggle].gameObject.SetActive(toggle.isOn);
        }

        // Method to calculate a vector based on angle and index
        System.Tuple<float, float> GetVector(float angle, int i, float radius)
        {
            float cos = Mathf.Cos(Mathf.Deg2Rad * (angle * i));
            float sin = Mathf.Sin(Mathf.Deg2Rad * (angle * i));
            return new System.Tuple<float, float>((float)System.Math.Round(cos, 3) * radius, (float)System.Math.Round(sin, 3) * radius);
        }

        // Method called when the script is enabled
        private void OnEnable()
        {
            // Set initial number of desks to match the number of students
            settings.NumDesks = settings.NumStudents;
            numDesks.SetValue(settings.NumStudents);
            numDesks.SetMin(settings.NumStudents);

            // Check if the structure mode is U
            if (settings.StructureMode == StructureMode.U)
            {
                numDesks.SetMax(1 + (numDesks.GetMax() / 2));
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

            gradesOpt.SetValue(settings.Degrees);
            radiusOpt.SetValue(settings.Radius);

            Set();
        }

        // Method called when the script starts
        private void Start()
        {
            radiusOpt.onValueChanged.AddListener(ChangeRadius);
            gradesOpt.onValueChanged.AddListener(ChangeGrades);
        }

        // Method to handle radius change
        void ChangeRadius(float value)
        {
            lastOptionClicked = 1;
            settings.Radius = value;
            Set();
        }

        // Method to handle grades change
        void ChangeGrades(float value)
        {
            lastOptionClicked = 2;
            settings.Degrees = value;
            Set();
        }

        // Method called when the script is disabled
        private void OnDisable()
        {
            if (settings.StructureMode == StructureMode.U)
            {
                numDesks.SetMax((numDesks.GetMax() - 1) * 2);
                isUStructure = false;
            }
        }
        [SerializeField] float espacioEntreParedYSilla=0.3f;
        public override void MaxDesk()
        {
            float anchoDisponible = aula.size.x - 2 * espacioEntreParedYSilla;
            float largoDisponible = aula.size.z - 2 * espacioEntreParedYSilla;
            float radioMaximo = Mathf.Min(anchoDisponible, largoDisponible) / 2f;
            radiusOpt.SetMax(radioMaximo);
            Debug.Log(radioMaximo);
        }
    }
}
