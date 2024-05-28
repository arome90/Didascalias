using BehaviorDesigner.Runtime.Tasks;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class DeskManager : GenericSingleton<DeskManager>
    {
        [SerializeField] Desk deskPrefab;
        private List<Vector2> deskPositions;
        private List<Desk> desks;

        public List<Vector2> GetDeskPosition() => deskPositions;
        public List<Desk> GetDesks() => desks;
        public override void Awake()
        {
            deskPositions = new List<Vector2>();
            desks = new List<Desk>();
        }
        public void GetFreeDesk(ref int deskPosition, int numGroups)
        {
            for (int i = deskPosition; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                {
                    deskPosition = i;
                    return;
                }
            }
        }

        public float deskOffsetX, deskOffsetZ, deskOffsetO;
        public void CreateRegularLayout(int numDesks, int numRows, int numColumns)
        {
            deskPositions.Clear();
            desks.Clear();
            DestroyChildren();
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
                    deskPositions.Add(new Vector2(xPos, zPos));
                    desks.Add(Instantiate(deskPrefab, new Vector3(transform.position.x + xPos * deskOffsetX, transform.position.y, transform.position.z + zPos * deskOffsetZ), Quaternion.identity, transform));
                    numDesks--;
                }
            }
        }

        public void CreateCircle(int numDesks, float radius, float degrees = 180f)
        {
            deskPositions.Clear();
            desks.Clear();
            DestroyChildren();
            float angle = degrees / (numDesks - (degrees > 180 ? 0 : 1));
            for (int i = 0; i < numDesks; i++)
            {
                float xPos = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius;
                float zPos = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius;
                deskPositions.Add(new Vector2(xPos, zPos));
                Vector3 position = new Vector3(xPos, 0, -zPos) + transform.position;
                Desk desk = Instantiate(deskPrefab, position, Quaternion.identity, transform);
                if (degrees > 180)
                {
                    desk.transform.LookAt(transform.position);
                }
                desks.Add(desk);
            }
        }
        public void DestroyChildren()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
        public void DestroyInactiveChildObjects()
        {

            foreach (Transform child in transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    Destroy(child.gameObject); // Destroy inactive child objects under parentDesk
                }
            }

        }
        public Renderer GetDeskCollider() 
        {
            return deskPrefab.transform.GetChild(1).GetChild(0).GetComponent<SkinnedMeshRenderer>();
        }
    }
}
