using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class CircularStructure : Structure
    {
        [SerializeField] Option radiusOpt;
        [SerializeField] Option gradesOpt;
        bool Ubool = false;
        public override void Set()
        {
            float radius = (float)settings.Radius;
            int numObjects = settings.NumDesks;
            float degrees = (float)settings.Degrees;

            Destroy(parent); 
            parent = new GameObject("Toggles");
            parent.transform.SetParent(transform, false);

            foreach (Transform child in parentDesk.transform)
            {
                Destroy(child.gameObject);
            }

            float angle = degrees / (numObjects - (Ubool ? 1.0f : 0.0f)); // Angle between objects
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
                d.onCollisionChanged.AddListener(CollisionWithOtherDesk);
               
                toggle.onValueChanged.AddListener(delegate { ChangeDesk(toggle); });
                toggleToDeskMap.Add(toggle, d);
            }
            
        }


        void CollisionWithOtherDesk()
        {
            foreach (Desk d in toggleToDeskMap.Values)
            {
                d.onCollisionChanged.RemoveListener(CollisionWithOtherDesk);
            }

            switch (lastOptionClicked)
            {
                case 0:
                    if (settings.Radius < radiusOpt.GetMax())
                    {
                        settings.Radius += 0.1f;
                        Set();
                        radiusOpt.SetValue(settings.Radius);
                    }
                    else if (!Ubool && settings.Degrees <= 360f)
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
                        // radiusOpt.SetMin(settings.Radius);
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
                        //gradesOpt.SetMin(settings.Degrees);
                        gradesOpt.SetValue(settings.Degrees);

                    }
                    break;
            }


        }

        void ChangeDesk(Toggle toggle)
        {
            toggleToDeskMap[toggle].gameObject.SetActive(toggle.isOn);
        }

        System.Tuple<float, float> GetVector(float angle, int i, float radius)
        {
            float cos = Mathf.Cos(Mathf.Deg2Rad * (angle * i));
            float sin = Mathf.Sin(Mathf.Deg2Rad * (angle * i));            
            return new System.Tuple<float, float>((float)System.Math.Round(cos, 3) * radius, (float)System.Math.Round(sin, 3) * radius);
        }

        private void OnEnable()
        {
            settings.NumDesks = settings.NumStudents;
            numDesks.SetValue(settings.NumStudents);
            numDesks.SetMin(settings.NumStudents);

            if (settings.StructureMode == StructureMode.U)
            {
                Debug.Log(numDesks.GetMax());
                numDesks.SetMax(1 + (numDesks.GetMax() / 2));
                gradesOpt.gameObject.SetActive(false);
                settings.Degrees = 180f;
                Ubool = true;
            }
            else
            {
                gradesOpt.gameObject.SetActive(true);
                settings.Degrees = 360f;
                Ubool = false;
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
            lastOptionClicked = 1;
            settings.Radius = value;
            Set();

        }



        void ChangeGrades(float value)
        {
            lastOptionClicked = 2;
            settings.Degrees = value;
            Set();
        }

        private void OnDisable()
        {

            if (settings.StructureMode == StructureMode.U)
            {
                numDesks.SetMax((numDesks.GetMax()-1) * 2 );
                Debug.Log(numDesks.GetMax());
                Ubool = false;
            }
        }
    }
}
