using UnityEngine;

namespace ClassRoomVR
{
    public class DeskManager : MonoBehaviour
    {
        private static DeskManager instance;
        [SerializeField] Desk deskPrefab;

        public static DeskManager Instance { get { return instance; } }

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                DontDestroyOnLoad(this);
            }
            else
            {
                instance = this;
            }
        }

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

            int numRows = settings.Rows;
            int numColumns = settings.Columns;

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    float xPos = j - (numColumns - 1) / 2f;
                    float zPos = -i + (numRows - 1) / 2f;
                    Instantiate(deskPrefab, new Vector3(transform.position.x + xPos * 1.4f, transform.position.y, transform.position.z + zPos * 2f), Quaternion.identity, transform);
                }
            }
        }

        public void CreateCircle()
        {
            ClassSettings settings = GameManager.Instance.GetCurrentSettings();

            int numDesks = settings.NumDesks;
            float degrees = (float)settings.Degrees;
            float radius = (float)settings.Radius;

            float angle = degrees / numDesks;

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

            int numDesks = settings.NumDesks;
            float radius = (float)settings.Radius;

            float angle = 360 / (numDesks-1);
            float currentAngle = 0f;

            for (int i = 0; i < numDesks; i++)
            {
                float x = Mathf.Cos(Mathf.Deg2Rad/2 * currentAngle ) * radius;
                float z = Mathf.Sin(Mathf.Deg2Rad/2* currentAngle ) * radius;
                Vector3 position = new Vector3(x, 0, -z) + transform.position;

                Desk desk = Instantiate(deskPrefab, position, Quaternion.identity, transform);
                desk.Position = new Vector2(i, 0);
                currentAngle += angle;
            }
        }
    }
}
