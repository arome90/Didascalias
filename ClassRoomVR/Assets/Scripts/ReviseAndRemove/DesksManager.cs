//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//namespace ClassRoomVR
//{
//    public class DesksManager : MonoBehaviour
//    {
//        int desksNum;
//        [SerializeField] int groups;
//        //de momento un booleano para saber si esta ocupada o no la silla
//        List<int> freeDesks;
//         int F;
//         int C;
//        [SerializeField] bool sillasVacias;

//        [SerializeField] Desk prefabDesk;

//        [SerializeField] float spaceX;
//        [SerializeField] float spaceZ;

//        public int numObjects; // número de objetos a instanciar
//        public float radius; // radio de la circunferencia
//        public enum Allign {None,Centro, Frente };
//        public Allign allign;

//        [Range(-180f, 370f)] public float grados = 360.0f;


//        List<Desk> desks_;
//        // Start is called before the first frame update

//        public enum Direction
//        {
           
//            UP = 270,
//            DOWN = 90,
//            LEFT = 180,
//            RIGHT = 0
//        }

//        void Awake()
//        {
//            ClassSettings set= GameManager.Instance.Settings;
//            desksNum = transform.childCount;
//            F = set.rows;
//            C = set.columns;
//            freeDesks = Enumerable.Range(0, desksNum/*+1*/).ToList();
//            desks_ = new List<Desk>();
//            //Quitar en un futuro 
//            if (DeskManager.Instance.transform.childCount==0)
//            {
//                StructureMode a = set.StructureClass;
//                switch (a)
//                {
//                    case StructureMode.Fila:
//                        CreateDesks();
//                        break;
//                    case StructureMode.U:
//                        CreateU();
//                        break;
//                    case StructureMode.Circular:
//                        CreateO();
//                        break;
//                    case StructureMode.UnPasillo:
//                        CreatePasillo(2);
//                        break;
//                    case StructureMode.DosPasillos:
//                        CreatePasillo(1);
//                        break;

//                }
//            }
//            else 
//            {

//            }

//        }



//        public void CreateDesks()
//        {
           
//            for (int i = 0; i < F; i++)
//            {
//                for (int j = 0; j < C; j++)
//                {
//                    float xPos = j - (C - 1) / 2f;
//                    float zPos = -i + (F - 1) / 2f;
//                    Instantiate(prefabDesk, new Vector3(transform.position.x + xPos * spaceX, transform.position.y,transform.position.z+ zPos * spaceZ), new Quaternion(),transform);
                    
//                }
//            }

//        }
//        GameObject gam;
//        public void CreateO()
//        {
//            List<bool> l = GameManager.Instance.GetDeskFormation();
//            bool lista = l==null;
//            float angle = grados / numObjects; // ángulo entre los objetos
//            gam = new GameObject();
//            gam.transform.SetParent(transform, true);
//            for (int i = 0; i < numObjects; i++)
//            {
//                    float x = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius; // coordenada x
//                    float z = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius; // coordenada z
//                    Vector3 position = new Vector3(x, 0, z); // posición del objeto
//                    position += transform.position;
//                    Desk desk = Instantiate(prefabDesk, position, Quaternion.identity, transform); // instanciar objeto
//                    desk.transform.SetParent(gam.transform, true);
//                    desk.Pos = new Vector2(i, 0);
//                    if (allign != Allign.None) // alinear objetos al centro de la circunferencia
//                    {
//                        desk.transform.LookAt(transform.position + new Vector3(0, 0, 10) * Convert.ToInt32(allign == Allign.Frente));
//                    }
//                    desks_.Add(desk);
//            }

//        }

      

//        //private void Update()
//        //{

//        //    if (Input.GetKeyUp(KeyCode.Alpha9))
//        //    {
//        //        numObjects++;
//        //        Destroy(gam);
//        //        CreateO();
//        //        //Debug.Log(numObjects);

//        //    }
//        //    else if (Input.GetKeyUp(KeyCode.Alpha8))
//        //    {
//        //        numObjects--;
//        //        Destroy(gam);
//        //        CreateO();
//        //        //Debug.Log(numObjects);

//        //    }

//        //    if (Input.GetKeyUp(KeyCode.Alpha7))
//        //    {
//        //        radius += 0.01f;
//        //        Destroy(gam);
//        //        CreateO();
//        //       // Debug.Log(radius);

//        //    }
//        //    else if (Input.GetKeyUp(KeyCode.Alpha6))
//        //    {
//        //        radius -= 0.01f;
//        //        Destroy(gam);
//        //        CreateO();
//        //       // Debug.Log(radius);
//        //    }
//        //}

//        public void CreateU()
//        {
//            for (int i = 0; i < F; i++)
//            {
//                for (int j = 0; j < C; j++)
//                {

//                    if (j == 0 || j == C - 1 || i == F - 1)
//                    {

//                        Instantiate(prefabDesk, new Vector3(transform.position.x + j * spaceX, transform.position.y, transform.position.z - i * spaceZ), new Quaternion(), transform);
//                        continue;
//                    }
//                    // sillas += sillasVacias ? "-" : " ";
//                }
//                // sillas += "\n";
//            }
//        }


//        public void CreatePasillo(int sitiosJuntos)
//        {

//            int cont = 0;
//            for (int i = 0; i < F; i++)
//            {
//                for (int j = 0; j < C; j++)
//                {

//                    if (cont == sitiosJuntos)
//                    {
//                        //sillas += sillasVacias ? "-" : " ";
//                        cont = 0;
//                        continue;
//                    }
//                    Instantiate(prefabDesk, new Vector3(transform.position.x + j * spaceX, transform.position.y, transform.position.z - i * spaceZ), new Quaternion(), transform);
//                    cont++;
//                }
//                cont = 0;
//            }
//        }


//        public int getFreeDesk()
//        {
//            int f = freeDesks.First();
//            freeDesks.RemoveAt(0);
//            return f;
//        }

//        public void getFreeDesk(ref int deskPos, int ngrupos)
//        {
//            // Ordenamiento por grupos
//            if (ngrupos > 1)
//            {
//                if (deskPos == 2 || deskPos == 7 || deskPos == 12 || deskPos == 17 || deskPos == 22 || deskPos == 27)
//                    deskPos++;
//                if (deskPos == 10 || deskPos == 11 || deskPos == 12 || deskPos == 13 || deskPos == 14)
//                    deskPos = 15;
//            }
//        }

//        public int GetNearDeskRandom(int pos, int nStu)
//        {
//            //do
//            //int randomDirection = UnityEngine.Random.Range(0, 4); // genera un número aleatorio entre 0 y 3
//            //Vector2 direction = Vector2.zero;
//            //switch (randomDirection)
//            //{
//            //    case 0:
//            //        direction = Vector2.left; // izquierda
//            //        break;
//            //    case 1:
//            //        direction = Vector2.right; // derecha
//            //        break;
//            //    case 2:
//            //        direction = Vector2.up; // arriba
//            //        break;
//            //    case 3:
//            //        direction = Vector2.down; // abajo
//            //        break;
//            //}


//            //Vector2 vec = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

//            int a = -1;
//            do
//            {
//                a = UnityEngine.Random.Range(0, 4);
//                switch (a)
//                {
//                    case 0:
//                        a = 1;
//                        break;
//                    case 1:
//                        a = -1;
//                        break;
//                    case 2:
//                        a = 5;
//                        break;
//                    case 3:
//                        a = -5;
//                        break;
//                    default:
//                        break;
//                }
//            } while (pos + a > nStu - 1 || pos + a < 0);


//            return pos + a;

//        }

    
//    }
//}
