using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class Player : MonoBehaviour
    {
        private void Awake()
        {
            GameManager.Instance.SetPlayer(transform.GetChild(0).gameObject);

        }
    }
}
