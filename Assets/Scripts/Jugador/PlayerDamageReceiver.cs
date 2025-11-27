using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDamageReceiver : MonoBehaviour
{
    public float invincibleTime = 0.3f;      // tiempo sin poder hacer nada
    public float knockbackForce = 10f;       // fuerza del empujón
    public float damage = 10f;               // para pruebas con HurtCube

    private bool isStunned = false;
    private Rigidbody rb;
    private PlayerCombat combat;
    private PlayerMovement movement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();
    }

    public void TakeDamage(Vector3 hitSourcePosition)
    {
        if (isStunned) return;

        Debug.Log("[PLAYER] Recibió daño");

        StartCoroutine(ApplyStun());
        ApplyKnockback(hitSourcePosition);
    }

    void ApplyKnockback(Vector3 source)
    {
        Vector3 direction = (transform.position - source).normalized;
        direction.y = 0;

        rb.linearVelocity = Vector3.zero;  // reset
        rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
    }

    System.Collections.IEnumerator ApplyStun()
    {
        isStunned = true;

        combat.isDashing = true;      // bloquea ataques
        movement.enabled = false;     // bloquea movimiento del PlayerMovement

        yield return new WaitForSeconds(invincibleTime);

        combat.isDashing = false;
        movement.enabled = true;

        isStunned = false;
    }
}
