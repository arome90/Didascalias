using UnityEngine;

/// <summary>
/// Encargado de establecer el límite de escritorios
/// en el layout cuadrado de la clase. También está encargado
/// de hacer que las modificaciones aplicadas a filas y columnas
/// tengan sentido y no se puedan reducir los escritorios
/// o filas o columnas si el número de estudiantes mínimo
/// no está cubierto por el número de escritorios
/// </summary>
public class NumDesksSquareLayoutLimitUI : MonoBehaviour
{
    [SerializeField,
        Tooltip("ValueUI que controla el número " +
        "de escritorios de la forma de clase cuadrada")]
    ValueUI _desksUI;

    [SerializeField,
        Tooltip("ValueUI que controla el número " +
        "de filas de la forma de clase cuadrada")]
    ValueUI _rowsUI;

    [SerializeField,
        Tooltip("ValueUI que controla el número " +
        "de columnas de la forma de clase cuadrada")]
    ValueUI _colsUI;

    /// <summary>
    /// Variable auxiliar que previene que una función se llame de forma recursiva sin querer, por
    /// acción de un evento que llama a la función desde dentro de la misma
    /// </summary>
    bool _modificationsEnded = true;

    private void OnEnable()
    {
        SetLimit();
        SetRowsAndColsLimit();
    }

    /// <summary>
    /// Settea el límite del ValueUI de escritorios
    /// según la multiplicación del número de filas y columnas
    /// existentes
    /// </summary>
    public void SetLimit()
    {
        ClassSettings settings = ClassManager.Instance.Settings;

        _desksUI.SetMinValue(settings.NumStudents);
        _desksUI.SetMaxValue(settings.Rows * settings.Cols);
    }

    /// <summary>
    /// Establecemos el valor de las filas y columnas según
    /// el número de estudiantes que haya.
    /// Es la combinación de filas y columnas mínima (donde
    /// las columnas siempre serán mayores en valor) que da
    /// para el número de estudiantes establecido
    /// </summary>
    public void SetRowsAndColsLimit()
    {
        ClassSettings settings = ClassManager.Instance.Settings;

        int maxDesks = settings.Rows * settings.Cols;

        if (settings.NumStudents <= maxDesks) return;
        else
        {
            (int rows, int cols) = GetRowsAndCols(settings.NumStudents);

            _rowsUI.SetValue(rows);
            _colsUI.SetValue(cols);
        }
    }

    /// <summary>
    /// Modifica el layout de la clase intentando cambiar las filas.
    /// Este método debe llamarse tras modificar las columnas, ya que es el valor que no
    /// queremos que cambie tras nuestra modificación
    /// </summary>
    public void ModifyRows()
    {
        ModifyGrid(ClassManager.Instance.Settings.Cols, ClassManager.Instance.Settings.Rows, false);
    }

    /// <summary>
    /// Modifica el layout de la clase intentando cambiar las columnas.
    /// Este método debe llamarse tras modificar las filas, ya que es el valor que no
    /// queremos que cambie tras nuestra modificación
    /// </summary>
    public void ModifyCols()
    {
        ModifyGrid(ClassManager.Instance.Settings.Rows, ClassManager.Instance.Settings.Cols, true);
    }

    /// <summary>
    /// Este método lo utilizamos para modificar las filas y columnas al haber
    /// incrementado o disminuido una de las dos variables.
    /// 
    /// El objetivo del método es NO modificar el nuevo valor dado, sino
    /// ajustar el otro para seguir cumpliendo la condición de que el número de escritorios
    /// que podemos encajar sea mayor o igual que el número de estudiantes
    /// </summary>
    /// <param name="changedValue"> Valor modificado por UI </param>
    /// <param name="unchangedValue"> Valor que NO ha sido modificado </param>
    /// <param name="modifyingCols"> Si se están modificando las columnas o, en caso contrario, las filas </param>
    private void ModifyGrid(int changedValue, int unchangedValue, bool modifyingCols)
    {
        // utilizamos esta variable por si se intenta volver a llamar a este método
        // a pesar de que este no haya acabado (a través de eventos, por ejemplo)
        if (!_modificationsEnded) return;
        _modificationsEnded = false;

        ClassSettings settings = ClassManager.Instance.Settings;

        // establecemos el valor máximo de i y de j según estemos modificando
        // las columnas o las filas
        int maxJValue;
        int maxIValue;
        if (!modifyingCols)
        {
            maxJValue = (int)_colsUI.GetMaxValue();
            maxIValue = (int)_rowsUI.GetMaxValue();
        }
        else
        {
            maxJValue = (int)_rowsUI.GetMaxValue();
            maxIValue = (int)_colsUI.GetMaxValue();
        }

        // Con este bucle intentamos buscar una combinación de valores de filas y columnas que nos den la combinación
        // idónea para el número de estudiantes.
        // El bucle  prioriza no cambiar la variable 'changedValue'
        int maxDesks = changedValue * unchangedValue;
        for (int j = changedValue; j <= maxJValue && (maxDesks < settings.NumStudents); ++j)
        {
            for (int i = unchangedValue; i <= maxIValue; ++i)
            {
                maxDesks = i * j;
                if (maxDesks >= settings.NumStudents)
                {
                    if(modifyingCols)
                    {
                        settings.Rows = j;
                        settings.Cols = i;
                    }
                    else
                    {
                        settings.Rows = i;
                        settings.Cols = j;
                    }
                    // es posible que esta función llame a un evento que vuelva 
                    // a llamar al método ModifyGrid, es por eso que
                    // utilizamos la variable _modificationsEnded
                    _colsUI.SetValue(settings.Cols);
                    _rowsUI.SetValue(settings.Rows);
                    break;
                }
            }
        }

        _desksUI.SetMaxValue(settings.Rows * settings.Cols);

        // quitamos la flag para poder volver a llamar al método sin problema
        _modificationsEnded = true;
    }

    /// <summary>
    /// Calcula la combinación de filas y columnas más equilibrada que:
    /// filas * columnas >= objective
    /// El número de filas siempre será menor o igual al número de columnas 
    /// (excepto cuando las columnas ya hayan llegado a su valor máximo, que las filas pueden sobrepasarlas)
    /// </summary>
    /// <param name="objective"> Valor objetivo </param>
    /// <returns> numero de filas y columnas </returns>
    private (int filas, int columnas) GetRowsAndCols(int objective)
    {
        int i = 0;
        int j = 0;

        bool reachedObjective = false;

        for(i = 0; i < _rowsUI.GetMaxValue() && !reachedObjective; )
        {
            ++i;
            for(j = 0; j < _colsUI.GetMaxValue() && !reachedObjective;)
            {
                ++j;
                reachedObjective = i * j >= objective;
            }
        }

        return (i, j);
    }
}
