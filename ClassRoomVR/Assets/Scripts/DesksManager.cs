using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DesksManager : MonoBehaviour
{
    //TODO : generar los pupitres desde codigo 
    // generar una matriz y luego rellenar la forma deseada 
    //  ¡ . . . ¡  
    //  ¡ . . . ¡ 
    //  ¡ . . . ¡
    //  ¡ ¡ ¡ ¡ ¡
    //  
    //  ¡ ¡ . ¡ ¡ 
    //  ¡ ¡ . ¡ ¡
    //  . . . . .
    //  ¡ ¡ . ¡ ¡
    //  ¡ ¡ . ¡ ¡

    enum SeatingArrangement { Fila, U };
    int desksNum;
    [SerializeField] int groups;
    //de momento un booleano para saber si esta ocupada o no la silla
    List<int> freeDesks;
    [SerializeField] int F;
    [SerializeField] int C;
    // Start is called before the first frame update
    void Start()
    {
        desksNum = transform.childCount;
        freeDesks = Enumerable.Range(0,desksNum/*+1*/).ToList();
        //for(int i = 0; i < F; i++)
        //{
        //    for (int j = 0; j < C; j++) 
        //    {

        //    }
        //}
    }


    public int getFreeDesk()
    {
        int f = freeDesks.First();
        freeDesks.RemoveAt(0);
        return f;
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

    public int GetNearDeskRandom(int pos,int nStu) 
    {
        //do
        //int randomDirection = UnityEngine.Random.Range(0, 4); // genera un número aleatorio entre 0 y 3
        //Vector2 direction = Vector2.zero;
        //switch (randomDirection)
        //{
        //    case 0:
        //        direction = Vector2.left; // izquierda
        //        break;
        //    case 1:
        //        direction = Vector2.right; // derecha
        //        break;
        //    case 2:
        //        direction = Vector2.up; // arriba
        //        break;
        //    case 3:
        //        direction = Vector2.down; // abajo
        //        break;
        //}


        int a = -1;
        do
        {
            a = UnityEngine.Random.Range(0, 4);
            switch (a)
            {
                case 0:
                    a = 1;
                    break;
                case 1:
                    a = -1;
                    break;
                case 2:
                    a = 5;
                    break;
                case 3:
                    a = -5;
                    break;
                default:
                    break;
            }
        } while (pos + a >nStu - 1 || pos + a < 0);


        return pos + a;

    }

   // public int GetDesk(int x ,int y) { }

    //public List<List<int>> SeparateIntoGroups(int[,] matrix, int numStudentsPerGroup)
    //{
    //    List<List<int>> groups = new List<List<int>>();
    //    int numGroups = (int)Math.Ceiling((double)(matrix.Length) / numStudentsPerGroup);

    //    for (int i = 0; i < numGroups; i++)
    //    {
    //        groups.Add(new List<int>());
    //    }

    //    int index = 0;
    //    for (int i = 0; i < matrix.GetLength(0); i++)
    //    {
    //        for (int j = 0; j < matrix.GetLength(1); j++)
    //        {
    //            groups[index % numGroups].Add(i * matrix.GetLength(1) + j);
    //            index++;
    //        }
    //    }

    //    return groups;
    //}
}
