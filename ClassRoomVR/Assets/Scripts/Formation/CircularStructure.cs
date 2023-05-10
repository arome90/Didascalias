using System.Collections;
using System.Collections.Generic;
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

            float radius = settings.radius;
            int numObjects = settings.numDesks;
            float grades= settings.grades;
            if (parent != null) { Destroy(parent); }
            parent = new GameObject("Toggles");
            parent.transform.SetParent(transform, false);

            foreach (Transform child in parentDesk.transform)
            {
                Destroy(child.gameObject);
            }


            float angle = grades / numObjects; // ángulo entre los objetos
            for (int i = 0; i < numObjects; i++)
            {
                var v = getVector(angle, i,radius);
                Vector3 position = new Vector3(v.Item1/5.0f, -v.Item2/5.0f); // posición del objeto
                position += parent.transform.position;
                var toggle = Instantiate(prefab, position, Quaternion.identity, parent.transform);

                position = new Vector3(v.Item1,0, -v.Item2); // posición del objeto
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


        System.Tuple<float,float> getVector(float angle,int i,float radio) 
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle * i)*radio ; // coordenada x
            float z = Mathf.Sin(Mathf.Deg2Rad * angle * i)*radio ; // coordenada z
            return new System.Tuple<float,float>(x,z);
        }

        private void OnEnable()
        {
            radiusOpt.onValueChanged.AddListener(ChangeRadius);
            // SetMaxDesks();
            radiusOpt.SetValue(settings.radius);
            if (settings.StructureClass == StructureMode.U) 
            {
                gradesOpt.gameObject.SetActive(false);
                settings.grades = 180f;
            }
            else
            {
                gradesOpt.gameObject.SetActive(true);
                settings.grades = settings.grades == 180f ? 360f : settings.grades;
            }

            Set();

        }

        void ChangeRadius(float value)
        {
            settings.radius = value;
           // SetMaxDesks();
            Set();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeRadius(settings.radius + 0.1f);
                radiusOpt.SetValue(settings.radius);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2)) 
            {
                ChangeRadius(settings.radius - 0.1f);
                radiusOpt.SetValue(settings.radius);

            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeObjects(settings.numDesks + 1);
                numDesks.SetValue(settings.numDesks);

            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                ChangeObjects(settings.numDesks - 1);
                numDesks.SetValue(settings.numDesks);

            }
        }
        //void SetMaxDesks()
        //{
        ////    n(r) = -r ^ 2 + 43r - 374
        //    float r = settings.radius;
        //    Debug.Log(r);
        //    Debug.Log(settings.numDesks);
        //    int n = (int)(Mathf.Pow(-r, 2) + 43 * r - 374);
        //    if (settings.numDesks > n)
        //    {
        //        settings.numDesks = n;
        //        numDesks.SetValue(settings.numDesks);
        //    }
        //}

        /*

       float angle = grades / numObjects; // ángulo entre los objetos
            for (int i = 0; i < numObjects; i++)
            {
                float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius; // coordenada x
                float z = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius; // coordenada z
                Vector3 position = new Vector3(x, -z); // posición del objeto
                position += parent.transform.position;
                var toggle = Instantiate(prefab, position, Quaternion.identity, parent.transform);
                var d = Instantiate(desk, position, Quaternion.identity, parentDesk.transform);

                toggle.onValueChanged.AddListener(delegate { ChangeDesk(toggle); });
                list_.Add(toggle, d);

            }
        */

    }
}