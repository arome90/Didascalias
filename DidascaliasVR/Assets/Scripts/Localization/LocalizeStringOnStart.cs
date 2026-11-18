using TMPro;
using UnityEngine.Localization.Components;

public class LocalizeStringOnStart : LocalizeStringEvent
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
        if (_text == null) _text = GetComponent<TextMeshProUGUI>();

        OnUpdateString.AddListener(ChangeText);
    }

    private void OnDestroy() => OnUpdateString.RemoveListener(ChangeText);
    private void ChangeText(string text) => _text.text = text;
}
