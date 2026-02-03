using Oculus.Voice;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

public class WitTextTypingDebug : MonoBehaviour
{
    [SerializeField] TMP_InputField _inputField = null;

    AppVoiceExperience _wit = null;

    XRDeviceSimulator _simulator = null;

    private void Start()
    {
        _wit = GetComponent<AppVoiceExperience>();
        if (_wit == null) Debug.LogError("AppVoiceExperience not found in: " + gameObject.name);

        _simulator = FindFirstObjectByType<XRDeviceSimulator>();

        _inputField.gameObject.SetActive(false);

        _inputField.onEndEdit.AddListener(_wit.Activate);
        _inputField.onEndEdit.AddListener(DisableInputField);
    }

    private void EnableInputField()
    {
        _inputField.gameObject.SetActive(true);
        _inputField.ActivateInputField();

        if(_simulator != null) _simulator.enabled = false;
    }

    private void DisableInputField(string _)
    {
        _inputField.DeactivateInputField();
        _inputField.gameObject.SetActive(false);

        if (_simulator != null) _simulator.enabled = true;
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.T))
        {
            EnableInputField();
        }
    }


    private void OnDestroy()
    {
        _inputField.onEndEdit.RemoveAllListeners();
    }
}
