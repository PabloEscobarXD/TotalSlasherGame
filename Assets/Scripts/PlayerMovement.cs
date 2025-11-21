using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveForce = 10f;
    public float jumpForce = 250f;

    private Vector2 moveInput;
    private Rigidbody rb;
    private PlayerInput playerInput;

    public PlayerCombat playerCombatIntance;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        if(!playerCombatIntance.isCharging && !playerCombatIntance.isDashing)
        {
            Vector3 velocity = rb.linearVelocity;

            // Dirección cámara (XZ)
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 camRight = Camera.main.transform.right;
            camRight.y = 0;
            camRight.Normalize();

            // Movimiento relativo a cámara
            Vector3 move = (camForward * moveInput.y + camRight * moveInput.x).normalized * moveForce;
            rb.linearVelocity = new Vector3(move.x, velocity.y, move.z);

            // Rotación hacia dirección de movimiento
            if (move.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 0.15f));
            }
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
            rb.AddForce(Vector3.up * jumpForce);
    }
    public void Pause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PauseManager menu = FindAnyObjectByType<PauseManager>();

            if (menu != null)
            {
                menu.TogglePause();
            }
        }
    }

}
