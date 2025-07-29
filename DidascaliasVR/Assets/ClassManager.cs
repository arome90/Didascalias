using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Maneja la configuración de la clase y la aplica según corresponda.
/// Encargada de hacer la disposición de los escritorios.
/// </summary>
public class ClassManager : Singleton<ClassManager>
{
    [Header("Class Settings")]
    [SerializeField,
        Tooltip("Configuración del aula actual")] 
    private ClassSettings _settings;

    [Header("Class Objects")]
    [SerializeField,
        Tooltip("Prefab utilizado para los escritorios")] 
    private GameObject _deskPrefab;
    [SerializeField,
        Tooltip("Punto central de la clase, desde la cuál se generarán los escritorios")]
    private Transform _classRoot;
    public ClassSettings Settings { get { return _settings; } }

    private List<GameObject> _desks = null;

    #region Settings Change

    public void SetBoysNumber(int numBoys)
    {
        _settings.NumBoys = numBoys;
    }

    public void SetGirlsNumber(int numGirls)
    {
        _settings.NumGirls = numGirls;
    }

    public void SetNumDeks(int numDesks)
    {
        _settings.NumDesks = numDesks;
    }

    public void SetRows(int rows)
    {
        _settings.Rows = rows;
    }

    public void SetCols(int cols)
    {
        _settings.Cols = cols;
    }

    public void SetRadius(float radius)
    {
        _settings.Radius = radius;
    }

    #endregion

    /// <summary>
    /// Esta región contiene los métodos que se llamarán desde la UI
    /// con la que configuraremos la clase
    /// </summary>
    #region UI_Layout_Generation 

    public void SetRowsAndGenerate(Single rows, Single _)
    {
        SetRows((int)rows);
        GenerateClass();
    }

    public void SetColsAndGenerate(Single cols, Single _)
    {
        SetCols((int)cols);
        GenerateClass();
    }

    public void SetDesksAndGenerate(Single numDesks, Single _)
    {
        SetNumDeks((int)numDesks);
        GenerateClass();
    }

    public void SetRadiusAndGenerate(Single radius, Single _)
    {
        SetRadius(radius);
        GenerateClass();
    }

    public void GenerateClass()
    {
        ArrangeClass(_settings.ClassShape);
    }

    public void ArrangeClass(int type)
    {
        ArrangeClass((ClassSettings.Shape)type);
    }
    #endregion

    [Header("Generation Settings")]
    [SerializeField,
        Tooltip("Distancia desde el centro de la clase hasta cada lateral de la misma. Utilizado para saber cómmo colocar los escritorios"), 
        Range(2.0f, 4.5f)]
    private float _classWidth = 3.4f;

    /// <summary>
    /// Llamamos a este método para generar una clase con escritorios según
    /// una forma dada
    /// </summary>
    /// <param name="shape"> La forma de la clase </param>
    public void ArrangeClass(ClassSettings.Shape shape)
    {
        _settings.ClassShape = shape;
        
        // Casos especiales
        if(_settings.NumDesks == 1)
        {
            CleanClass();

            _desks.Add(Instantiate(_deskPrefab, _classRoot));

            return;
        }
        
        // Casos normales
        switch (shape) {
            case ClassSettings.Shape.Square:
                ArrangeSquareClass();
                break;
            case ClassSettings.Shape.Circular:
                ArrangeCircularClass();
                break;
            case ClassSettings.Shape.U:
                ArrangeUClass();
                break;
        }
    }

    public override void Awake()
    {
        base.Awake();
    }

    void CleanClass()
    {
        // Cleaning
        if (_desks != null)
        {
            foreach (GameObject obj in _desks) { Destroy(obj); }
            _desks.Clear();
        }
        else
        {
            _desks = new List<GameObject>();
        }
    }

