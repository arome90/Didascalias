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
                renderers[i].material.color = Color.green;
            }
            else if (inputActions[i].action.WasReleasedThisFrame())
            {
                renderers[i].material.color = Color.white;

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


    public Vector2 ThumbStickVector()
    {
        return inputActions[inputActions.Count - 1].action.ReadValue<Vector2>();
    }
}