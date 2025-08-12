using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WitDebugPanel : MonoBehaviour
{
    [SerializeField]
    GameObject _panelPrefab;

    [SerializeField]
    TextMeshProUGUI _mainPanelText = null;

    [SerializeField]
    TextMeshProUGUI _studentsPanelText = null;

    VerticalLayoutGroup _layout;

    private void Start()
    {
        _layout = GetComponentInChildren<VerticalLayoutGroup>();
    }

    public void AddPanel(string text)
    {
        GameObject go = Instantiate(_panelPrefab, _layout.transform);
        go.GetComponentInChildren<TextMeshProUGUI>().text = text;
    }

    public void SetMainText(string text)
    {
        _mainPanelText.text = text;
    }

    public void ChangeStudentPanel(string students)
    {
        _studentsPanelText.text = students;
    }
}
