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
    public float furyLineDashWidth = 1.5f;
    public bool isFuryDashing = false;

    [Header("Ataque en área")]
    public float areaDashSpeed = 80f;
    public float areaDashDuration = 0.095f;
    public float maxChargeTime = 1.5f;
    public float minChargeTime = 0.4f;
    public float areaBurstMaxRadius = 5f;
    public float areaCooldown = 0.5f;
    private bool canAreaAttack = true;

    [Header("Ataque en área con furia")]
    public float furyAreaDashSpeed = 80f;
    public float furyAreaDashDuration = 0.095f;
    public float furyAreaDashDamage = 40f;
    public float furyAreaDashRadius = 5f;
    public int furyAreaMaxHits = 5;

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

    public bool isDashing = false;
    public bool isCharging = false;
    private float chargeTimer = 0f;

    private Transform currentTarget;
    private bool attackCancelled = false;
    private CameraFollow cameraFollow;

    private PlayerMovement movement;
    private PlayerInput playerInput;

    private Vector3 chargeDirection;
    private Vector3 lastStickDirection;

    void Start()
    {
        targeting = GetComponent<TargetingSystem>();
        rb = GetComponent<Rigidbody>();
        fury = GetComponent<FurySystem>();
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        comboText.text = $"x{ScoreManager.Instance.GetCurrentMultiplier():F1}";
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
        comboText.text = $"x{ScoreManager.Instance.GetCurrentMultiplier():F1}";

        // Guardar última dirección válida del stick
        if (movement != null && movement.WorldMoveDirection.sqrMagnitude > 0.01f)
            lastStickDirection = movement.WorldMoveDirection;

        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
            if (movement != null && movement.WorldMoveDirection.sqrMagnitude > 0.01f)
            {
                chargeDirection = movement.WorldMoveDirection;
                Quaternion targetRot = Quaternion.LookRotation(chargeDirection);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, 0.2f));
            }
        }

        if (ScoreManager.Instance != null && ScoreManager.Instance.GetCurrentComboKills() > 1)
        {
            comboGroup.SetActive(true);
            comboText.text = $"x{ScoreManager.Instance.GetCurrentMultiplier():F1}";
        }
        else
        {
            comboGroup.SetActive(false);
        }
    }

    // ---------------- Bloqueo ----------------
    public void Block(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            if (isTornado || isFuryDashing) return;

            if (isDashing || isCharging)
                CancelAreaAttack();

            StopAllCoroutines();
            playerInput.actions["Move"].Enable();
            rb.linearVelocity = Vector3.zero;
            isDashing = false;
            isCharging = false;
            canDirectedAttack = true;
            canAreaAttack = true;
            swordHitbox.gameObject.SetActive(false);

            int playerLayer = gameObject.layer;
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);

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

        // Furia: enemigo más lejano. Normal: enemigo más cercano
        if (furyAttack)
            currentTarget = targeting.GetFarthestEnemyInDirection(attackDir);
        else
            currentTarget = targeting.GetNearestEnemyInDirection(attackDir);

        if (attackDir.sqrMagnitude > 0.01f)
            rb.MoveRotation(Quaternion.LookRotation(attackDir));

        if (furyAttack)
        {
            isDashing = true;
            StartCoroutine(DashTowardsTarget(furyAttack));
        }
        else if (currentTarget != null)
        {
            isDashing = true;
            StartCoroutine(DashTowardsTarget(furyAttack));
        }
        else
        {
            animator.SetTrigger("attackDash");
            playerInput.actions["Move"].Enable();
        }
        AudioManager.GetOrCreate().PlaySFX("player_attack_single");
    }

    private IEnumerator DashTowardsTarget(bool furyAttack)
    {
        if (furyAttack)
        {
            yield return StartCoroutine(FuryLineDash());
            playerInput.actions["Move"].Enable();
            yield break;
        }

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
                dmg.TakeDamage(dashDamage, transform.position, "Player", AttackType.Normal);
            fury.AddFury();
        }
    }

    private IEnumerator FuryLineDash()
    {
        isDashing = true;
        isFuryDashing = true;

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        Vector3 dashDir = transform.forward;
        float elapsed = 0f;
        HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

        AudioManager.Instance.PlaySFX("furySingleAttack");

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
        if (ctx.started && !isDashing && !isTornado && !isCharging && canAreaAttack)
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

            chargeDirection = transform.forward;
            if (movement != null && movement.WorldMoveDirection.sqrMagnitude > 0.01f)
                chargeDirection = movement.WorldMoveDirection;

            animator.SetTrigger("areaChargeStart");
            animator.SetBool("areaChargeHold", true);
            AudioManager.GetOrCreate().PlaySFXCancellable("area_charge");
        }
        else if (ctx.canceled)
        {
            if (attackCancelled)
            {
                AudioManager.GetOrCreate().StopSFXCancellable();
                attackCancelled = false;
                return;
            }
            if (!isCharging) return;

            isCharging = false;
            animator.SetBool("areaChargeHold", false);

            float ratio = Mathf.Clamp01(chargeTimer / maxChargeTime);
            animator.SetTrigger("areaAttackSweep");

            AudioManager.GetOrCreate().StopSFXCancellable();
            AudioManager.GetOrCreate().PlaySFX("area_release");

            if (chargeDirection.sqrMagnitude < 0.01f)
                chargeDirection = transform.forward;

            rb.MoveRotation(Quaternion.LookRotation(chargeDirection));
            StartCoroutine(ExecuteAreaAttack(ratio, false, chargeDirection));
        }
    }

    private IEnumerator ExecuteAreaAttack(float ratio, bool furyAttack, Vector3 dashDir = default)
    {
        if (furyAttack)
        {
            yield return StartCoroutine(FuryAreaDash());
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

            Collider[] hits = Physics.OverlapSphere(transform.position, areaBurstMaxRadius, targeting.enemyLayer);
            int hitCount = 0;
            foreach (Collider col in hits)
            {
                if (hitCount >= 5) break; // máximo 5 enemigos
                Damageable dmg = col.GetComponent<Damageable>();
                if (dmg != null && !alreadyHit.Contains(dmg))
                {
                    alreadyHit.Add(dmg);
                    dmg.TakeDamage(damage, transform.position, "Player", AttackType.Normal);
                    fury.AddFury();
                    hitCount++;
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        isDashing = false;
        StartCoroutine(AreaAttackCooldown());

        swordHitbox.gameObject.SetActive(true);
        yield return new WaitForSeconds(swordActiveTime);
        swordHitbox.gameObject.SetActive(false);
    }

    private IEnumerator FuryAreaDash()
    {
        isDashing = true;
        isFuryDashing = true;

        Vector3 dashDir = lastStickDirection.sqrMagnitude > 0.01f
            ? lastStickDirection
            : transform.forward;

        int playerLayer = gameObject.layer;
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        float elapsed = 0f;
        HashSet<Damageable> alreadyHit = new HashSet<Damageable>();
        
        AudioManager.Instance.PlaySFX("furyAreaAttack");
        while (elapsed < furyAreaDashDuration)
        {
            rb.linearVelocity = dashDir * furyAreaDashSpeed;

            Collider[] hits = Physics.OverlapSphere(transform.position, furyAreaDashRadius, targeting.enemyLayer);
            int hitCount = 0;
            foreach (Collider col in hits)
            {
                if (hitCount >= furyAreaMaxHits) break;
                Damageable dmg = col.GetComponent<Damageable>();
                if (dmg != null && !alreadyHit.Contains(dmg))
                {
                    alreadyHit.Add(dmg);
                    dmg.TakeDamage(furyAreaDashDamage, transform.position, "Player", AttackType.Normal);
                    hitCount++;
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

    private IEnumerator AreaAttackCooldown()
    {
        canAreaAttack = false;
        yield return new WaitForSeconds(areaCooldown);
        canAreaAttack = true;
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
        if (isDashing || isCharging)
        {
            StopAllCoroutines();
            playerInput.actions["Move"].Enable();
            rb.linearVelocity = Vector3.zero;
            isDashing = false;
            isCharging = false;
            attackCancelled = true;
            canDirectedAttack = true;
            canAreaAttack = true; // <-- agregar
            animator.SetBool("areaChargeHold", false);
            animator.ResetTrigger("areaChargeStart");
            animator.SetTrigger("areaFailedStart");
            swordHitbox.gameObject.SetActive(false);

            int playerLayer = gameObject.layer;
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
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
        AudioManager.GetOrCreate().PlaySFX("block_success");
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