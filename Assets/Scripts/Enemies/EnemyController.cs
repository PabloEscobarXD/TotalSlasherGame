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

    [Header("Ataque a distancia")]
    public GameObject projectilePrefab;
    public Transform firePoint; // punto de disparo, asignar en inspector

    [Header("Comportamiento")]
    public bool prefersRanged = false;

    [Header("Bloqueo")]
    public int hitsToBlock = 3;
    public float blockIdleTimeout = 2f;    // segundos sin daño para dejar de bloquear
    public int hitsToCounterAttack = 3;   // golpes recibidos bloqueando para contraatacar
    private int hitCounter = 0;
    private int blockedHitCounter = 0;
    public bool isBlocking = false;        // público para que EnemyManager lo consulte
    private float timeSinceLastBlockedHit = 0f;
    private Coroutine blockCoroutine;

    [Header("Animación")]
    public Animator animator;

    private float timeSinceLastHit = Mathf.Infinity;

    void Start()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        rb = GetComponent<Rigidbody>();
        health = GetComponent<Damageable>();

        health.OnDeath += () =>
        {
            SetAnimBlock(false);
            health.isBlocking = false;
            isBlocking = false;
            if (fsm != null)
                fsm.ChangeState(new DeadState(this));
        };

        health.OnHit += (attackerPos, attackerTag) =>
        {
            timeSinceLastHit = 0f;

            if (attackerTag == "Player")
            {
                if (isBlocking)
                {
                    SetAnimBlockSuccess();
                    timeSinceLastBlockedHit = 0f;
                    blockedHitCounter++;
                    if (blockedHitCounter >= hitsToCounterAttack)
                    {
                        blockedHitCounter = 0;
                        ExecuteAttack();
                    }
                    return;
                }

                SetAnimHit(); // reacción al daño

                hitCounter++;
                if (hitCounter >= hitsToBlock)
                {
                    hitCounter = 0;
                    if (blockCoroutine != null) StopCoroutine(blockCoroutine);
                    blockCoroutine = StartCoroutine(BlockCoroutine());
                    return;
                }
            }

            if (fsm?.CurrentState is RangedState && attackerTag != "Enemy")
                fsm.ChangeState(new MeleeState(this));
        };

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        fsm = new StateMachine();
        fsm.ChangeState(new EvaluateState(this));
        EnemyManager.Instance?.RegisterEnemy(this);
    }

    void Update()
    {
        timeSinceLastHit += Time.deltaTime;
        fsm.Update();
    }

    // ================================
    //           MÉTODOS DE IA
    // ================================

    public float GetTimeSinceLastHit() => timeSinceLastHit;
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
    public bool IsPlayerInAttackRange()
    {
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= stopDistance + 1.2f;
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
        SetAnimAttack();
        yield return new WaitForSeconds(0.4f); // windup

        float dashDuration = 0.2f;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            if (player != null)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                dir.y = 0;
                rb.linearVelocity = new Vector3(dir.x * moveSpeed * 2.5f, rb.linearVelocity.y, dir.z * moveSpeed * 2.5f);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        if (player != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer <= stopDistance + 1.2f)
            {
                PlayerDamageReceiver receiver = player.GetComponent<PlayerDamageReceiver>();
                receiver?.TakeDamage(transform.position);
            }
        }
    }

    public void ApplyKnockback(Vector3 attackerPosition, Damageable.AttackType attackType = Damageable.AttackType.Normal)
    {
        if (attackType == Damageable.AttackType.Tornado) return;
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

    public void MoveAwayFromPlayer()
    {
        if (!player) return;
        Vector3 dir = (transform.position - player.position).normalized;
        dir.y = 0;
        rb.MovePosition(transform.position + dir * retreatSpeed * Time.deltaTime);
    }

    public void ShootProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        Vector3 dir = (player.position - firePoint.position).normalized;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));
        proj.GetComponent<Projectile>()?.Init(dir);
    }

    private IEnumerator BlockCoroutine()
    {
        isBlocking = true;
        health.isBlocking = true;
        blockedHitCounter = 0;
        timeSinceLastBlockedHit = 0f;

        SetAnimBlock(true);

        while (timeSinceLastBlockedHit < blockIdleTimeout)
        {
            timeSinceLastBlockedHit += Time.deltaTime;
            yield return null;
        }

        SetAnimBlock(false);
        health.isBlocking = false;
        isBlocking = false;
        blockCoroutine = null;
    }

    // ================================
    //      MÉTODOS DE ANIMACIÓN
    // ================================

    public void SetAnimIdle()
    {
        animator?.SetTrigger("idle");
    }

    public void SetAnimRun()
    {
        animator?.SetBool("isRunning", true);
        animator?.SetBool("isRetreat", false);
        animator?.SetBool("isStandby", false);
    }
    public void SetAnimRetreat()
    {
        animator?.SetBool("isRunning", false);
        animator?.SetBool("isRetreat", true);
        animator?.SetBool("isStandby", false);
    }

    public void SetAnimAttack()
    {
        // Variación aleatoria entre 2 ataques
        int variant = Random.Range(0, 2);
        animator?.SetFloat("attackVariant", variant);
        animator?.SetTrigger("attack");
    }

    public void SetAnimHit()
    {
        // Variación aleatoria entre 3 reacciones
        int variant = Random.Range(0, 3);
        animator?.SetInteger("hitVariant", variant);
        animator?.SetTrigger("hit");
    }

    public void SetAnimBlock(bool holding)
    {
        animator?.SetBool("isBlocking", holding);
    }

    public void SetAnimBlockSuccess()
    {
        animator?.SetTrigger("blockSuccess");
    }

    public void SetAnimRangedPrepare()
    {
        animator?.SetTrigger("rangedPrepare");
    }

    public void SetAnimRangedShoot()
    {
        animator?.SetTrigger("rangedShoot");
    }

}
