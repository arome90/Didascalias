using UnityEngine;

public class PeerMovementComponent : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 2f;
    public float gravity = -9.81f;

    [Header("Rotación")]
    public float lookSensitivity = 2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Suavizado")]
    [Range(0f, 20f)] public float moveSmoothness = 10f;
    [Range(0f, 20f)] public float lookSmoothness = 15f;

    // --- Estado de comandos (se setean desde fuera) ---
    private Vector2 moveInput;      // X = lateral, Y = adelante/atrás
    private Vector2 rotationInput;      // X = yaw, Y = pitch
    private bool isSprinting;
    private bool moveUpInput;    // Subir (volar/noclip)
    private bool moveDownInput;  // Bajar

    // --- Estado interno ---
    private float pitch = 0f;
    private float yaw = 0f;
    private Vector3 velocity;
    private Vector3 smoothMoveVelocity;

    public void SetMoveInput(Vector2 direction)
    {
        moveInput = Vector2.ClampMagnitude(direction, 1f);
    }

    public void SetLookInput(Vector2 delta)
    {
        rotationInput = delta;
    }

    public void ApplyNetworkInput(InputData input)
    {
        SetMoveInput(input.move);
        SetLookInput(input.rotation);
    }

    private void ApplyRotation()
    {
        float targetYaw = yaw + rotationInput.x * lookSensitivity;
        float targetPitch = pitch - rotationInput.y * lookSensitivity;

        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        yaw = Mathf.LerpAngle(yaw, targetYaw, Time.deltaTime * lookSmoothness);
        pitch = Mathf.LerpAngle(pitch, targetPitch, Time.deltaTime * lookSmoothness);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Consumir input de look (es un delta, no un estado continuo)
        rotationInput = Vector2.zero;
    }

    void ApplyMovement()
    {
        float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);

        // Movimiento relativo a donde mira la cámara (sin inclinación vertical)
        Vector3 forward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 right = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

        Vector3 targetVelocity = (forward * moveInput.y + right * moveInput.x) * speed;

        // Movimiento vertical libre (noclip / vuelo)
        if (moveUpInput) targetVelocity.y = speed;
        if (moveDownInput) targetVelocity.y = -speed;

        velocity = Vector3.Lerp(velocity, targetVelocity, Time.deltaTime * moveSmoothness);

        transform.position += velocity * Time.deltaTime;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyRotation();
        ApplyMovement();
    }
}
