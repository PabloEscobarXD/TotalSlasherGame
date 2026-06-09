using UnityEngine;
using System;
using System.Collections;

public class Damageable : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100f;
    private float currentHealth;

[Header("Efecto visual de daño")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;
    public int flashCount = 2;

    // Evento para notificar muerte
    public event Action OnDeath;
    public event Action<Vector3, string> OnHit;

    private Renderer rend;
    private Material matInstance;
    private Color originalColor;

    public bool isBlocking = false;
    public enum AttackType { Normal, Tornado, Projectile }

    private void Start()
    {
        currentHealth = maxHealth;
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            matInstance = rend.material;
            originalColor = matInstance.color; // leer el color que ya tiene
        }
    }

    public void TakeDamage(float damage, Vector3 attackerPosition, string attackerTag = "", AttackType attackType = AttackType.Normal)
    {
        OnHit?.Invoke(attackerPosition, attackerTag); // siempre notificar

        if (isBlocking && attackType != AttackType.Tornado)
        {
            AudioManager.GetOrCreate().PlaySFX3D("enemy_block", transform.position);
            return; // bloquear daño pero hit ya registrado
        }


        AudioManager.GetOrCreate().PlaySFX3D("enemy_hit", transform.position);
        currentHealth -= damage;
        GetComponent<EnemyController>()?.ApplyKnockback(attackerPosition, attackType);
        if (rend != null)
            StartCoroutine(FlashDamage());
        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator FlashDamage()
    {
        for (int i = 0; i < flashCount; i++)
        {
            matInstance.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            matInstance.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto.");
        OnDeath?.Invoke();
        AudioManager.Instance.PlaySFX3D("enemy_death", transform.position);
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0f); // tiempo para que se vea la muerte
        Destroy(gameObject);
    }

}
