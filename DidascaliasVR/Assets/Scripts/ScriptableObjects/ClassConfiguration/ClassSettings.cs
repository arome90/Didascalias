using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Se define el ScriptableObject "ClassSettings", con el que podremos
/// configurar la clase y su disposición.
/// </summary>
[CreateAssetMenu(fileName = "ClassSettings", menuName = "Scriptable Objects/ClassSettings")]
public class ClassSettings : ScriptableObject
{
    public enum Shape { Square, Circular, U }

    [Header("Parameters")]
    [SerializeField,
        Tooltip("Número máximo de estudiantes permitidos")]
    private int _maxStudents = 30;

    /// <summary>
    /// Número máximo de estudiantes permitidos
    /// </summary>
    public int MaxStudents { get { return _maxStudents; } set { _maxStudents = value; } }

    [SerializeField,
        Tooltip("Número de chicos en el aula")]
    private int _numBoys = 0;

    /// <summary>
    /// Número de chicos en el aula
    /// Se ajusta según el número máximo de estudiantes y el número de chicas
    /// </summary>
    public int NumBoys { get { return _numBoys; } 
        set 
        {
            if (value > _maxStudents)
            {
                _numBoys = _maxStudents;
                _numGirls = 0;
            }
            else if (value + _numGirls > _maxStudents)
            {
                _numBoys = value;
                _numGirls = _maxStudents - _numBoys;
            }
            else _numBoys = value;

            if (_numDesks < NumStudents) _numDesks = NumStudents;
        } }

    [SerializeField,
        Tooltip("Número de chicas en el aula")]
    private int _numGirls = 0;

    /// <summary>
    /// Número de chicas en el aula
    /// Se ajusta según el número máximo de estudiantes y el número de chicos
    /// </summary>
    public int NumGirls { get { return _numGirls; } 
        set 
        { 
            if(value > _maxStudents)
            {
                _numGirls = _maxStudents;
                _numBoys = 0;
            }
            else if(value + _numBoys > _maxStudents)
            {
                _numGirls = value;
                _numBoys = _maxStudents - _numGirls;
            }
            else _numGirls = value;

            if (_numDesks < NumStudents) _numDesks = NumStudents;
        } }

    /// <summary>
    /// Número de estudiantes totales. Devuelve una suma del número de chicas y chicos.
    /// Al settear el valor, el número de chicos y chicas pasa a ser la mitad de dicho
    /// valor.
    /// </summary>
    public int NumStudents
    {
        get
        {
            return _numGirls + _numBoys;
        }
        set
        {
            value = Mathf.Min(value, _maxStudents);
            if (value % 2 == 0)
            {
                _numGirls = value / 2;
                _numBoys = value / 2;
            }
            else
            {
                _numGirls = value / 2 + 1;
                _numBoys = value / 2;
            }
        }
    }

    [Header("Shape Options")]
    [SerializeField,
        Tooltip("Layout de la clase")]
    private Shape _shape = Shape.Square;

    /// <summary>
    /// Layout de la clase
    /// </summary>
    public Shape ClassShape { get { return _shape; } set { _shape = value;  } }

    [SerializeField,
        Tooltip("Número de escritorios")]
    private int _numDesks = 10;

    /// <summary>
    /// Número de escritorios en el aula. 
    /// Si resultan ser más que el número total de alumnos,
    /// quedarán vacíos los que estén más al fondo.
    /// Si resultan ser menos que el número total de alumnos,
    /// se ajustará para que sean tantos escritorios como alumnos
    /// </summary>
    public int NumDesks { get {return _numDesks; } set { _numDesks = value; } }

    // Square Options
    [SerializeField,
        Tooltip("Cuántas filas tiene la clase. Solo se usa en Shape.Square"), Range(1,5)]
    private int _rows = 2;
    /// <summary>
    /// Cuántas filas tiene la clase. 
    /// Solo se usa en Shape.Square
    /// </summary>
    public int Rows { get { return _rows; } set { _rows = value; } }

    [SerializeField,
        Tooltip("Cuántas columnas tiene la clase. Solo se usa en Shape.Square"), Range(1,5)]
    private int _cols = 2;
    /// <summary>
    /// Cuántas columnas tiene la clase. 
    /// Solo se usa en Shape.Square
    /// </summary>
    public int Cols { get { return _cols; } set { _cols = value; } }

    // Circular Options
    [SerializeField,
        Tooltip("Radio de la circunferencia descrita en la forma de clase U o Circular")]
    /*Both Circular and U options*/
    private float _radius = 10.0f;

    /// <summary>
    /// Radio de la circunferencia descrita en la forma de clase U o Circular
    /// </summary>
    public float Radius { get { return _radius; } set { _radius = value; } }

    // U Options
    [SerializeField,
        Tooltip("Número máximo de escritorios que forman la semicircunferencia del layout en forma de U")]
    private int _maxDesksInSemicircle = 7;

    /// <summary>
    /// Número máximo de escritorios que forman la semicircunferencia del layout en forma de U
    /// </summary>
    public int MaxDesksInSemiCircle { get { return _maxDesksInSemicircle; } set { _maxDesksInSemicircle = value; } }


#if UNITY_EDITOR
    /// <summary>
    /// Encargado de mostrar las propiedades adecuadas para el ClassSettings, pudiendo cambiar
    /// entre las diferentes formas de las clases y sus parámetros
    /// </summary>
    [CustomEditor(typeof(ClassSettings))]
    public class ClassSettingsEditor : Editor {
        private Dictionary<string, SerializedProperty>_properties;

        private void OnEnable()
        {
            string[] propertyNames = {
                    "_maxStudents", "_shape", "_rows", "_cols", "_radius",
                    "_numDesks", "_numBoys", "_numGirls", "_genderSpecified", "_maxDesksInSemicircle"
                };
            _properties = new Dictionary<string, SerializedProperty>();
            for (int i = 0; i < propertyNames.Length; i++)
            {
                _properties.Add(propertyNames[i], serializedObject.FindProperty(propertyNames[i]));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            DrawProperty(_properties["_maxStudents"], "Max Students");
            DrawProperty(_properties["_numBoys"], "Boys");
            DrawProperty(_properties["_numGirls"], "Girls");

            DrawProperty(_properties["_shape"], "Shape");
            DrawProperty(_properties["_numDesks"], "Number of Desks");

            switch ((Shape)_properties["_shape"].enumValueIndex)
            {
                case Shape.Square:
                    DrawProperty(_properties["_rows"], "Rows");
                    DrawProperty(_properties["_cols"], "Columns");
                    break;
                case Shape.Circular:
                    DrawProperty(_properties["_radius"], "Radius");
                    break;
                case Shape.U:
                    DrawProperty(_properties["_radius"], "Radius");
                    DrawProperty(_properties["_maxDesksInSemicircle"], "Maximum Desks in Semicircle");
                    break;
            }

            EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawProperty(SerializedProperty prop, string label)
        {
            EditorGUILayout.PropertyField(prop, new GUIContent(label));
        }
    }
#endif
}
