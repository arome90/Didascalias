using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClassRoomVR
{
    public class ChooserScript : MonoBehaviour
    {

        // Start is called before the first frame update
        void Start()
        {

        }

        public void pushedButton(int i)
        {
            GameManager.Instance.makeChoice(i);
        }
    }
}