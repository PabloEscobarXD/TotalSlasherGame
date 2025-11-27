using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -8);
    public float followSpeed = 10f;
    public float rotationSmooth = 5f;
    public float maxFollowAngle = 60f;   // Ángulo máximo para seguir la rotación

    private float currentYaw;

    private void LateUpdate()
    {
        if (target == null) return;

        // Dirección de movimiento del jugador (proyectada en el plano XZ)
        Vector3 forward = target.forward;
        forward.y = 0;

        // Dirección actual de la cámara
        Vector3 camForward = transform.forward;
        camForward.y = 0;

        // Ángulo entre cámara y jugador
        float angle = Vector3.Angle(camForward, forward);

        // Si el ángulo es menor al límite -> seguimos rotación del jugador
        if (angle < maxFollowAngle)
        {
            float targetYaw = target.eulerAngles.y;
            currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, rotationSmooth * Time.deltaTime);
        }

        // Rotación final de la cámara (cuando no sigue, se queda en su último yaw)
        Quaternion rotation = Quaternion.Euler(0, currentYaw, 0);

        // Calculamos posición en base al yaw actual
        Vector3 desiredPosition = target.position + rotation * offset;

        // Movimiento suave
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Que mire al jugador (puedes ajustar altura si lo deseas)
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}
