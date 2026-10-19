using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class VRKeyboardHelper : MonoBehaviour
{
    private TMP_InputField _inputField;
    private TouchScreenKeyboard _keyboard;

    void Start()
    {
        _inputField = GetComponent<TMP_InputField>();

        // Triggers the keyboard to open when the VR pointer clicks the input field
        _inputField.onSelect.AddListener(delegate { OpenKeyboard(); });
    }

    public void OpenKeyboard()
    {
        // Opens the native Meta Quest system keyboard
        if (_keyboard == null || !_keyboard.active)
        {
            _keyboard = TouchScreenKeyboard.Open(_inputField.text, TouchScreenKeyboardType.Default);
        }
    }

    void Update()
    {
        // Constantly updates the Unity UI text with what the user types on the Quest keyboard
        if (_keyboard != null && _keyboard.active)
        {
            _inputField.text = _keyboard.text;
        }
    }
}