    /// <summary>
    /// Genera una clase con escritorios en forma cuadrada
    /// </summary>
    void ArrangeSquareClass()
    {
        CleanClass();
        int rows = _settings.Rows;
        int cols = _settings.Cols;

        // 5 es el número máximo de columnas y filas
        // Me gustaría ver cómo incorporarlo de otra forma
        // que no sea a mano
        float generalOffset = _classWidth / 5.0f;

        Vector3 startingPoint = _classRoot.position + 
            new Vector3(cols *generalOffset, 0, rows * generalOffset);

        float xDeskOffset = _classWidth / 2;
        float zDeskOffset = _classWidth / 2;

        for (int i = 0; i < rows && _desks.Count < _settings.NumDesks; ++i)
        {
            Vector3 currentPoint = startingPoint;
            for (int j = 0; j < cols && _desks.Count < _settings.NumDesks; ++j) 
            {
                AddDesk(currentPoint, Quaternion.identity);
                currentPoint += Vector3.right * -xDeskOffset; 
            }
            startingPoint += Vector3.forward * -zDeskOffset;
        }
    }

    /// <summary>
    /// Genera una clase con escritorios en forma circular, mirando hacia el centro
    /// </summary>
    void ArrangeCircularClass()
    {
        CleanClass();

        float angleIncrement = Mathf.Deg2Rad * (360.0f / _settings.NumDesks);

        for(int i = 0; i < _settings.NumDesks; ++i)
        {
            Vector3 currentPosition = _classRoot.position +
                Vector3.right * _settings.Radius * Mathf.Cos(angleIncrement * i) +
                Vector3.forward * _settings.Radius * Mathf.Sin(angleIncrement * i);

            Quaternion rot = Quaternion.LookRotation(currentPosition - _classRoot.position);

            AddDesk(currentPosition, rot);
        }
    }

    /// <summary>
    /// Genera una clase con escritorios en forma de U, mirando hacia la pizarra
    /// </summary>
    void ArrangeUClass()
    {
        CleanClass();

        int desksInCircle = (Mathf.Min(_settings.NumDesks, _settings.MaxDesksInSemiCircle));
        if(desksInCircle == _settings.MaxDesksInSemiCircle && _settings.NumDesks % 2 == 0)
        {
            // restamos 1 para que no quede un lado más largo que otro en la U
            //
            // ||
            // ||     ||       <== Para evitar esto
            //   || ||
            desksInCircle--;
        }

        float angleIncrement = Mathf.Deg2Rad * (180.0f / (desksInCircle - 1));

        Vector3 currentPosition = _classRoot.position;

        for (int i = 0; i < desksInCircle; ++i)
        {
            currentPosition = _classRoot.position +
                Vector3.right * _settings.Radius * Mathf.Cos(angleIncrement * i) -
                Vector3.forward * _settings.Radius * Mathf.Sin(angleIncrement * i);

            AddDesk(currentPosition, Quaternion.identity);
        }

        int desksInLine = _settings.NumDesks - desksInCircle;

        for(int i = 0; i < desksInLine; ++i)
        {
            Vector3 dir;
            if (i % 2 == 0) dir = Vector3.right * _settings.Radius; 
            else dir = -Vector3.right *_settings.Radius;

            currentPosition = _classRoot.position + dir;
            currentPosition += Vector3.forward * (_settings.Radius / 2.0f) * (i/2 + 1);
            
            AddDesk(currentPosition, Quaternion.identity);
        }

        //// Añadimos dos escritorios, uno a cada lado del semicírculo
        
        //currentPosition = _classRoot.position - Vector3.right * _settings.Radius;
        //currentPosition += Vector3.forward * (_settings.Radius / 2.0f);
        //AddDesk(currentPosition, Quaternion.identity);
    }

    void AddDesk(Vector3 position, Quaternion rot)
    {
        _desks.Add(Instantiate(_deskPrefab, position, rot));
        _desks[_desks.Count - 1].transform.parent = _classRoot;
    }
}
