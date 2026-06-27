
using UnityEngine;

using UnityEngine.SpatialTracking;

/// <summary>
/// Clase que gestiona las variables de la cabeza y reconoce gestos como asentir o negar.
/// </summary>
public class HeadVariables
{
    private Quaternion _miradaPoint;
    private Vector3 _position;
    private Vector3 _lastPosition;
    private float _distance;
    private VariableMeasurement _velocidad;

    private Motion _shake;
    private Motion _nod;
    private Requirement _req;

    /// <summary>
    /// Propiedad que devuelve la velocidad medida.
    /// </summary>
    public VariableMeasurement Velocidad { get { return _velocidad; } }

    /// <summary>
    /// Constructor de HeadVariables que inicializa las variables y la velocidad con una ventana de medición.
    /// </summary>
    /// <param name="windowSize">Tamaño de la ventana de medición para la velocidad.</param>
    public HeadVariables()
    {
        InitMotions();
        Pose po;
        PoseDataSource.TryGetDataFromSource(TrackedPoseDriver.TrackedPose.Head, out po);
        _position = po.position;
        _velocidad = new VariableMeasurement();
    }

    /// <summary>
    /// Actualiza las variables de la posición de la cabeza y calcula la velocidad en base al tiempo transcurrido.
    /// </summary>
    /// <param name="time">Tiempo transcurrido entre actualizaciones.</param>
    public void UpdateHead(float time)
    {
        Pose pose;
        if (PoseDataSource.TryGetDataFromSource(TrackedPoseDriver.TrackedPose.Head, out pose))
        {
            _lastPosition = _position;
            _position = pose.position;
            _miradaPoint = pose.rotation;
            _distance = Vector3.Distance(_position, _lastPosition);
            _velocidad.Variable = _distance / time;
        }
    }

    #region HeadGesture
    /// <summary>
    /// Estructura para manejar los movimientos de la cabeza.
    /// </summary>
    private struct Motion
    {
        public float inProgress;
        public float lastSignificantAngle;
        public int lastDigital;
        public int count;
        public string message;
    }

    /// <summary>
    /// Estructura que define los requisitos para reconocer gestos de la cabeza.
    /// </summary>
    private struct Requirement
    {
        public int count;
        public float timing;
        public float angular;
    }

    /// <summary>
    /// Inicializa los movimientos de asentir y negar.
    /// </summary>
    private void InitMotions()
    {
        _req = new Requirement { count = 6, angular = 3f, timing = 0.75f };
        _shake.message = "NO";
        _nod.message = "SI";
    }

    /// <summary>
    /// Actualiza los movimientos de asentir y negar en base a la rotación de la cabeza.
    /// </summary>
    public void UpdateMotionHead()
    {
        Pose pose;
        PoseDataSource.TryGetDataFromSource(TrackedPoseDriver.TrackedPose.Head, out pose);
        UpdateMotion(ref _shake, GetShakeAngle(pose.rotation));
        UpdateMotion(ref _nod, GetNodAngle(pose.rotation));
    }

    /// <summary>
    /// Actualiza un movimiento específico de la cabeza en base al ángulo calculado.
    /// </summary>
    /// <param name="mot">Referencia a la estructura Motion.</param>
    /// <param name="angle">Ángulo de la cabeza.</param>
    /// <returns>True si se detecta un gesto completo, False en caso contrario.</returns>
    private bool UpdateMotion(ref Motion mot, float angle)
    {
        if (mot.inProgress > 0)
        {
            mot.inProgress -= Time.deltaTime;
            if (mot.inProgress <= 0)
            {
                mot.count = 0;
                mot.lastDigital = 0;
            }
        }

        int gesture = 0;
        float deltaAngle = Mathf.DeltaAngle(angle, mot.lastSignificantAngle);

        if (deltaAngle < -_req.angular)
        {
            gesture = -1;
            mot.lastSignificantAngle = angle;
        }
        else if (deltaAngle > _req.angular)
        {
            gesture = 1;
            mot.lastSignificantAngle = angle;
        }

        if (gesture != 0 && gesture != mot.lastDigital)
        {
            mot.lastDigital = gesture;
            mot.count++;
            mot.inProgress = _req.timing;

            if (mot.count >= _req.count)
            {
                mot.count = 0;
                Debug.Log(mot.message);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Obtiene el ángulo de negación (shake) de la cabeza a partir de la rotación.
    /// </summary>
    /// <param name="rot">Rotación actual de la cabeza.</param>
    /// <returns>Ángulo de negación.</returns>
    private float GetShakeAngle(Quaternion rot)
    {
        return rot.eulerAngles.y;
    }

    /// <summary>
    /// Obtiene el ángulo de asentimiento (nod) de la cabeza a partir de la rotación.
    /// </summary>
    /// <param name="rot">Rotación actual de la cabeza.</param>
    /// <returns>Ángulo de asentimiento.</returns>
    private float GetNodAngle(Quaternion rot)
    {
        Vector3 forward = rot * Vector3.forward;
        forward = Vector3.Normalize(forward);
        float forwardY = forward.y;
        return Mathf.Asin(forwardY) * Mathf.Rad2Deg;
    }
    #endregion
}
