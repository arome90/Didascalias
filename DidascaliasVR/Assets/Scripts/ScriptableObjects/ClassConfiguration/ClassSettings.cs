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
    [SerializeField]
    private int _maxStudents = 30;
    public int MaxStudents { get { return _maxStudents; } set { _maxStudents = value; } }

    [SerializeField,
        Tooltip("Si se aplican los números de estudiantes por género o se hacen aleatoriamente")]
    private bool _genderSpecified = false;
    public bool GenderSpecified { get { return _genderSpecified; } set { _genderSpecified = value; } }

    [SerializeField]
    private int _numBoys = 0;
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
        } }

    [SerializeField]
    private int _numGirls = 0;
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
        } }

    [Header("Shape Options")]
    [SerializeField]
    private Shape _shape = Shape.Square;
    public Shape ClassShape { get { return _shape; } set { _shape = value;  } }

    [SerializeField]
    private int _numDesks = 10;
    public int NumDesks { get {return _numDesks; } set { _numDesks = value; } }

    // Square Options
    [SerializeField]
    private int _rows = 2;
    public int Rows { get { return _rows; } set { _rows = value; } }

    [SerializeField]
    private int _cols = 2;
    public int Cols { get { return _cols; } set { _cols = value; } }

    // Circular Options
    [SerializeField]
    /*Both Circular and U options*/
    private float _radius = 10.0f;
    public float Radius { get { return _radius; } set { _radius = value; } }

    [SerializeField]
    private float _angleSeparation = 15.0f;
    public float AngleSeparation { get { return _angleSeparation; } set { _angleSeparation = value; } }

    // U Options
    [SerializeField]
    private int _maxDesksInSemicircle = 7;
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
                    "_maxStudents", "_shape", "_rows", "_cols", "_radius", "_angleSeparation",
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

            if (_properties["_genderSpecified"].boolValue)
            {
                DrawProperty(_properties["_numBoys"], "Number of Boys");
                DrawProperty(_properties["_numGirls"], "Number of Girls");
            }

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
                    DrawProperty(_properties["_angleSeparation"], "Angle Separation");
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
