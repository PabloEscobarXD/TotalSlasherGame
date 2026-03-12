using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Damageable))]
public class EnemyController : MonoBehaviour
{
    [Header("Percepción")]
    public float meleeRange = 20f;
    public float rangedRange = 10f;
    public float stopDistance = 5f;   // Mantener distancia mínima

    [Header("Movimiento")]
    public float moveSpeed = 17f;
    public float retreatSpeed = 20f; // mayor que moveSpeed para simular retroceso rápido

    [Header("Knockback")]
    public float knockbackDistance = 1.5f;
    public float knockbackDuration = 0.15f;
    private bool isKnockedBack = false;

    [HideInInspector] public StateMachine fsm;
    private Transform player;
    private Rigidbody rb;
    private Damageable health;

    [Header("Ataque")]
    public float attackDuration = 0.6f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Damageable>();

        // Evento de muerte → cambiar estado
        health.OnDeath += () =>
        {
            if (fsm != null)
                fsm.ChangeState(new DeadState(this));
        };

        // Buscar un player en la escena
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Crear FSM
        fsm = new StateMachine();
        fsm.ChangeState(new EvaluateState(this));
    }

    void Update()
    {
        fsm.Update();
    }

    // ================================
    //           MÉTODOS DE IA
    // ================================

    public bool IsDead()
    {
        return health != null && health.IsDead();
    }

    public bool IsPlayerClose()
    {
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= meleeRange;
    }

    public bool IsPlayerInRanged()
    {
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= rangedRange;
    }

    public void MoveTowardsPlayer()
    {
        if (!player) return;

        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        // Girar siempre hacia el jugador
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                0.1f
            );
        }

        // --- Dead zone ---
        float innerLimit = stopDistance - 0.5f; // empieza a retroceder aquí
        float outerLimit = stopDistance + 0.5f; // deja de retroceder aquí

        if (distance < innerLimit)
        {
            // Demasiado cerca → retroceder
            rb.MovePosition(transform.position - dir * retreatSpeed * Time.deltaTime);
        }
        else if (distance > outerLimit)
        {
            // Demasiado lejos → avanzar
            rb.MovePosition(transform.position + dir * moveSpeed * Time.deltaTime);
        }
        // Entre innerLimit y outerLimit → no hacer nada (zona muerta)
    }

    public Transform GetPlayer()
    {
        return player;
    }

    public void ExecuteAttack()
    {
        StartCoroutine(AttackCoroutine());
    }
    private IEnumerator AttackCoroutine()
    {
        // Por ahora: flash rojo como representación del ataque
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            Color original = rend.material.color;
            rend.material.color = Color.red;
            yield return new WaitForSeconds(attackDuration);
            rend.material.color = original;
        }
    }

    public void ApplyKnockback(Vector3 attackerPosition)
    {
        if (isKnockedBack) return;
        StartCoroutine(KnockbackCoroutine(attackerPosition));
    }
    private IEnumerator KnockbackCoroutine(Vector3 attackerPosition)
    {
        isKnockedBack = true;

        Vector3 dir = (transform.position - attackerPosition).normalized;
        dir.y = 0;
        Vector3 targetPos = transform.position + dir * knockbackDistance;

        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;
            rb.MovePosition(Vector3.Lerp(startPos, targetPos, t));
            yield return null;
        }

        isKnockedBack = false;
    }

}
