using UnityEngine;
using UnityEngine.SpatialTracking;

/// <summary>
/// Administra y actualiza los datos de las manos según la variable de tiempo.
/// Inicializa cada mano y actualiza sus datos.
/// </summary>
public class HandsManager
{
    private HandVariables _leftHand;
    private HandVariables _rightHand;
    public HandVariables LeftHand => _leftHand; // Propiedad para acceder a los datos de la mano izquierda
    public HandVariables RightHand => _rightHand; // Propiedad para acceder a los datos de la mano derecha

    /// <summary>
    /// Constructor que inicializa las variables de las manos.
    /// </summary>
    public HandsManager()
    {
        _leftHand = new HandVariables(TrackedPoseDriver.TrackedPose.LeftPose);
        _rightHand = new HandVariables(TrackedPoseDriver.TrackedPose.RightPose);
    }

    /// <summary>
    /// Actualiza los datos de las manos izquierda y derecha.
    /// </summary>
    /// <param name="time">Tiempo transcurrido para calcular la velocidad y aceleración</param>
    public void UpdateHands(float time)
    {
        _leftHand.UpdateHand(time);
        _rightHand.UpdateHand(time);
    }
}

/// <summary>
/// Clase que representa las variables relacionadas con las manos.
/// Gestiona la posición, velocidad y aceleración de la mano.
/// </summary>
public class HandVariables
{
    private Vector3 _currentPosition; // Posición actual de la mano
    private Vector3 _lastPosition; // Posición anterior de la mano

    private VariableMeasurement _positionMagnitude; // Magnitud de la posición
    private VariableMeasurementVector3 _positionVector; // Vector de la posición
    private VariableMeasurement _distanceTraveled; // Distancia recorrida
    private VariableMeasurement _velocity; // Velocidad de la mano
    private VariableMeasurement _acceleration; // Aceleración de la mano

    private TrackedPoseDriver.TrackedPose _trackedPose; // Pose de la mano rastreada


    public VariableMeasurement Velocity => _velocity;

    /// <summary>
    /// Constructor que inicializa la clase con la pose rastreada y el tamaño de la ventana de medición.
    /// </summary>
    /// <param name="trackedPose">Pose rastreada (izquierda o derecha)</param>
    /// <param name="windowSize">Tamaño de la ventana para los cálculos de medición</param>
    public HandVariables(TrackedPoseDriver.TrackedPose trackedPose)
    {
        _trackedPose = trackedPose;
        InitializeVariables();
    }

    /// <summary>
    /// Inicializa las variables basadas en la pose inicial de la mano.
    /// </summary>
    private void InitializeVariables()
    {
        Pose initialPose;
        PoseDataSource.TryGetDataFromSource(_trackedPose, out initialPose); // Obtiene la pose inicial
        _currentPosition = initialPose.position; // Asigna la posición inicial
        _lastPosition = _currentPosition; // La posición anterior es la misma inicialmente

        // Inicializa las variables de medición
        _velocity = new VariableMeasurement();
        _positionMagnitude = new VariableMeasurement();
        _positionVector = new VariableMeasurementVector3(5);
        _distanceTraveled = new VariableMeasurement();
        _acceleration = new VariableMeasurement();
    }

    /// <summary>
    /// Actualiza los datos de la mano basados en la pose actual y el tiempo transcurrido.
    /// </summary>
    /// <param name="time">Tiempo transcurrido para los cálculos</param>
    public void UpdateHand(float time)
    {
        Pose currentPose;
        if (PoseDataSource.TryGetDataFromSource(_trackedPose, out currentPose)) // Obtiene la pose actual
        {
            _lastPosition = _currentPosition; // Actualiza la posición anterior
            _currentPosition = currentPose.position; // Actualiza la posición actual

            // Actualiza las variables de medición
            _positionMagnitude.Variable = _currentPosition.magnitude;
            _positionVector.Variable = _currentPosition;

            float distance = Vector3.Distance(_currentPosition, _lastPosition); // Calcula la distancia recorrida
            _distanceTraveled.Variable = distance;

            // Calcula la velocidad y aceleración
            float newVelocity = distance / time;
            _velocity.Variable = newVelocity;
            _acceleration.Variable = (newVelocity - _velocity.Variable) / time;
        }
    }
}
