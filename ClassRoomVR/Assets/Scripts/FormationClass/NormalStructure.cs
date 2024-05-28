using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class NormalStructure : Structure
    {
        [SerializeField] Option rowsOpt; // UI option for setting the number of rows
        [SerializeField] Option coluOpt; // UI option for setting the number of columns
        private void Start()
        {
            numDesks.onValueChanged.AddListener(ChangeObjects);
            rowsOpt.onValueChanged.AddListener(ChangeRows);
            coluOpt.onValueChanged.AddListener(ChangeColumns);

        }

        public override void Set()
        {
            ControlMatrix();
            DeskManager.Instance.CreateRegularLayout(settings.NumDesks, settings.Rows, settings.Columns);
            //  toggleDesks.CreateToggles(DeskManager.Instance.GetDeskPosition(), DeskManager.Instance.GetDesks());
        }

        private void OnEnable()
        {
            settings = GameManager.Instance.GetCurrentSettings(); // Get the current classroom settings
            // Initialize UI options and settings
            numDesks.SetMax(MaxDesk());
            settings.NumDesks = settings.NumStudents;
            settings.Columns = Mathf.Min((int)coluOpt.GetMax(), Mathf.CeilToInt(Mathf.Sqrt(settings.NumDesks))); ;
            settings.Rows = Mathf.CeilToInt(settings.NumDesks / (float)settings.Columns);
            if (numDesks)
                numDesks.SetValueMin(settings.NumStudents);
            if (rowsOpt)
                rowsOpt.SetValueMin(settings.Rows);
            if (coluOpt)
                coluOpt.SetValueMin(settings.Columns);

            Set();
        }

        private void ControlMatrix()
        {
            if (settings.NumDesks > settings.Columns * settings.Rows)
            {
                if (coluOpt.GetMax() == settings.Columns || settings.Columns > settings.Rows)
                {
                    settings.Rows++;
                    rowsOpt.SetValue(settings.Rows);

                }
                else
                {
                    settings.Columns++;
                    coluOpt.SetValue(settings.Columns);
                }
            }
            else if (settings.NumDesks <= (settings.Columns - 1) * settings.Rows)
            {
                if (coluOpt.GetMin() == settings.Columns || settings.Columns < settings.Rows)
                {
                    settings.Rows--;
                    rowsOpt.SetValue(settings.Rows);

                }
                else
                {
                    settings.Columns--;
                    coluOpt.SetValue(settings.Columns);

                }
            }
        }

        // Method to handle changing the number of rows
        void ChangeRows(float value)
        {
            settings.Rows = (int)value;
            ChangeControl();
            Set();
        }

        // Method to handle changing the number of columns
        void ChangeColumns(float value)
        {
            settings.Columns = (int)value;
            ChangeControl();
            Set();
        }
       
        void ChangeControl()
        {
            int res = (settings.Columns * settings.Rows) - settings.NumDesks;
            if (res != 0)
            {
                settings.NumDesks += res;
                numDesks.SetValue(settings.NumDesks);
            }
        }

        public override int MaxDesk()
        {
            Renderer boxCollider = DeskManager.Instance.GetDeskCollider();
            // Calcular las dimensiones reales de la silla teniendo en cuenta la escala
            Vector3 sillaDimensions = Vector3.Scale(boxCollider.bounds.size, boxCollider.transform.lossyScale);
            Vector3 aulaDimensions = DeskManager.Instance.GetComponent<BoxCollider>().size;
            int numColumnas = Mathf.RoundToInt(aulaDimensions.x / (sillaDimensions.x * DeskManager.Instance.deskOffsetX));
            //En z la mesa ocupa el doble porque se suma la silla
            int numFilas = Mathf.RoundToInt(aulaDimensions.z / (sillaDimensions.z * 2 * DeskManager.Instance.deskOffsetZ));
            if (coluOpt != null) coluOpt.SetMax(numColumnas);
            if (rowsOpt != null) rowsOpt.SetMax(numFilas);
            Debug.Log(numFilas + " " + numColumnas);
            return numFilas * numColumnas; 

        }
    }
}
