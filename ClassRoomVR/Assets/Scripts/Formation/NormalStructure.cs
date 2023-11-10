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

        public float espacioEntreCol = 1.4f;  // Espacio entre sillas
        public float espacioEntreRow = 1.4f;  // Espacio entre sillas
                                                  // public float espacioPrimeraFila = 3f;

        public override void Set()
        {
            // Destroy previous parent objects
            Destroy(parent);
            parent = new GameObject("Toggles");
            parent.transform.SetParent(parent2.transform, false);

            foreach (Transform child in parentDesk.transform)
            {
                Destroy(child.gameObject);
            }

            int numDesks = settings.NumDesks;
            int numRows = settings.Rows;
            int numColumns = settings.Columns;

            Vector3 startPos = parent.transform.position;
            Vector3 startDeskPos = parentDesk.transform.position;

            float deskSpacing = 1.0f / 5.0f; // Spacing between desks
            for (int i = 0; i < numRows; i++)
            {

                var queue = new Queue<int>(hallways);
                for (int j = 0; j < numColumns; j++)
                {
                    if (numDesks == 0)
                    {
                        return; // If there are no more desks to place, exit the loop
                    }
                    if (queue.Count > 0 && queue.Peek() == j)
                    {
                        queue.Dequeue();
                        continue;
                    }


                    float xPos = j - (numColumns - 1) / 2f;
                    float zPos = -i + (numRows - 1) / 2f;

                    Vector3 position = startPos + new Vector3(xPos * deskSpacing, zPos * deskSpacing, 0);
                    Toggle toggle = null;
                    if (prefab != null)
                    {
                         toggle = Instantiate(prefab, position, Quaternion.identity, parent.transform);
                    }

                    position = startDeskPos + new Vector3(xPos * espacioEntreCol, 0, zPos * espacioEntreRow );
                    var d = Instantiate(desk, parentDesk.transform);
                    d.transform.position = position;
                    
                    //position = startDeskPos + new Vector3(xPos * espacioEntreCol, 0, zPos * espacioEntreRow );
                    //var d = Instantiate(desk, parentDesk.transform);
                    //d.transform.position = position;

                    if (toggle != null)
                    {
                        toggle.onValueChanged.AddListener(delegate { ChangeDesk(toggle); });
                        toggleToDeskMap.Add(toggle, d);
                    }

                    numDesks--;
                }
            }
        }

        // Method to handle toggling the visibility of desks based on toggle state
        void ChangeDesk(Toggle toggle)
        {
            toggleToDeskMap[toggle].gameObject.SetActive(toggle.isOn);
        }

        private void Start()
        {
            if (rowsOpt != null && coluOpt != null)
            {
                rowsOpt.onValueChanged.AddListener(ChangeRows);
                coluOpt.onValueChanged.AddListener(ChangeColumns);
            }
        }

        private void OnEnable()
        {
            // Initialize UI options and settings
            settings.NumDesks = Mathf.Max(settings.NumStudents,settings.NumDesks);
            if (numDesks != null)
            {
                numDesks.SetValue(settings.NumStudents);
                numDesks.SetMin(settings.NumStudents);
            }
            if (rowsOpt)
            {
                rowsOpt.SetValue(settings.Rows);
            }
            if (coluOpt)
            {
                coluOpt.SetValue(settings.Columns);
            }
            GetHole();
            Set();
        }

        // Method to handle changing the number of rows
        void ChangeRows(float value)
        {
            settings.Rows = (int)value;
            Set();
        }

        // Method to handle changing the number of columns
        void ChangeColumns(float value)
        {
            settings.Columns = (int)value;
            Set();
        }
        List<int> hallways;
        void GetHole()
        {
            hallways = new List<int>();
            switch (GameManager.Instance.GetCurrentSettings().StructureMode)
            {
                case StructureMode.UnPasillo:
                    hallways.Add(2);
                    break;
                case StructureMode.DosPasillos:
                    hallways.AddRange(new int[] { 1, 3 });
                    break;
            }

        }


        public override void MaxDesk() 
        { 
            // Obtener el BoxCollider del prefab de la silla (o escritorio)
            BoxCollider boxCollider = desk.GetComponent<BoxCollider>();
            // Calcular las dimensiones reales de la silla teniendo en cuenta la escala
            Vector3 sillaDimensions = Vector3.Scale(boxCollider.size, desk.transform.lossyScale);
            Vector3 aulaDimensions = aula.size;
            int numColumnas = Mathf.FloorToInt(aulaDimensions.x  / (sillaDimensions.x * espacioEntreCol)) ;
            int numFilas = Mathf.FloorToInt(aulaDimensions.z / (sillaDimensions.z * espacioEntreRow));
            if (coluOpt != null) coluOpt.SetMax(numColumnas);
            //else settings.Columns = numColumnas;
            if (rowsOpt != null) rowsOpt.SetMax(numFilas);
            //else settings.Rows = numFilas;

            Debug.Log("F " + numFilas + " C " + numColumnas);
        
        }
    }
}
