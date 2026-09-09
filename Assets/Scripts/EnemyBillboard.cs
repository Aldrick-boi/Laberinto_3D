using UnityEngine;

// Gira el objeto para que siempre mire hacia la cámara, como un sprite 2D en un mundo 3D.
// Necesario porque la geometría visual del enemigo es un plano: sin esto, se volvería
// invisible (una línea) al verlo de canto.
public class EnemyBillboard : MonoBehaviour
{
    private Transform cameraTransform;

    void Start()
    {
        if (Camera.main != null) cameraTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // Solo rota en el eje horizontal para no inclinar el plano hacia arriba/abajo
        Vector3 lookDirection = transform.position - cameraTransform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}
