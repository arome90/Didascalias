using System.Collections;
using UnityEngine;

namespace ClassRoomVR
{
    public class DeskManager : MonoBehaviour
    {
        private static DeskManager _instance;
        private static ClassSettings sett;
        [SerializeField]  Desk prefabDesk;
        public static DeskManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                DontDestroyOnLoad(this);
            }
            else
            {
                _instance = this;
                sett = GameManager.Instance.Settings;

            }
        }


        

        public void getFreeDesk(ref int deskPos, int ngrupos)
        {
            // Ordenamiento por grupos
            if (ngrupos > 1)
            {
                if (deskPos == 2 || deskPos == 7 || deskPos == 12 || deskPos == 17 || deskPos == 22 || deskPos == 27)
                    deskPos++;
                if (deskPos == 10 || deskPos == 11 || deskPos == 12 || deskPos == 13 || deskPos == 14)
                    deskPos = 15;
            }
        }



        public void CreateDesks()
        {
            int F = sett.rows;
            int C = sett.columns;
            for (int i = 0; i < F; i++)
            {
                for (int j = 0; j < C; j++)
                {
                    float xPos = j - (C - 1) / 2f;
                    float zPos = -i + (F - 1) / 2f;
                    Instantiate(prefabDesk, new Vector3(transform.position.x + xPos * 1.4f, transform.position.y, transform.position.z + zPos * 2f), new Quaternion(), transform);

                }
            }

        }
        public void CreateO()
        {

            int num = sett.numDesks;
            float grados = sett.grades;
            float radius = sett.radius;

            float angle = grados / num; // ángulo entre los objetos
          
            for (int i = 0; i < num; i++)
            {
                float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius; // coordenada x
                float z = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius; // coordenada z
                Vector3 position = new Vector3(x, 0,- z); // posición del objeto
                position += transform.position;
                Desk desk = Instantiate(prefabDesk, position, Quaternion.identity, transform); // instanciar objeto
                desk.Pos = new Vector2(i, 0);
                desk.transform.LookAt(transform.position);

                //if (allign != Allign.None) // alinear objetos al centro de la circunferencia
                //{
                //    desk.transform.LookAt(transform.position + new Vector3(0, 0, 10) * Convert.ToInt32(allign == Allign.Frente));
                //}
                // desks_.Add(desk);
            }


        }



        public void CreateU()
        {

            int num = sett.numDesks;
            float radius = sett.radius;

            float angle = 180f/  num; // ángulo entre los objetos

            for (int i = 0; i < num; i++)
            {
                float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius; // coordenada x
                float z = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius; // coordenada z
                Vector3 position = new Vector3(x, 0, -z); // posición del objeto
                position += transform.position;
                Desk desk = Instantiate(prefabDesk, position, Quaternion.identity, transform); // instanciar objeto
                                                                                               // desk.transform.SetParent(gam.transform, true);
                desk.Pos = new Vector2(i, 0);

            }


        }




    }
}