using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desk : MonoBehaviour
{
    Vector2 pos;
    bool ocupado;


    public bool Ocupado { get { return ocupado; } set { ocupado = value; } }
    public Vector2 Pos { get { return pos; } set { pos = value; } }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Desk")) 
        {
            Debug.Log("tocar");
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Desk"))
        {
            Debug.Log("tocat");
        }
    }
   
}
