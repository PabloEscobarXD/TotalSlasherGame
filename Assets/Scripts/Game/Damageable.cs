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

    private Renderer rend;
    private Material matInstance;
    private Color originalColor;

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

    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        currentHealth -= damage;

        GetComponent<EnemyController>()?.ApplyKnockback(attackerPosition);

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

        // Lanza evento para que EnemyController o quien escuche actúe
        OnDeath?.Invoke();

        // Si quieres destruir inmediatamente:
        // Destroy(gameObject);
    }

}
