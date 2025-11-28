using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(TargetingSystem))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Ataque dirigido")]
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;

    [Header("Ataque en área")]
    public float areaDashSpeed = 80f;
    public float areaDashDuration = 0.095f;
    public float maxChargeTime = 1.5f;
    public float minChargeTime = 0.4f;

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
    private TargetingSystem targeting;
    private Rigidbody rb;
    private FurySystem fury;

    public bool isDashing = false;
    public bool isCharging = false;
    private float chargeTimer = 0f;

    private Transform currentTarget;

    void Start()
    {
        targeting = GetComponent<TargetingSystem>();
        rb = GetComponent<Rigidbody>();
        fury = GetComponent<FurySystem>();
    }

    void Update()
    {
        if (isCharging)
            chargeTimer += Time.deltaTime;
    }

    // ---------------- Bloqueo ----------------
    public void Block(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            animator.SetTrigger("blockStart");
            blockBox.SetActive(true);
        }
        else if (ctx.performed)
        {
            animator.SetBool("blockHold", true);
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
        if (ctx.performed && !isDashing)
        {
            bool furyAttack = fury.IsFuryReady();

            if (furyAttack)
                fury.TriggerSlowmo();

            currentTarget = targeting.GetNearestEnemy();

            if (currentTarget != null)
                StartCoroutine(DashTowardsTarget(furyAttack));
            else
                animator.SetTrigger("attackDash");
        }
    }

    private IEnumerator DashTowardsTarget(bool furyAttack)
    {
        isDashing = true;
        animator.SetTrigger("attackDash");

        float elapsed = 0f;

        while (elapsed < dashDuration && currentTarget != null)
        {
            Vector3 dir = (currentTarget.position - transform.position).normalized;
            dir.y = 0;

            rb.linearVelocity = dir * dashSpeed;

            Quaternion rot = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, rot, 0.3f));

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        isDashing = false;

        // Aplicar daño
        if (currentTarget != null)
        {
            Damageable dmg = currentTarget.GetComponent<Damageable>();
            if (dmg != null)
            {
                float finalDamage = dashDamage * fury.GetDamageMultiplier();
                dmg.TakeDamage(finalDamage);

                if (furyAttack)
                    fury.ConsumeFury();
                else
                    fury.AddFury();
            }
        }
    }

    // ---------------- Ataque en Área ----------------
    public void AreaAttack(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !isDashing)
        {
            isCharging = true;
            chargeTimer = 0f;
            animator.SetTrigger("areaChargeStart");
        }
        else if (ctx.performed && isCharging)
        {
            animator.SetBool("areaChargeHold", true);
        }
        else if (ctx.canceled && isCharging)
        {
            isCharging = false;
            animator.SetBool("areaChargeHold", false);

            if (chargeTimer >= minChargeTime)
            {
                float ratio = Mathf.Clamp01(chargeTimer / maxChargeTime);
                bool furyAttack = fury.IsFuryReady();

                if (furyAttack)
                    fury.TriggerSlowmo();

                animator.SetTrigger("areaAttackSweep");

                StartCoroutine(ExecuteAreaAttack(ratio, furyAttack));
            }
            else
            {
                animator.SetTrigger("areaFailedStart");
            }
        }
    }

    private IEnumerator ExecuteAreaAttack(float ratio, bool furyAttack)
    {
        isDashing = true;

        float elapsed = 0f;
        Vector3 dashDir = transform.forward;
        float speed = areaDashSpeed * (1f + ratio);

        float damage = Mathf.Lerp(areaMinDamage, areaMaxDamage, ratio);
        damage *= fury.GetDamageMultiplier();

        swordHitbox.damage = damage;
        swordHitbox.enemyLayer = targeting.enemyLayer;
        swordHitbox.gameObject.SetActive(true);

        while (elapsed < areaDashDuration)
        {
            rb.linearVelocity = dashDir * speed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        isDashing = false;

        yield return new WaitForSeconds(swordActiveTime);
        swordHitbox.gameObject.SetActive(false);

        if (furyAttack)
            fury.ConsumeFury();
        else
            fury.AddFury();
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

        blockParticles?.Play();
        animator.SetTrigger("blockHit");
    }

    // ---------------- Input de Modo Furia ----------------
    public void Fury(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            fury.SetFuryMode(true);
        else if (ctx.canceled)
            fury.SetFuryMode(false);
    }
}
