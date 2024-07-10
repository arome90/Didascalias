using ClassRoomVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VisualController : MonoBehaviour
{

    [SerializeField] List<MeshRenderer> renderers;
    [SerializeField] GameObject thumbStick;
    MeshRenderer thumbStickRenderer;
    enum Hand { Left, Right }
    [SerializeField] Hand hand;
    [SerializeField] List<InputActionReference> inputActions;

    public List<InputActionReference> InputActions { get => inputActions; }

    private void Start()
    {
        thumbStickRenderer = thumbStick.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            if (inputActions[i].action.WasPerformedThisFrame())
            {
                if (objetive == i)
                {
                    objetive = -1;
                }
                renderers[i].material.color = Color.green;
            }
            else if (inputActions[i].action.WasReleasedThisFrame())
            {
                if (objetive != i) renderers[i].material.color = Color.white;

            }
        }

        if (inputActions[inputActions.Count - 1].action.WasPerformedThisFrame())
        {
            thumbStickRenderer.material.color = Color.green;
        }
        else if (inputActions[inputActions.Count - 1].action.WasReleasedThisFrame())
        {
            thumbStickRenderer.material.color = Color.white;

        }
        Vector2 thumb = ThumbStickVector();
        float x = Unity.Mathematics.math.remap(-1f, 1f, -30f, 30f, thumb.y);
        float z = Unity.Mathematics.math.remap(-1f, 1f, -30f, 30f, thumb.x);
        thumbStick.transform.localRotation = Quaternion.Euler(-x, 0, z);
    }

    int objetive = -1;
    public void SetRed(VisualAction action)
    {
        renderers[(int)action].material.color = Color.red;
        objetive = (int)action;
    }
    public void CleanRed(VisualAction action) 
    {
        renderers[(int)action].material.color = Color.white;
        objetive = -1;
    }
    public Vector2 ThumbStickVector()
    {
        return inputActions[inputActions.Count - 1].action.ReadValue<Vector2>();
    }
  

   
}