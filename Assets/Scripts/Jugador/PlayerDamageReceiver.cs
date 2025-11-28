using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDamageReceiver : MonoBehaviour
{
    [Header("Vida del jugador")]
    public float maxHP = 100f;
    public float currentHP;

    [Header("Efectos de daño")]
    public float invincibleTime = 0.3f;      // tiempo sin poder hacer nada
    public float knockbackForce = 10f;       // fuerza del empujón
    public float damage = 10f;               // daño de prueba (HurtCube)

    private bool isStunned = false;

    private Rigidbody rb;
    private PlayerCombat combat;
    private PlayerMovement movement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();

        currentHP = maxHP;   // iniciar vida llena
    }

    // -------------------------------------------
    // El enemigo llama a esta función
    // -------------------------------------------
    public void TakeDamage(Vector3 hitSourcePosition)
    {
        // Verificar si está bloqueando
        if (combat != null && combat.blockBox.activeSelf)
        {
            // El bloqueo absorbe el golpe → NO recibir daño
            combat.OnBlockSuccess(hitSourcePosition);
            return;
        }

        // Daño normal
        if (isStunned) return;

        ApplyHealthReduction(damage);
        ApplyKnockback(hitSourcePosition);
        StartCoroutine(ApplyStun());
    }


    // -------------------------------------------
    // Reduce vida y revisa si muere
    // -------------------------------------------
    void ApplyHealthReduction(float amount)
    {
        currentHP -= amount;

        Debug.Log("[PLAYER] Recibió daño. HP actual = " + currentHP);

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    // -------------------------------------------
    void ApplyKnockback(Vector3 source)
    {
        Vector3 direction = (transform.position - source).normalized;
        direction.y = 0;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
    }

    // -------------------------------------------
    private System.Collections.IEnumerator ApplyStun()
    {
        isStunned = true;

        combat.isDashing = true;  // bloquear ataques
        movement.enabled = false; // bloquear movimiento

        yield return new WaitForSeconds(invincibleTime);

        combat.isDashing = false;
        movement.enabled = true;

        isStunned = false;
    }

    // -------------------------------------------
    void Die()
    {
        Debug.Log("PLAYER MUERTO");
        // Aquí puedes agregar animación death, respawn, etc.
    }
}
