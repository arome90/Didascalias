using Meta.WitAi;
using Meta.WitAi.Data;
using Meta.WitAi.Requests;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la activación y desactivación del servicio de voz mediante un botón.
/// </summary>
public class ActivationButton : MonoBehaviour
{
    [SerializeField]
    private InputActionProperty _talkAction; // Acción de entrada para activar la voz.

    [Tooltip("Referencia al servicio de voz actual")]
    [SerializeField]
    private VoiceService _voiceService;

    [Tooltip("Texto que se muestra cuando el servicio de voz no está activo")]
    [SerializeField]
    private string _activateText = "Activate"; // Texto para mostrar cuando el servicio de voz está desactivado.

    [Tooltip("Si se debe enviar datos al servicio de inmediato o esperar al umbral de audio")]
    [SerializeField]
    private bool _activateImmediately = false; // Indica si se activa de inmediato o espera.

    [Tooltip("Texto que se muestra mientras el servicio de voz está activo")]
    [SerializeField]
    private string _deactivateText = "Deactivate"; // Texto para mostrar cuando el servicio de voz está activado.

    [Tooltip("Si se debe abortar inmediatamente la activación de la solicitud al desactivar")]
    [SerializeField]
    private bool _deactivateAndAbort = false; // Indica si se debe cancelar de inmediato la activación.

    private TextMeshProUGUI _buttonLabel; // Etiqueta del botón que se ajustará según el estado.
    private VoiceServiceRequest _request; // Solicitud actual de servicio de voz.
    private bool _isActive = false; // Estado de activación del servicio de voz.

    /// <summary>
    /// Inicializa el botón y obtiene el componente de texto.
    /// </summary>
    private void Awake()
    {
        _buttonLabel = GetComponentInChildren<TextMeshProUGUI>(); // Busca el componente de texto dentro del objeto hijo.

        // Si no se ha asignado un servicio de voz, busca uno en la escena.
        if (_voiceService == null)
        {
            _voiceService = FindObjectOfType<VoiceService>();
        }
    }

    /// <summary>
    /// Configura los manejadores de acción cuando el componente está habilitado.
    /// </summary>
    private void OnEnable()
    {
        RefreshButtonState(); // Refrescar el estado del botón.

        // Añade el manejador de acción para la acción de "hablar".
        if (_talkAction != null)
        {
            _talkAction.action.performed += HandleTalkAction;
        }
    }

    /// <summary>
    /// Elimina los manejadores de acción cuando el componente está deshabilitado.
    /// </summary>
    private void OnDisable()
    {
        if (_talkAction != null)
        {
            _talkAction.action.performed -= HandleTalkAction;
        }
    }

    /// <summary>
    /// Maneja la acción de hablar (activar/desactivar el servicio de voz).
    /// </summary>
    /// <param name="context">Contexto de la acción de entrada.</param>
    private void HandleTalkAction(InputAction.CallbackContext context)
    {
        if (!_isActive)
        {
            ActivateVoiceService(); // Activa el servicio de voz si no está activo.
        }
        else
        {
            DeactivateVoiceService(); // Desactiva el servicio de voz si está activo.
        }
    }

    /// <summary>
    /// Activa el servicio de voz dependiendo de la configuración.
    /// </summary>
    private void ActivateVoiceService()
    {
        // Dependiendo de si debe activarse inmediatamente o no, llama al método correspondiente.
        _request = _activateImmediately
            ? _voiceService.ActivateImmediately(GetRequestEvents())
            : _voiceService.Activate(GetRequestEvents());
    }

    /// <summary>
    /// Desactiva el servicio de voz dependiendo de la configuración.
    /// </summary>
    private void DeactivateVoiceService()
    {
        // Si se debe cancelar inmediatamente la solicitud, la cancela.
        if (_deactivateAndAbort)
        {
            _request.Cancel();
        }
        else
        {
            _request.DeactivateAudio(); // De lo contrario, solo detiene el audio.
        }
    }

    /// <summary>
    /// Obtiene los eventos de la solicitud de servicio de voz.
    /// </summary>
    /// <returns>Eventos de la solicitud de servicio de voz.</returns>
    private VoiceServiceRequestEvents GetRequestEvents()
    {
        var events = new VoiceServiceRequestEvents();
        events.OnInit.AddListener(OnRequestInit); // Agrega el listener para cuando se inicialice la solicitud.
        events.OnComplete.AddListener(OnRequestComplete); // Agrega el listener para cuando se complete la solicitud.
        return events;
    }

    /// <summary>
    /// Maneja el evento cuando la solicitud se inicializa.
    /// </summary>
    /// <param name="request">Solicitud de servicio de voz.</param>
    private void OnRequestInit(VoiceServiceRequest request)
    {
        _isActive = true; // Cambia el estado a activo.
        RefreshButtonState(); // Actualiza la etiqueta del botón.
    }

    /// <summary>
    /// Maneja el evento cuando la solicitud se completa.
    /// </summary>
    /// <param name="request">Solicitud de servicio de voz.</param>
    private void OnRequestComplete(VoiceServiceRequest request)
    {
        _isActive = false; // Cambia el estado a inactivo.
        RefreshButtonState(); // Actualiza la etiqueta del botón.
    }

    /// <summary>
    /// Actualiza el texto del botón según el estado.
    /// </summary>
    private void RefreshButtonState()
    {
        if (_buttonLabel != null)
        {
            // Cambia el texto dependiendo de si el servicio está activo o no.
            _buttonLabel.text = _isActive ? _deactivateText : _activateText;
        }
    }
}
