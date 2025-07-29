using UnityEngine;

/// <summary>
/// Encargado de establecer el límite de escritorios
/// en el layout cuadrado de la clase.
/// </summary>
public class NumDesksSquareLayoutLimitUI : MonoBehaviour
{
    [SerializeField,
        Tooltip("ValueUI que controla el número " +
        "de escritorios de la forma de clase cuadrada")]
    ValueUI _desksUI;

    public void SetLimit()
    {
        _desksUI.SetMaxValue(
            ClassManager.Instance.Settings.Rows * 
            ClassManager.Instance.Settings.Cols);
    }
}
