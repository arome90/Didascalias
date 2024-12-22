using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassEventListener : MonoBehaviour
{
    protected ClassEventManager eventManager;
    public void OnEventReceived(ClassEvent classEvent)
    {
        classEvent.Execute(gameObject);
    }

    public void setManager(ClassEventManager classEventManager)
    {
        eventManager = classEventManager;
    }

    public void sendEvent(ClassEvent classEvent, bool delay=false)
    {
        eventManager.sendEvent(classEvent, delay);
    }
    
}

