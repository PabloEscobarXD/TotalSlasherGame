using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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

    [Header("Reducción de daño")]
    [Range(0f, 1f)]
    public float damageReductionMultiplier = 1f;

    private bool isStunned = false;

    private Rigidbody rb;
    private PlayerCombat combat;
    private PlayerMovement movement;
    public bool isUntouchable = false; // durante tornado

    private UIShake uiShake;
    private GameObject pauseManagerObject;
    private PauseManager pauseManager;
    public UnityEngine.UI.Button firstButton;

    public GameObject loseCanvas;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<PlayerCombat>();
        movement = GetComponent<PlayerMovement>();
        currentHP = maxHP;
        uiShake = FindAnyObjectByType<UIShake>();
        pauseManagerObject = GameObject.Find("PauseManager");

        if (loseCanvas != null)
            loseCanvas.SetActive(false); // asegurar que empieza desactivado

        rb.isKinematic = false;
    }

    void Update()
    {
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
            Die();
    }

    // -------------------------------------------
    // El enemigo llama a esta función
    // -------------------------------------------
    public void TakeDamage(Vector3 hitSourcePosition, float overrideDamage)
    {
        if (combat != null && combat.blockBox.activeSelf)
        {
            combat.OnBlockSuccess(hitSourcePosition);
            return;
        }
        if (isStunned) return;

        ApplyHealthReduction(overrideDamage);

        if (uiShake == null)
            uiShake = FindAnyObjectByType<UIShake>();

        uiShake?.TriggerShake();

        if (!isUntouchable)
        {
            ApplyKnockback(hitSourcePosition);
            StartCoroutine(ApplyStun());
        }
        AudioManager.GetOrCreate().PlaySFX("player_hit");
    }


    // -------------------------------------------
    // Reduce vida y revisa si muere
    // -------------------------------------------
    void ApplyHealthReduction(float amount)
    {
        currentHP -= amount * damageReductionMultiplier;

        AudioManager.GetOrCreate().SetPlayerHPLoss(1f - (currentHP / maxHP));

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

        // No rotar si está bloqueando
        if (combat != null && combat.blockBox.activeSelf) return;

        //if (direction.sqrMagnitude > 0.01f)
            //rb.MoveRotation(Quaternion.LookRotation(direction));
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
        // Detener todos los enemigos
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (EnemyController enemy in enemies)
            enemy.enabled = false;

        if (pauseManagerObject != null)
            pauseManagerObject.SetActive(false);

        if (loseCanvas != null)
            loseCanvas.SetActive(true);

        if (combat != null)
            combat.enabled = false;

        if (movement != null)
            movement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AudioManager.GetOrCreate().StopMusic();

        // Seleccionar primer botón para el mando
        PlayerInput playerInput = GetComponent<PlayerInput>();
        playerInput?.SwitchCurrentActionMap("UI");

        if (firstButton != null)
        {
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(firstButton.gameObject);
        }

        rb.isKinematic = true;
    }
}
