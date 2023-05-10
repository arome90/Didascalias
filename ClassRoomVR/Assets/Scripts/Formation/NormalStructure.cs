using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class NormalStructure : Structure
    {

        [SerializeField] Option rowsOpt;
        [SerializeField] Option coluOpt;
        public override void Set()
        {

            if (parent != null) { Destroy(parent); }
            parent = new GameObject("Toggles");
            parent.transform.SetParent(transform,false);

            foreach (Transform child in parentDesk.transform)
            {
                Destroy(child.gameObject);
            }

            int n = settings.numDesks;
            for (int i = 0; i < settings.rows; i++)
            {
                for (int j = 0; j < settings.columns ; j++)
                {
                    if (n == 0) return;

                    
                    float xPos = j - (settings.columns - 1) / 2f;
                    float zPos = - i + (settings.rows - 1) / 2f;

                    
                    Vector3 position = new Vector3(xPos/5f, zPos/5f);
                    position += parent.transform.position;
                    var toggle=Instantiate(prefab,position,Quaternion.identity,parent.transform);


                    position = new Vector3(xPos, 0, zPos);
                    position += parentDesk.transform.position ;
                    var d = Instantiate(desk, position, Quaternion.identity, parentDesk.transform);


                    toggle.onValueChanged.AddListener(delegate { ChangeDesk(toggle); });
                    list_.Add(toggle, d);

                    n--;
                }
            }
        }







        void ChangeDesk(Toggle toggle)
        {
            list_[toggle].gameObject.SetActive(toggle.isOn);
        }



        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChangeColumns(settings.columns - 1f);
                coluOpt.SetValue(settings.columns);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChangeColumns(settings.columns + 1f);
                coluOpt.SetValue(settings.columns);

            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeRows(settings.rows - 1f);
                rowsOpt.SetValue(settings.rows);

            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                ChangeRows(settings.rows + 1f);
                rowsOpt.SetValue(settings.rows);

            } if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ChangeObjects(settings.numDesks - 1);
                numDesks.SetValue(settings.numDesks);

            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                ChangeObjects(settings.numDesks + 1);
                numDesks.SetValue(settings.numDesks);

            }
        }

        private void OnEnable()
        {
            rowsOpt.onValueChanged.AddListener(ChangeRows);
            coluOpt.onValueChanged.AddListener(ChangeColumns);
            rowsOpt.SetValue(settings.rows);
            coluOpt.SetValue(settings.columns);
            Set();

        }

        void ChangeRows(float value)
        {
            settings.rows = (int)value;
            Set();
        }

        void ChangeColumns(float value)
        {
            settings.columns =(int) value;
            Set();
        }


    
    }
}