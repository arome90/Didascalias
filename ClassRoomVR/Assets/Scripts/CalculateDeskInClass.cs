//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CalculateDeskInClass : MonoBehaviour
//{
//    [SerializeField] GameObject desk;
//    [SerializeField] BoxCollider aula;
//    public float espacioEntreSillas = 1.4f;  // Espacio entre sillas
//    public float espacioEntreParedYSilla = 0.2f;  // Espacio entre sillas y paredes
//    public float espacioPrimeraFila=3f;

//    void Start()
//    {
//        CalcularSillas();
//    }

//    void CalcularSillas()
//    {   // Obtener el BoxCollider del prefab
//        BoxCollider boxCollider = desk.GetComponent<BoxCollider>();

//        Vector3 sillaDimensions = Vector3.Scale(boxCollider.size,desk.transform.lossyScale);
//        Vector3 aulaDimensions = aula.size;
//        float anchoDisponible = aulaDimensions.x - 2 * espacioEntreParedYSilla;
//        float profundidadDisponible = aulaDimensions.z - espacioPrimeraFila;
//        int numColumnas = Mathf.FloorToInt((anchoDisponible - sillaDimensions.x) / (sillaDimensions.x + espacioEntreSillas  )) + 1;
//        int numFilas = Mathf.FloorToInt((profundidadDisponible - sillaDimensions.z) / (sillaDimensions.z + espacioEntreSillas)) + 1;
       

//        Debug.Log("Número máximo de filas: " + numFilas);
//        Debug.Log("Número máximo de columnas: " + numColumnas);
//      //  Debug.Log("Número total de sillas: " + (numFilas * numColumnas));
//    }
//}

