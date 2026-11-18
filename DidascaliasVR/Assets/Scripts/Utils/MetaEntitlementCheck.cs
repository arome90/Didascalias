using UnityEngine;
using Oculus.Platform;

public class MetaEntitlementCheck : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("ID de la aplicación en Meta Developer Center")]
    [SerializeField] private string appId = "TU_APP_ID_AQUI";

    private void Awake()
    {
        try
        {
            // 1. Inicializar la plataforma de Meta
            Core.AsyncInitialize(appId);

            // 2. Solicitar la verificación de licencia
            Entitlements.IsUserEntitledToApplication().OnComplete(OnEntitlementCheckComplete);
        }
        catch (UnityException e)
        {
            Debug.LogError($"[Meta Platform] Excepción al inicializar: {e.Message}");
            // En caso de fallo crítico de inicialización, cerrar o redirigir
        }
    }

    private void OnEntitlementCheckComplete(Message msg)
    {
        if (msg.IsError)
        {
            Debug.LogError($"[Meta Platform] Entitlement Check FALLIDO: {msg.GetError().Message}");

            // Exigencia VRC de Meta: Si el usuario no tiene licencia, 
            // se debe notificar o cerrar la app limpiamente para evitar pantallas colgadas.
            HandleUnentitledUser();
        }
        else
        {
            Debug.Log("[Meta Platform] Entitlement Check EXITOSO. El usuario tiene licencia válida.");
            // La app continúa su flujo normal (cargar menús, Firebase, etc.)
        }
    }

    private void HandleUnentitledUser()
    {
        // Puedes cerrar la app explícitamente:
        UnityEngine.Application.Quit();
    }
}