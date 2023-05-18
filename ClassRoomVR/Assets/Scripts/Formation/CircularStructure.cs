using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class CircularStructure : Structure
    {
        [SerializeField] Option radiusOpt;
        [SerializeField] Option gradesOpt;

        public override void Set()
        {
            float radius = settings.Radius;
            int numObjects = settings.NumDesks;
            float degrees = settings.Degrees;

            if (parent != null) { Destroy(parent); }
            parent = new GameObject("Toggles");
            parent.transform.SetParent(transform, false);

            foreach (Transform child in parentDesk.transform)
            {
                Destroy(child.gameObject);
            }

            float angle = degrees / numObjects; // Angle between objects
            for (int i = 0; i < numObjects; i++)
            {
                var v = GetVector(angle, i, radius);
                Vector3 position = new Vector3(v.Item1 / 5.0f, -v.Item2 / 5.0f); // Object position
                position += parent.transform.position;
                var toggle = Instantiate(prefab, position, Quaternion.identity, parent.transform);

                position = new Vector3(v.Item1, 0, -v.Item2); // Desk position
                position += parentDesk.transform.position;
                var d = Instantiate(desk, position, Quaternion.identity, parentDesk.transform);
                d.transform.LookAt(parentDesk.transform);

                toggle.onValueChanged.AddListener(delegate { ChangeDesk(toggle); });
                list_.Add(toggle, d);
            }
        }

        void ChangeDesk(Toggle toggle)
        {
            list_[toggle].gameObject.SetActive(toggle.isOn);
        }

        System.Tuple<float, float> GetVector(float angle, int i, float radius)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius; // X coordinate
            float z = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius; // Z coordinate
            return new System.Tuple<float, float>(x, z);
        }

        private void OnEnable()
        {
            settings.NumDesks = settings.NumStudents;
            numDesks.SetValue(settings.NumStudents);
            numDesks.SetMin(settings.NumStudents);

            if (settings.StructureMode == StructureMode.U)
            {
                gradesOpt.gameObject.SetActive(false);
                settings.Degrees = 180f;
            }
            else
            {
                gradesOpt.gameObject.SetActive(true);
                settings.Degrees = settings.Degrees == 180f ? 360f : settings.Degrees;
            }

            gradesOpt.SetValue(settings.Degrees);
            radiusOpt.SetValue(settings.Radius);

            Set();
        }

        private void Start()
        {
            radiusOpt.onValueChanged.AddListener(ChangeRadius);
            gradesOpt.onValueChanged.AddListener(ChangeGrades);
        }

        void ChangeRadius(float value)
        {
            settings.Radius = value;
            Set();
        }

        void ChangeGrades(float value)
        {
            settings.Degrees = value;
            Set();
        }

        //setmaxdesks
    }
}
