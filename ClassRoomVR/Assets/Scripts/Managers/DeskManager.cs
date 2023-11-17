using UnityEngine;

namespace ClassRoomVR
{
    public class DeskManager : GenericSingleton<DeskManager>
    {
        [SerializeField] Desk deskPrefab;

        public void GetFreeDesk(ref int deskPosition, int numGroups)
        {
            // Ordenamiento por grupos
            //if (numGroups > 1)
            //{
            //    if (deskPosition == 2 || deskPosition == 7 || deskPosition == 12 || deskPosition == 17 || deskPosition == 22 || deskPosition == 27)
            //        deskPosition++;
            //    if (deskPosition == 10 || deskPosition == 11 || deskPosition == 12 || deskPosition == 13 || deskPosition == 14)
            //        deskPosition = 15;
            //}
           for(int i = deskPosition; i < transform.childCount; i++) 
           {
                if (transform.GetChild(i).gameObject.activeSelf) 
                {
                    deskPosition = i;
                    return;
                }
           }
        }

        public void CreateDesks()
        {
            ClassSettings settings = GameManager.Instance.GetCurrentSettings();
            CreateRegularLayout(settings, settings.Rows, settings.Columns, 1.3f, 1.5f);
        }

        private void CreateRegularLayout(ClassSettings settings, int numRows, int numColumns, float deskOffsetX, float deskOffsetZ)
        {
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    float xPos = j - (numColumns - 1) / 2f;
                    float zPos = -i + (numRows - 1) / 2f;
                    Instantiate(deskPrefab, new Vector3(transform.position.x + xPos * deskOffsetX, transform.position.y, transform.position.z + zPos * deskOffsetZ), Quaternion.identity, transform);
                }
            }
        }

        public void CreateCircle()
        {
            ClassSettings settings = GameManager.Instance.GetCurrentSettings();
            int numDesks = settings.NumStudents;
            float radius = 3.4f;
            float angle = 360f / numDesks;

            for (int i = 0; i < numDesks; i++)
            {
                float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius;
                float z = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius;
                Vector3 position = new Vector3(x, 0, -z) + transform.position;
                Desk desk = Instantiate(deskPrefab, position, Quaternion.identity, transform);
                desk.Position = new Vector2(i, 0);
                desk.transform.LookAt(transform.position);
            }
        }

        public void CreateUShape()
        {
            ClassSettings settings = GameManager.Instance.GetCurrentSettings();
            int numDesks = settings.NumStudents;
            float radius = 3;
            float angle = 360f / (numDesks-1);
            float currentAngle = 0f;

            for (int i = 0; i < numDesks; i++)
            {
                float x = Mathf.Cos(Mathf.Deg2Rad / 2f * currentAngle) * radius;
                float z = Mathf.Sin(Mathf.Deg2Rad / 2f * currentAngle) * radius;
                Vector3 position = new Vector3(x, 0, -z) + transform.position;

                Desk desk = Instantiate(deskPrefab, position, Quaternion.identity, transform);
                desk.Position = new Vector2(i, 0);
                currentAngle += angle;
            }
        }

        public void DestroyChildren() 
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}
