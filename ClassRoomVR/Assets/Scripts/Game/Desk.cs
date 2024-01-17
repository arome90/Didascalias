using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Playables;

public class Desk : MonoBehaviour
{
    private bool isOccupied; // Flag to indicate if the desk is occupied
    [SerializeField] GenerateBackpack backpack;
    public bool IsOccupied { get => isOccupied; set => isOccupied = value; } // Property to access the occupancy status

    // Get the position of the student sitting at the desk
    public Vector3 GetPositionStudent() => transform.GetChild(0).position;
    public Vector3 GetPositionSitStudent() => transform.GetChild(1).position;

    [HideInInspector] public UnityEngine.Events.UnityEvent onCollisionChanged; // Event invoked when collision with other desk occurs
    [SerializeField] Animation deskAnim;
    [SerializeField] Animation chairAnim;
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


}
