using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;

    void Start()
    {
        // Oculta y bloquea el puntero en el centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        // Permite mirar completamente hacia arriba y hacia abajo (90 grados)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Aplica la rotación vertical a la cámara
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Aplica la rotación horizontal al cuerpo del personaje
        playerBody.Rotate(Vector3.up * mouseX);
    }
}