//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//namespace ClassRoomVR
//{
//    public class CollisionProvisional : MonoBehaviour
//    {
//        private void OnTriggerEnter(Collider other)
//        {
//            Debug.Log("Collision con " + other.GetComponentInParent<Transform>().gameObject.name);
//           // GameManager.Instance._sceneManager.setCollision(other.GetComponentInParent<Transform>().gameObject.name);
//        }

//        void OnCollisionEnter(Collision other)
//        {
//            Debug.Log(other.gameObject.name);
//            //Debug.Log("Collision con " + other.GetComponentInParent<Transform>().gameObject.name);
//            //GameManager.Instance._sceneManager.setCollision(other.GetComponentInParent<Transform>().gameObject.name);
//        }
//    }
//}