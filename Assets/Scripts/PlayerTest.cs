using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Detección de Suelo")]
    public LayerMask groundLayer;

    private Rigidbody rb;
    private CapsuleCollider col;

void Start()
{
    rb = GetComponent<Rigidbody>();
    col = GetComponent<CapsuleCollider>();

    // Congela las rotaciones físicas para que las colisiones jamás hagan girar al personaje
    rb.freezeRotation = true; 
}

    void Update()
    {
        // GetAxisRaw para respuesta instantánea sin desaceleración progresiva
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        // Dirección de movimiento alineada con la vista de la cámara
        Vector3 moveDir = (transform.right * moveX + transform.forward * moveZ).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            // Aplicar velocidad al moverse
            Vector3 velocity = moveDir * moveSpeed;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        }
        else
        {
            // Detener por completo la velocidad horizontal al soltar las teclas (evita el drift)
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        // Sistema de salto
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        // Detector de suelo vía Raycast desde la base de la cápsula
        float rayLength = col.bounds.extents.y + 0.1f;
        return Physics.Raycast(transform.position, Vector3.down, rayLength, groundLayer);
    }
}