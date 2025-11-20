using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(TargetingSystem))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Ataque dirigido")]
    public float dashSpeed = 30f;       // velocidad de ataque dirigido
    public float dashDuration = 0.2f;   // tiempo del dash

    [Header("Ataque en área (hold)")]
    public float areaDashSpeed = 80f;
    public float areaDashDuration = 0.095f;
    public float maxChargeTime = 1.5f;
    public float minChargeTime = 0.3f;

    [Header("Daño")]
    public float dashDamage = 25f;
    public float areaMinDamage = 20f;
    public float areaMaxDamage = 60f;

    [Header("Hitbox")]
    public WeaponHitbox swordHitbox;
    public float swordActiveTime = 0.3f; // cuánto dura activa la hitbox en el ataque en área

    public bool isDashing = false;
    public bool isCharging = false;
    private bool isBlocking = false;
    private float chargeTimer = 0f;

    public Animator animator;
    private TargetingSystem targetingSystem;
    private Rigidbody rb;
    private Transform currentTarget;

    void Start()
    {
        targetingSystem = GetComponent<TargetingSystem>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isCharging)
        {
            chargeTimer += Time.deltaTime;
        }
    }

    // ---------------- Bloqueo ----------------
    public void Block(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            animator.SetTrigger("blockStart");
        }
        else if(context.performed)
        {
            animator.SetBool("blockHold", true);
        }
        else if (context.canceled)
        {
            animator.SetBool("blockHold", false);
        }

    }

    // ---------------- Ataque dirigido ----------------
    public void Attack(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing)
        {
            currentTarget = targetingSystem.GetNearestEnemy();

            if (currentTarget != null)
            {
                // Evitar corrutinas duplicadas
                StopAllCoroutines();
                StartCoroutine(DashTowardsTarget(currentTarget));
            }
            else
            {
                // Ataque básico sin target
                animator.SetTrigger("attackDash");
            }
        }
    }

    private System.Collections.IEnumerator DashTowardsTarget(Transform target)
    {
        isDashing = true;
        animator.SetTrigger("attackDash");

        float elapsed = 0f;

        while (elapsed < dashDuration && target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            rb.linearVelocity = direction * dashSpeed;

            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion lookRot = Quaternion.LookRotation(direction, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, lookRot, 0.3f));
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        isDashing = false;

        // ✅ Aplicar daño al final del dash
        if (target != null)
        {
            Damageable dmg = target.GetComponent<Damageable>();
            if (dmg != null)
                dmg.TakeDamage(dashDamage);
        }
    }

    // ---------------- Ataque en área (hold + release) ----------------
    public void AreaAttack(InputAction.CallbackContext context)
    {
        if (context.started && !isDashing)
        {
            // Comienza la carga
            isCharging = true;
            chargeTimer = 0f;
            animator.SetTrigger("areaChargeStart");
        }
        else if (context.performed && isCharging)
        {
            // Unity dispara esto cuando supera el pressPoint (0.1)
            // Aquí solo ponemos el bool en true, no revisamos tiempos aún
            animator.SetBool("areaChargeHold", true);
        }
        else if (context.canceled && isCharging)
        {
            // Soltar el botón
            isCharging = false;
            animator.SetBool("areaChargeHold", false);

            if (chargeTimer >= minChargeTime)
            {
                // Ataque válido
                animator.ResetTrigger("areaFailedStart");
                animator.SetTrigger("areaAttackSweep");

                float chargeRatio = Mathf.Clamp01(chargeTimer / maxChargeTime);
                StartCoroutine(ExecuteAreaAttack(chargeRatio));
            }
            else
            {
                // Ataque fallido
                animator.SetTrigger("areaFailedStart");
            }
        }
    }
    private System.Collections.IEnumerator ExecuteAreaAttack(float chargeRatio)
    {
        isDashing = true;

        float elapsed = 0f;
        Vector3 dashDir = transform.forward;
        float finalSpeed = areaDashSpeed * (1f + chargeRatio);
        float damage = Mathf.Lerp(areaMinDamage, areaMaxDamage, chargeRatio);

        // ✅ Activar hitbox
        swordHitbox.damage = damage;
        swordHitbox.enemyLayer = targetingSystem.enemyLayer;
        swordHitbox.gameObject.SetActive(true);

        // Movimiento del dash
        while (elapsed < areaDashDuration)
        {
            rb.linearVelocity = dashDir * finalSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        isDashing = false; // <-- el movimiento termina aquí

        // ✅ Mantener la hitbox activa solo por el tiempo que corresponda
        yield return new WaitForSeconds(swordActiveTime);
        swordHitbox.gameObject.SetActive(false);
    }




}
