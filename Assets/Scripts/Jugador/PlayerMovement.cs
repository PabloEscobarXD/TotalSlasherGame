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
    public Animator animator;
    public Vector3 WorldMoveDirection { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        bool isBlocking = playerCombatIntance.blockBox.activeSelf;
        animator.SetBool("isPlayerMoving", moveInput.sqrMagnitude > 0.01f && !isBlocking);
    }

    void FixedUpdate()
    {
        Vector3 camForward = Camera.main.transform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = Camera.main.transform.right; camRight.y = 0; camRight.Normalize();
        WorldMoveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        bool canMove = !playerCombatIntance.isCharging || playerCombatIntance.isTornado;
        if (canMove)
        {
            Vector3 velocity = rb.linearVelocity;
            Vector3 move = WorldMoveDirection * moveForce;
            rb.linearVelocity = new Vector3(move.x, velocity.y, move.z);

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
                menu.TogglePause();
        }
    }
}