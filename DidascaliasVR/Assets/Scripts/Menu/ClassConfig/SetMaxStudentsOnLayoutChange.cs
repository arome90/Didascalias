using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.UI;

public class SetMaxStudentsOnLayoutChange : MonoBehaviour
{
    [SerializeField]
    Button[] _layoutButtons = null;

    [SerializeField]
    ValueUI _boysValueUI = null;

    [SerializeField]
    ValueUI _girlsValueUI = null;

    [SerializedDictionary("Class Shape", "Max Students"), SerializeField]
    SerializedDictionary<ClassSettings.Shape, int> _maxStudentsByShape;

#if DEBUG
    void DebugMessages()
    {
        if (!_boysValueUI) Debug.LogWarning("Boys Value UI is not set!");
        if (!_girlsValueUI) Debug.LogWarning("Girls Value UI is not set!");
    }
#endif

    private void OnEnable()
    {
#if DEBUG
        DebugMessages();
#endif
        OnShapeChanged();
    }

    public void OnShapeChanged()
    {
        if (!ClassManager.Instance) return;

        ClassSettings settings = ClassManager.Instance.Settings;

        settings.MaxStudents = _maxStudentsByShape[settings.ClassShape];

        int boys = (int)_boysValueUI.Value;
        int girls = (int)_girlsValueUI.Value;

        while (boys + girls > settings.MaxStudents)
        {
            if(boys >= girls) boys--;
            else girls--;
        }

        _boysValueUI.SetMaxValue(settings.MaxStudents);
        _boysValueUI.SetValue(boys);

        _girlsValueUI.SetMaxValue(settings.MaxStudents);
        _girlsValueUI.SetValue(girls);

        int i = 0;
        foreach(Button button in _layoutButtons)
        {
            button.interactable = (ClassSettings.Shape)i != settings.ClassShape;
            ++i;
        }
    }
}
