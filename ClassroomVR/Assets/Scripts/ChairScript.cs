using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChairScript : MonoBehaviour
{
    public UnityEvent stopMovingEvent;

    private bool entered = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Ha entrado " + other.gameObject.name);
        if (other.tag != Constants.PLAYER_TAG && !entered)
        {
            entered = true;
            stopMovingEvent.Invoke();
        }
    }
}
