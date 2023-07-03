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
            //ifnull
            Destroy(parent);
            parent = new GameObject("Toggles");
            parent.transform.SetParent(transform, false);

            foreach (Transform child in parentDesk.transform)
            {
                Destroy(child.gameObject);
            }

            int numDesks = settings.NumDesks;
            int numRows = settings.Rows;
            int numColumns = settings.Columns;

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if (numDesks == 0)
                    {
                        return;
                    }

                    float xPos = j - (numColumns - 1) / 2f;
                    float zPos = -i + (numRows - 1) / 2f;

                    Vector3 position = new Vector3(xPos / 5f, zPos / 5f);
                    position += parent.transform.position;
                    var toggle = Instantiate(prefab, position, Quaternion.identity, parent.transform);

                    position = new Vector3(xPos, 0, zPos);
                    position += parentDesk.transform.position;
                    var d = Instantiate(desk, position, Quaternion.identity, parentDesk.transform);

                    toggle.onValueChanged.AddListener(delegate { ChangeDesk(toggle); });
                    toggleToDeskMap.Add(toggle, d);

                    numDesks--;
                }
            }
        }

        void ChangeDesk(Toggle toggle)
        {
            toggleToDeskMap[toggle].gameObject.SetActive(toggle.isOn);
        }

        private void Start()
        {
            rowsOpt.onValueChanged.AddListener(ChangeRows);
            coluOpt.onValueChanged.AddListener(ChangeColumns);
        }

        private void OnEnable()
        {
            settings.NumDesks = settings.NumStudents;
            numDesks.SetValue(settings.NumStudents);
            numDesks.SetMin(settings.NumStudents);

            rowsOpt.SetValue(settings.Rows);
            coluOpt.SetValue(settings.Columns);
            Set();
        }

        void ChangeRows(float value)
        {
            settings.Rows = (int)value;
            Set();
        }

        void ChangeColumns(float value)
        {
            settings.Columns = (int)value;
            Set();
        }
    }
}
