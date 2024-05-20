using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class DeskManager : GenericSingleton<DeskManager>
    {
        [SerializeField] Desk deskPrefab;
        private List<Vector2> deskPositions;

        public List<Vector2> GetDeskPosition() => deskPositions;
        private void Start()
        {
            deskPositions = new List<Vector2>();
        }
        public void GetFreeDesk(ref int deskPosition, int numGroups)
        {
           for(int i = deskPosition; i < transform.childCount; i++) 
           {
                if (transform.GetChild(i).gameObject.activeSelf) 
                {
                    deskPosition = i;
                    return;
                }
           }
        }
        public void CreateRegularLayout(int numDesks,int numRows, int numColumns, float deskOffsetX, float deskOffsetZ)
        {
            deskPositions.Clear();
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if (numDesks == 0)
                    {
                        return; // If there are no more desks to place, exit the loop
                    }
                    float xPos = j - (numColumns - 1) / 2f;
                    float zPos = -i + (numRows - 1) / 2f;
                    deskPositions.Add(new Vector2(xPos, zPos));
                    Instantiate(deskPrefab, new Vector3(transform.position.x + xPos * deskOffsetX, transform.position.y, transform.position.z + zPos * deskOffsetZ), Quaternion.identity, transform);
                    numDesks--;
                }
            }
        }

        public void CreateCircle(int numDesks, float radius, float degrees)
        {
            deskPositions.Clear();
            float angle = degrees / numDesks;
            for (int i = 0; i < numDesks; i++)
            {
                float xPos = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius;
                float zPos = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius;
                deskPositions.Add(new Vector2(xPos, zPos));
                Vector3 position = new Vector3(xPos, 0, -zPos) + transform.position;
                Desk desk = Instantiate(deskPrefab, position, Quaternion.identity, transform);
                //desk.Position = new Vector2(i, 0);
                desk.transform.LookAt(transform.position);
            }
        }

        public void CreateUShape(int numDesks, float radius)
        {
            deskPositions.Clear();
            float angle = 360f / (numDesks-1);
            float currentAngle = 0f;
            for (int i = 0; i < numDesks; i++)
            {
                float xPos = Mathf.Cos(Mathf.Deg2Rad / 2f * currentAngle) * radius;
                float zPos = Mathf.Sin(Mathf.Deg2Rad / 2f * currentAngle) * radius;
                deskPositions.Add(new Vector2(xPos, zPos));
                Vector3 position = new Vector3(xPos, 0, -zPos) + transform.position;
                Instantiate(deskPrefab, position, Quaternion.identity, transform);
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
