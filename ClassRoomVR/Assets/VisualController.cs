using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


public class VisualController : MonoBehaviour
{

    [SerializeField] List<MeshRenderer> renderers;
    [SerializeField] GameObject thumbStick;
    enum Hand { Left, Right }

    [SerializeField] Hand hand;
    InputFeatureUsage<bool>[] inputFeatureUsages = 
    {
        CommonUsages.menuButton,CommonUsages.triggerButton,CommonUsages.gripButton,CommonUsages.primaryButton,CommonUsages.secondaryButton
    };
    InputFeatureUsage<float>axis;


    void Start()
    {
        //var characteristics = InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        //StartCoroutine(RepeatGetDevice(_rightController, characteristics));

        //var characteristics = InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        //StartCoroutine(RepeatGetDevice( targetDevice, characteristics));

        foreach (var renderer in renderers)
        {
            renderer.material.color = Color.green;
        }

    }

    private IEnumerator RepeatGetDevice(InputDevice device, InputDeviceCharacteristics characteristics)
    {
        var devices = new List<InputDevice>();

        do
        {
            yield return null;
            InputDevices.GetDevicesWithCharacteristics(characteristics, devices);
            if (devices.Count > 0)
                device = devices[0];
        } while (devices.Count == 0);

        Debug.Log($"{device.name} : {device.characteristics}");
    }

    //public InputDevice targetDevice;
    public InputDevice _rightController;
    public InputDevice _leftController;

    private void Update()
    {
        if (!_rightController.isValid || !_leftController.isValid)
            InitializeInputDevices();
        else
        {
            bool button;
            for (int i = 0; i < inputFeatureUsages.Length; i++)
            {
                _leftController.TryGetFeatureValue(inputFeatureUsages[i], out button);
                if (button)
                {
                    renderers[i].material.color = Color.red;
                    Debug.Log("mano");


                }
                else Debug.Log("noooo");
                //else 
                //{
                //    renderers[1].material.color = Color.green;

                //}
            }
            Vector2 thumb;
            _leftController.TryReadAxis2DValue(InputHelpers.Axis2D.PrimaryAxis2D, out thumb);
            float x = Unity.Mathematics.math.remap(-1f, 1f, -30f, 30f, thumb.y);
            float z = Unity.Mathematics.math.remap(-1f, 1f, -30f, 30f, thumb.x);

            thumbStick.transform.rotation = Quaternion.Euler(x, 0,-z);
        }
    }


    
    private void InitializeInputDevices()
    {

        if (!_rightController.isValid)
            InitializeInputDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, ref _rightController);
        if (!_leftController.isValid)
            InitializeInputDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left, ref _leftController);
    
    }

    private void InitializeInputDevice(InputDeviceCharacteristics inputCharacteristics, ref InputDevice inputDevice)
    {
        List<InputDevice> devices = new List<InputDevice>();
        //Call InputDevices to see if it can find any devices with the characteristics we're looking for
        InputDevices.GetDevicesWithCharacteristics(inputCharacteristics, devices);

        //Our hands might not be active and so they will not be generated from the search.
        //We check if any devices are found here to avoid errors.
        if (devices.Count > 0)
        {
            inputDevice = devices[0];
        }
    }

    //public void ActiveAction(UnityEngine.InputSystem.InputAction.CallbackContext context)
    //{
    //    Debug.Log("bottttooon");
    //    renderers[1].material.color = Color.red;
    //}

    //public void SelectAction(InputAction.CallbackContext context)
    //{
    //    Debug.Log("bottttooon");
    //    renderers[2].material.color = Color.red;
    //}
    //public void RotateAction(InputAction.CallbackContext context)
    //{

    //    Debug.Log("bottttooon");
    //    renderers[3].material.color = Color.red;
    //}
}