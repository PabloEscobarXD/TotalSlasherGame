using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 5, -8);
    public float followSpeed = 10f;
    public float rotationSmooth = 5f;
    public float maxFollowAngle = 60f;

    [Header("Control manual de cámara")]
    public float rotateSpeed = 120f;
    public float zoomSpeed = 5f;
    public float minZoom = 4f;
    public float maxZoom = 15f;

    private float currentYaw;
    private bool manualControl = false; // true cuando el jugador mueve el stick
    private PlayerInput playerInput;
    public bool lockRotation = false;

    private void Start()
    {
        // Buscar PlayerInput en el jugador
        playerInput = FindAnyObjectByType<PlayerInput>();
        currentYaw = target != null ? target.eulerAngles.y : 0f;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector2 camInput = Vector2.zero;
        if (playerInput != null)
            camInput = playerInput.actions["CameraMovement"].ReadValue<Vector2>();

        if (!lockRotation)
        {
            if (Mathf.Abs(camInput.x) > 0.1f)
            {
                currentYaw += camInput.x * rotateSpeed * Time.deltaTime;
                manualControl = true;
            }
            else
            {
                manualControl = false;
            }

            if (!manualControl)
            {
                Vector3 forward = target.forward;
                forward.y = 0;
                Vector3 camForward = transform.forward;
                camForward.y = 0;
                float angle = Vector3.Angle(camForward, forward);
                if (angle < maxFollowAngle)
                {
                    float targetYaw = target.eulerAngles.y;
                    currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, rotationSmooth * Time.deltaTime);
                }
            }
        }

        if (Mathf.Abs(camInput.y) > 0.1f)
        {
            float currentDist = offset.magnitude;
            float newDist = Mathf.Clamp(currentDist - camInput.y * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
            offset = offset.normalized * newDist;
        }

        Quaternion rotation = Quaternion.Euler(0, currentYaw, 0);
        Vector3 desiredPosition = target.position + rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 3f);
    }
}