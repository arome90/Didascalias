using ClassRoomVR;
using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassEventManager : MonoBehaviour
{
    private HashSet<ClassEventListener> listeners;

    private List<ClassEvent> events;

    public ClassEventManager()
    {
        listeners = new HashSet<ClassEventListener>();
        events = new List<ClassEvent>();
    }

    public void Start()
    {
        foreach (var s in ClassManager.Instance.getStudents()) { 
            s.Value.gameObject.GetComponent<ClassEventListener>().setManager(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (ClassEvent classEvent in events)
        {
            processEvent(classEvent);
        }
        events.Clear();
    }

    public void Subscribe(ClassEventListener listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    public void Unsubscribe(ClassEventListener listener)
    {
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
        }
    }

    public void sendEvent(ClassEvent classEvent, bool delay = false)
    {
        if (delay) events.Add(classEvent);
        else processEvent(classEvent);

    }

    private void processEvent(ClassEvent classEvent)
    {
        foreach (ClassEventListener listener in listeners)
        {
            listener.OnEventReceived(classEvent);
        }
    }
}