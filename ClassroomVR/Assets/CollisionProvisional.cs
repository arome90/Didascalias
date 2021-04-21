using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClassRoomVR
{
    public class CollisionProvisional : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            GameManager.Instance._sceneManager.setCollision(other.GetComponentInParent<Transform>().gameObject.name);
        }
    }
}