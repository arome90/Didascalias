using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Permite que se apliquen las settings guardadas en los ValueUIs correspondientes
/// antes de que se apliquen los valores que tienen guardados las UI por defecto
/// </summary>
public class LayoutValuePersistency : MonoBehaviour
{
    [SerializeField,
    Tooltip("UI que modifica el número de chicos")]
    List<ValueUI> _boysValueUIs = new List<ValueUI>();

    [SerializeField,
    Tooltip("UI que modifica el número de chicas")]
    List<ValueUI> _girlsValueUIs = new List<ValueUI>();

    [SerializeField,
        Tooltip("UI que modifica el número de escritorios")]
    List<ValueUI> _deskValueUIs = new List<ValueUI>();

    [SerializeField,
        Tooltip("UI que modifica el radio")]
    List<ValueUI> _radiusValueUIs = new List<ValueUI>();

    [SerializeField,
        Tooltip("UI que modifica el número de filas")]
    List<ValueUI> _rowsValueUIs = new List<ValueUI>();

    [SerializeField,
        Tooltip("UI que modifica el número de columnas")]
    List<ValueUI> _colsValueUIs = new List<ValueUI>();

    private void Start()
    {
        ClassSettings settings = ClassManager.Instance.Settings;

        foreach (ValueUI item in _boysValueUIs)
        {
            item.SetValueNotRaiseEvent(settings.NumBoys);
        }
        foreach (ValueUI item in _girlsValueUIs)
        {
            item.SetValueNotRaiseEvent(settings.NumGirls);
        }
        foreach (ValueUI item in _deskValueUIs)
        {
            item.SetValueNotRaiseEvent(settings.NumDesks);
        }
        foreach (ValueUI item in _radiusValueUIs)
        {
            item.SetValueNotRaiseEvent(settings.Radius);
        }
        foreach (ValueUI item in _colsValueUIs)
        {
            item.SetValueNotRaiseEvent(settings.Cols);
        }
        foreach (ValueUI item in _rowsValueUIs)
        {
            item.SetValueNotRaiseEvent(settings.Rows);
        }
    }
}
