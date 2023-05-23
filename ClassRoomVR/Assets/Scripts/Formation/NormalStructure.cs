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
            parent.transform.SetParent(transform, false);

            foreach (Transform child in parentDesk.transform)
            {
                Destroy(child.gameObject);
            }

            int n = settings.NumDesks;
            for (int i = 0; i < settings.Rows; i++)
            {
                for (int j = 0; j < settings.Columns; j++)
                {
                    if (n == 0) return;

                    float xPos = j - (settings.Columns - 1) / 2f;
                    float zPos = -i + (settings.Rows - 1) / 2f;

                    Vector3 position = new Vector3(xPos / 5f, zPos / 5f);
                    position += parent.transform.position;
                    var toggle = Instantiate(prefab, position, Quaternion.identity, parent.transform);

                    position = new Vector3(xPos, 0, zPos);
                    position += parentDesk.transform.position;
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

        void ChangeRows(double value)
        {
            settings.Rows = (int)value;
            Set();
        }

        void ChangeColumns(double value)
        {
            settings.Columns = (int)value;
            Set();
        }
    }
}
