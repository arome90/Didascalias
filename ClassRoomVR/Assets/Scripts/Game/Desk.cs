using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Playables;
using Utilities.Extensions;

public class Desk : MonoBehaviour
{
    private bool isOccupied; // Flag to indicate if the desk is occupied
    public bool IsOccupied { get => isOccupied; set => isOccupied = value; } // Property to access the occupancy status

    private Vector2 position;
    private Vector2 Position { get => position; set => position = value; }

    // Get the position of the student sitting at the desk
    public Vector3 GetPositionStudent() => transform.GetChild(0).position + new Vector3(0, 0, 0.05f);

    [HideInInspector] public UnityEngine.Events.UnityEvent onCollisionChanged; // Event invoked when collision with other desk occurs
    [SerializeField] Animation deskAnim;
    [SerializeField] Animation chairAnim;

    [SerializeField] NavMeshObstacle chairObstacle;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Desk"))
        {
            onCollisionChanged.Invoke(); // Invoke the onCollisionChanged event when colliding with another desk
        }
    }
    List<string> clipDeskNames;
    private void Start()
    {
        clipDeskNames = new List<string>();
        if (deskAnim != null)
        {
            foreach (AnimationState animationState in deskAnim)
            {
                clipDeskNames.Add(animationState.name);
            }
        }
    }
    public void PlayAnimacionMesa(Animaciones anim)
    {
        if (deskAnim != null)
        {
            //deskAnim.clip = chairAnim.GetClip("EmpujarMesa");
            deskAnim.Play(clipDeskNames[(int)anim]);
        }
    }

    public void Balancearse()
    {
        if (chairAnim != null)
        {
            chairAnim.Play();
        }
    }

    public void SetChair(bool active)
    {
        if (chairObstacle != null)
        {
            chairObstacle.SetActive(active);
        }
    }
}
