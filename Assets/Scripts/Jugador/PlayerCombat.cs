using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using static Damageable;
using TMPro;

[RequireComponent(typeof(TargetingSystem))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Ataque dirigido")]
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    public float stopDistance = 1.5f;
    public float directedAttackCooldown = 0.5f;
    private bool canDirectedAttack = true;

    [Header("Dash Lineal de Furia")]
    public float furyLineDashSpeed = 60f;
    public float furyLineDashDuration = 0.3f;
    public float furyLineDashDamage = 35f;
    public float furyLineDashWidth = 1.5f; // ancho de la hitbox lineal
    public bool isFuryDashing = false; // agregar al header de campos

    [Header("Ataque en área")]
    public float areaDashSpeed = 80f;
    public float areaDashDuration = 0.095f;
    public float maxChargeTime = 1.5f;
    public float minChargeTime = 0.4f;
    public float areaBurstMaxRadius = 5f;

    [Header("Tornado de Furia")]
    public float tornadoRadius = 4f;
    public float tornadoDamage = 10f;
    public int tornadoHits = 5;
    public float tornadoInterval = 0.2f;
    public float tornadoMoveSpeed = 4f;
    public bool isTornado = false;

    [Header("Daño Base")]
    public float dashDamage = 25f;
    public float areaMinDamage = 20f;
    public float areaMaxDamage = 60f;

    [Header("Hitbox")]
    public WeaponHitbox swordHitbox;
    public float swordActiveTime = 0.3f;

    [Header("Bloqueo")]
    public GameObject blockBox;
    public ParticleSystem blockParticles;
    public float blockRotateSpeed = 100f;

    [Header("Referencias")]
    public Animator animator;
    public TMP_Text comboText;
    public GameObject comboGroup;
    private TargetingSystem targeting;
    private Rigidbody rb;
    private FurySystem fury;

    [Header("Combo")]
    private bool isComboActive = false;
    private float comboTimer = 0f;
    public float comboTimeLimit = 5f;
    public int comboCount = 0;


    public bool isDashing = false;
    public bool isCharging = false;
    private float chargeTimer = 0f;

    private Transform currentTarget;
    private bool attackCancelled = false;
    private CameraFollow cameraFollow;

    private PlayerMovement movement;

    private PlayerInput playerInput;

    private Vector3 chargeDirection; // dirección acumulada durante la carga


    void Start()
    {
        targeting = GetComponent<TargetingSystem>();
        rb = GetComponent<Rigidbody>();
        fury = GetComponent<FurySystem>();
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        comboText.text = comboCount.ToString();
        comboGroup.SetActive(false);
        movement = GetComponent<PlayerMovement>();

        isDashing = false;
        isCharging = false;
        isTornado = false;
        isFuryDashing = false;

        playerInput = GetComponent<PlayerInput>();
    }

    void Update()
    {
        comboText.text = comboCount.ToString();

        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            if (movement != null && movement.WorldMoveDirection.sqrMagnitude > 0.01f)
            {
                chargeDirection = movement.WorldMoveDirection; // guardar última válida
                Quaternion targetRot = Quaternion.LookRotation(chargeDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 0.2f));
            }
        }

        if (isComboActive)
        {
            comboGroup.SetActive(true);
            comboTimer += Time.deltaTime;
            if (comboTimer >= comboTimeLimit)
            {
                ScoreManager.Instance?.RegisterCombo(comboCount);
                comboCount = 0;
                comboTimer = 0f;
                isComboActive = false;
                comboGroup.SetActive(false);
            }
        }
    }

    // ---------------- Bloqueo ----------------
    public void Block(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            // Solo los ataques de furia son incancelables
            if (isTornado || isFuryDashing) return;

            // Cancelar todo lo demás
            if (isDashing || isCharging)
                CancelAreaAttack();

            // Detener cualquier corrutina restante y limpiar estado
            StopAllCoroutines();
            playerInput.actions["Move"].Enable();
            rb.linearVelocity = Vector3.zero;
            isDashing = false;
            isCharging = false;
            canDirectedAttack = true;
            swordHitbox.gameObject.SetActive(false);

            int playerLayer = gameObject.layer;
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);

            // Giro hacia enemigo más cercano si hay uno en rango
            Transform nearest = targeting.GetNearestEnemy();
            if (nearest != null)
            {
                Vector3 dir = (nearest.position - transform.position);
                dir.y = 0;
                if (dir.sqrMagnitude > 0.01f)
                    rb.MoveRotation(Quaternion.LookRotation(dir.normalized));
            }

            animator.SetBool("blockHold", true);
            blockBox.SetActive(true);
        }
        else if (ctx.canceled)
        {
            animator.SetBool("blockHold", false);
            blockBox.SetActive(false);
        }
    }

    // ---------------- Ataque dirigido ----------------
    public void Attack(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            if (isFuryDashing || isTornado) return;
            CancelBlock();
            if (!isDashing && canDirectedAttack)
                ExecuteAttack();
            // sin buffer — si está ocupado, el input se ignora
        }
    }

    private void ExecuteAttack()
    {
        playerInput.actions["Move"].Disable();
        StartCoroutine(DirectedAttackCooldown());
        bool furyAttack = fury.IsFuryReady();

        if (furyAttack)
        {
            fury.TriggerSlowmo();
            fury.ConsumeFuryPartial(0.5f);
        }

        Vector3 attackDir = movement != null ? movement.WorldMoveDirection : transform.forward;
        currentTarget = targeting.GetNearestEnemyInDirection(attackDir);

        if (attackDir.sqrMagnitude > 0.01f)
            rb.MoveRotation(Quaternion.LookRotation(attackDir));

        if (furyAttack)
        {
            isDashing = true; // <-- agregar antes de la corrutina
            StartCoroutine(DashTowardsTarget(furyAttack));
        }
        else if (currentTarget != null)
        {
            isDashing = true; // <-- agregar antes de la corrutina
            StartCoroutine(DashTowardsTarget(furyAttack));
        }
        else
        {
            animator.SetTrigger("attackDash");
            playerInput.actions["Move"].Enable();
        }
            
    }

    private IEnumerator DashTowardsTarget(bool furyAttack)
    {
        if (furyAttack)
        {
            yield return StartCoroutine(FuryLineDash());
            playerInput.actions["Move"].Enable();
            yield break;
        }

        // Dash normal (código existente)
        animator.SetTrigger("attackDash");
        float elapsed = 0f;

        while (elapsed < dashDuration && currentTarget != null)
        {
            Vector3 toTarget = currentTarget.position - transform.position;
            toTarget.y = 0;
            if (toTarget.magnitude <= stopDistance) break;

            Vector3 dir = toTarget.normalized;
            rb.linearVelocity = dir * dashSpeed;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(dir), 0.3f));
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        isDashing = false;
        playerInput.actions["Move"].Enable();

        if (currentTarget != null)
        {
            Damageable dmg = currentTarget.GetComponent<Damageable>();
            if (dmg != null)
            {
                if(comboCount == 0)
                    isComboActive = true;
                comboCount++;
                comboTimer = 0;
                dmg.TakeDamage(dashDamage, transform.position, "Player", AttackType.Normal);
            }
            fury.AddFury();
        }
    }

    private IEnumerator FuryLineDash()
    {
        isDashing = true;
        isFuryDashing = true;

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true); // <-- agregar

        Vector3 dashDir = transform.forward;
        float elapsed = 0f;
        HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

        while (elapsed < furyLineDashDuration)
        {
            rb.linearVelocity = dashDir * furyLineDashSpeed;

            Collider[] hits = Physics.OverlapCapsule(
                transform.position,
                transform.position + dashDir * 1.5f,
                furyLineDashWidth,
                targeting.enemyLayer
            );

            foreach (Collider col in hits)
            {
                Damageable dmg = col.GetComponent<Damageable>();
                if (dmg != null && !alreadyHit.Contains(dmg))
                {
                    alreadyHit.Add(dmg);
                    dmg.TakeDamage(furyLineDashDamage, transform.position, "Player", AttackType.Normal);
                    fury.AddFury();
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        isDashing = false;
        isFuryDashing = false;
    }

    // ---------------- Ataque en Área ----------------
    public void AreaAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !isDashing && !isTornado && !isCharging)
        {
            CancelBlock();
            bool furyAttack = fury.IsFuryReady();

            if (furyAttack)
            {
                fury.TriggerSlowmo();
                fury.ConsumeFury();
                StartCoroutine(ExecuteAreaAttack(0f, true));
                return;
            }

            isCharging = true;
            chargeTimer = 0f;
            animator.SetTrigger("areaChargeStart");
            chargeDirection = movement != null && movement.WorldMoveDirection.sqrMagnitude > 0.01f
                ? movement.WorldMoveDirection
                : transform.forward;
            
        }
        else if (ctx.performed && isCharging)
        {
            animator.SetBool("areaChargeHold", true);
        }
        else if (ctx.canceled)
        {
            if (attackCancelled) { attackCancelled = false; return; }
            if (!isCharging) return;

            isCharging = false;
            animator.SetBool("areaChargeHold", false);

            float ratio = Mathf.Clamp01(chargeTimer / maxChargeTime);
            animator.SetTrigger("areaAttackSweep");

            rb.MoveRotation(Quaternion.LookRotation(chargeDirection));
            StartCoroutine(ExecuteAreaAttack(ratio, false, chargeDirection));
        }
    }
    private IEnumerator ExecuteAreaAttack(float ratio, bool furyAttack, Vector3 dashDir = default)
    {
        if (furyAttack)
        {
            yield return StartCoroutine(FuryTornado());
            yield break;
        }

        if (dashDir == default || dashDir.sqrMagnitude < 0.01f)
            dashDir = transform.forward;

        isDashing = true;

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        float elapsed = 0f;
        float damage = Mathf.Lerp(areaMinDamage, areaMaxDamage, ratio);
        HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

        while (elapsed < areaDashDuration)
        {
            rb.linearVelocity = dashDir * areaDashSpeed;

            // Burst activo durante todo el dash
            Collider[] hits = Physics.OverlapSphere(transform.position, areaBurstMaxRadius, targeting.enemyLayer);
            int hitCount = 0;
            foreach (Collider col in hits)
            {
                if (hitCount >= 4) break; // máximo 4 enemigos
                Damageable dmg = col.GetComponent<Damageable>();
                if (dmg != null && !alreadyHit.Contains(dmg))
                {
                    alreadyHit.Add(dmg);
                    dmg.TakeDamage(damage, transform.position, "Player", AttackType.Normal);
                    hitCount++;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        isDashing = false;

        swordHitbox.gameObject.SetActive(true);
        yield return new WaitForSeconds(swordActiveTime);
        swordHitbox.gameObject.SetActive(false);
    }
    private IEnumerator FuryTornado()
    {
        PlayerDamageReceiver receiver = GetComponent<PlayerDamageReceiver>();
        PlayerMovement movement = GetComponent<PlayerMovement>();

        if (receiver != null)
        {
            receiver.isUntouchable = true;
            receiver.damageReductionMultiplier = 0.4f;
        }

        float originalSpeed = 0f;
        if (movement != null)
        {
            originalSpeed = movement.moveForce;
            movement.moveForce = tornadoMoveSpeed;
        }

        isTornado = true;
        // isDashing = true  <-- ELIMINADO, ya no bloquea PlayerMovement

        for (int i = 0; i < tornadoHits; i++)
        {
            DrawDebugCircle(transform.position, tornadoRadius, Color.yellow);

            Collider[] hits = Physics.OverlapSphere(transform.position, tornadoRadius, targeting.enemyLayer);
            foreach (Collider col in hits)
            {
                Damageable dmg = col.GetComponent<Damageable>();
                if (dmg != null)
                    dmg.TakeDamage(tornadoDamage, transform.position, "Player", AttackType.Tornado);
            }

            yield return new WaitForSeconds(tornadoInterval);
        }

        isTornado = false;

        if (movement != null)
            movement.moveForce = originalSpeed;

        if (receiver != null)
        {
            receiver.isUntouchable = false;
            receiver.damageReductionMultiplier = 1f;
        }
    }

    private void DrawDebugCircle(Vector3 center, float radius, Color color)
    {
        int segments = 20;
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Debug.DrawLine(prevPoint, point, color, tornadoInterval);
            prevPoint = point;
        }
    }

    public void CancelAreaAttack()
    {
        Debug.Log($"CancelAreaAttack llamado — isDashing:{isDashing} isCharging:{isCharging}");
        if (isDashing || isCharging)
        {
            StopAllCoroutines();
            playerInput.actions["Move"].Enable();
            rb.linearVelocity = Vector3.zero;
            isDashing = false;
            isCharging = false;
            attackCancelled = true; // marcar cancelación externa
            canDirectedAttack = true;
            animator.SetBool("areaChargeHold", false);
            animator.ResetTrigger("areaChargeStart");
            animator.SetTrigger("areaFailedStart"); // forzar salida de Area_Start
            swordHitbox.gameObject.SetActive(false);

            int playerLayer = gameObject.layer;
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
            Debug.Log("Cancelación ejecutada");
        }
    }

    // ---------------- Bloqueo exitoso ----------------
    public void OnBlockSuccess(Vector3 attackSource)
    {
        Vector3 dir = attackSource - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, blockRotateSpeed * Time.deltaTime);
        }
        animator.SetTrigger("blockHit");
    }
    private void CancelBlock()
    {
        if (blockBox.activeSelf)
        {
            animator.SetBool("blockHold", false);
            blockBox.SetActive(false);
        }
    }

    // ---------------- Input de Modo Furia ----------------
    public void Fury(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            fury.SetFuryMode(true);
        else if (ctx.canceled)
            fury.SetFuryMode(false);
    }
    private IEnumerator DirectedAttackCooldown()
    {
        canDirectedAttack = false;
        yield return new WaitForSeconds(directedAttackCooldown);
        canDirectedAttack = true;
    }

}
