using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Sprint")]
    public float sprintMultiplier = 1.6f;

    [Header("Crouch")]
    public Transform cameraTransform;
    public float crouchSpeedMultiplier = 0.5f;
    public float standingCameraHeight = 0.5f;
    public float crouchCameraHeight = 0.1f;
    public float crouchTransitionSpeed = 8f;

    [Header("Camera Bob")]
    public float bobFrequency = 10f;
    public float bobAmplitude = 0.07f;
    public float bobSmoothing = 10f;

    [Header("Pasos")]
    public AudioClip[] footstepClips;
    public float stepDistance = 2f;
    [Range(0f, 1f)] public float footstepVolume = 0.7f;

    [Header("Detección de Suelo")]
    public LayerMask groundLayer;

    private Rigidbody rb;
    private CapsuleCollider col;
    private AudioSource audioSource;
    private Vector3 lastStepPosition;
    private bool isCrouching;
    private float baseCameraY;
    private float bobTimer;
    private float currentBobOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        audioSource = GetComponent<AudioSource>();

        // Congela las rotaciones físicas para que las colisiones jamás hagan girar al personaje
        rb.freezeRotation = true;
        lastStepPosition = transform.position;
        baseCameraY = standingCameraHeight;
    }

    void Update()
    {
        // GetAxisRaw para respuesta instantánea sin desaceleración progresiva
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // Dirección de movimiento alineada con la vista de la cámara
        Vector3 moveDir = (transform.right * moveX + transform.forward * moveZ).normalized;

        isCrouching = Input.GetKey(KeyCode.LeftControl);
        bool isSprinting = !isCrouching && Input.GetKey(KeyCode.LeftShift) && moveDir.magnitude > 0.1f;

        // El agachado tiene prioridad sobre el sprint: no se puede correr agachado
        float currentSpeed = moveSpeed;
        if (isCrouching) currentSpeed *= crouchSpeedMultiplier;
        else if (isSprinting) currentSpeed *= sprintMultiplier;

        if (moveDir.magnitude > 0.1f)
        {
            // Aplicar velocidad al moverse
            Vector3 velocity = moveDir * currentSpeed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        }
        else
        {
            // Detener por completo la velocidad horizontal al soltar las teclas (evita el drift)
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        // Sistema de salto (deshabilitado mientras se está agachado)
        if (Input.GetButtonDown("Jump") && !isCrouching && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        bool isMoving = moveDir.magnitude > 0.1f && IsGrounded();
        UpdateCameraPosition(isMoving);
        UpdateFootsteps();
    }

    private void UpdateCameraPosition(bool isMoving)
    {
        if (cameraTransform == null) return;

        // Baja/sube la cámara suavemente según el estado de agachado
        float targetY = isCrouching ? crouchCameraHeight : standingCameraHeight;
        baseCameraY = Mathf.Lerp(baseCameraY, targetY, Time.deltaTime * crouchTransitionSpeed);

        // Camera bob: oscilación senoidal muy leve que simula el vaivén de la cabeza al caminar
        bobTimer = isMoving ? bobTimer + Time.deltaTime * bobFrequency : 0f;
        float targetBob = isMoving ? Mathf.Sin(bobTimer) * bobAmplitude : 0f;
        currentBobOffset = Mathf.Lerp(currentBobOffset, targetBob, Time.deltaTime * bobSmoothing);

        Vector3 pos = cameraTransform.localPosition;
        pos.y = baseCameraY + currentBobOffset;
        cameraTransform.localPosition = pos;
    }

    private void UpdateFootsteps()
    {
        if (!IsGrounded())
        {
            lastStepPosition = transform.position;
            return;
        }

        // Distancia horizontal recorrida desde el último paso: correr cubre más distancia por segundo,
        // así que el intervalo entre pasos se acelera solo, sin lógica separada para caminar/correr
        Vector3 horizontalDelta = transform.position - lastStepPosition;
        horizontalDelta.y = 0f;

        if (horizontalDelta.magnitude >= stepDistance)
        {
            PlayFootstep();
            lastStepPosition = transform.position;
        }
    }

    private void PlayFootstep()
    {
        if (footstepClips.Length == 0 || audioSource == null) return;
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.PlayOneShot(clip, footstepVolume);
    }

    private bool IsGrounded()
    {
        // Detector de suelo vía Raycast desde la base de la cápsula
        float rayLength = col.bounds.extents.y + 0.1f;
        return Physics.Raycast(transform.position, Vector3.down, rayLength, groundLayer);
    }
}
