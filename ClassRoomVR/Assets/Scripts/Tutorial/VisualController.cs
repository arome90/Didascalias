using ClassRoomVR;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de visualización para los controles del juego.
/// </summary>
public class VisualController : MonoBehaviour
{
    /// <summary>
    /// Enum para definir las manos izquierda y derecha.
    /// </summary>
    private enum Hand { Left, Right }

    [SerializeField] private List<MeshRenderer> _renderers;  // Lista de renderizadores de malla
    [SerializeField] private GameObject _thumbStick;  // Objeto del thumbstick
    [SerializeField] private Hand _hand;  // Mano asociada (izquierda o derecha)
    [SerializeField] private List<InputActionReference> _inputActions;  // Acciones de entrada asociadas

    private MeshRenderer _thumbStickRenderer;  // Renderizador de malla para el thumbstick
    private int _target = -1;  // Índice del objetivo actual

    /// <summary>
    /// Propiedad para acceder a las acciones de entrada.
    /// </summary>
    public List<InputActionReference> InputActions => _inputActions;

    private void Start()
    {
        _thumbStickRenderer = _thumbStick.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        UpdateRenderers();
        UpdateThumbStickColor();
        UpdateThumbStickRotation();
    }

    /// <summary>
    /// Actualiza el color de los renderizadores basados en la entrada del controlador.
    /// </summary>
    private void UpdateRenderers()
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            if (_inputActions[i].action.WasPerformedThisFrame())
            {
                _renderers[i].material.color = Color.green;
                if (_target == i) _target = -1;
            }
            else if (_inputActions[i].action.WasReleasedThisFrame())
            {
                if (_target != i) _renderers[i].material.color = Color.white;
            }
        }
    }

    /// <summary>
    /// Cambia el color del thumbstick en función de las acciones del controlador.
    /// </summary>
    private void UpdateThumbStickColor()
    {
        var lastAction = _inputActions[_inputActions.Count - 1].action;
        if (lastAction.WasPerformedThisFrame())
        {
            _thumbStickRenderer.material.color = Color.green;
        }
        else if (lastAction.WasReleasedThisFrame())
        {
            _thumbStickRenderer.material.color = Color.white;
        }
    }

    /// <summary>
    /// Actualiza la rotación del thumbstick en función de los movimientos del usuario.
    /// </summary>
    private void UpdateThumbStickRotation()
    {
        Vector2 thumb = ThumbStickVector();
        float x = Unity.Mathematics.math.remap(-1f, 1f, -30f, 30f, thumb.y);
        float z = Unity.Mathematics.math.remap(-1f, 1f, -30f, 30f, thumb.x);
        _thumbStick.transform.localRotation = Quaternion.Euler(-x, 0, z);
    }

    /// <summary>
    /// Establece el color rojo para una acción visual específica.
    /// </summary>
    /// <param name="action">Acción visual.</param>
    public void SetRed(VisualAction2 action)
    {
        _renderers[(int)action].material.color = Color.red;
        _target = (int)action;
    }

    /// <summary>
    /// Limpia el color rojo previamente establecido para una acción visual.
    /// </summary>
    /// <param name="action">Acción visual.</param>
    public void CleanRed(VisualAction2 action)
    {
        _renderers[(int)action].material.color = Color.white;
        _target = -1;
    }

    /// <summary>
    /// Obtiene el vector de movimiento del thumbstick.
    /// </summary>
    /// <returns>Vector de movimiento del thumbstick.</returns>
    public Vector2 ThumbStickVector()
    {
        return _inputActions[_inputActions.Count - 1].action.ReadValue<Vector2>();
    }
}
