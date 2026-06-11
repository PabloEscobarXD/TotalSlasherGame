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
    public float mouseZoomSpeed = 0.5f;

    [Header("Cinemática")]
    public bool cinematicMode = false;
    public Vector3 cinematicOffset = new Vector3(0, 8f, -12f);
    public Vector3 cinematicStartOffset = new Vector3(0, 8f, 20f); // lado opuesto
    public Vector3 cinematicLookOffset = new Vector3(0, 3f, 0);
    private Vector3 cinematicStartPos;
    private Vector3 cinematicEndPos;
    private float cinematicDuration = 0f;
    private float cinematicElapsed = 0f;

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

        if (cinematicMode)
        {
            cinematicElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(cinematicElapsed / cinematicDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t); // easing suave

            // Actualizar posición final en caso de que el jugador se mueva
            Quaternion _rotation = Quaternion.Euler(0, currentYaw, 0);
            cinematicEndPos = target.position + _rotation * offset;

            transform.position = Vector3.Lerp(cinematicStartPos, cinematicEndPos, smoothT);
            transform.LookAt(target.position + cinematicLookOffset);
            return;
        }

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

        // Zoom con stick derecho (camInput.y)
        if (Mathf.Abs(camInput.y) > 0.1f)
        {
            float currentDist = offset.magnitude;
            float newDist = Mathf.Clamp(currentDist - camInput.y * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
            offset = offset.normalized * newDist;
        }

        // ← NUEVO: Zoom con scroll del mouse (acción separada)
        if (playerInput != null)
        {
            Vector2 scroll = playerInput.actions["CameraZoom"].ReadValue<Vector2>();
            if (Mathf.Abs(scroll.y) > 0.01f)
            {
                float currentDist = offset.magnitude;
                float newDist = Mathf.Clamp(currentDist - scroll.y * mouseZoomSpeed, minZoom, maxZoom);
                offset = offset.normalized * newDist;
            }
        }

        Quaternion rotation = Quaternion.Euler(0, currentYaw, 0);
        Vector3 desiredPosition = target.position + rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 3f);
    }
    public void StartCinematic(Vector3 waveCenter, float duration)
    {
        cinematicMode = true;
        cinematicDuration = duration;
        cinematicElapsed = 0f;
        lockRotation = true;

        // Arrancar desde el lado opuesto de la arena
        cinematicStartPos = waveCenter + cinematicStartOffset;

        // Terminar en la posición normal del jugador
        Quaternion rotation = Quaternion.Euler(0, currentYaw, 0);
        cinematicEndPos = target.position + rotation * offset;

        // Teletransportar la cámara al punto de inicio
        transform.position = cinematicStartPos;
    }

    public void EndCinematic()
    {
        cinematicMode = false;
        lockRotation = false;
    }
}