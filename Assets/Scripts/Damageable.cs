using UnityEngine;
using System.Collections;

public class Damageable : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Efecto visual de daño")]
    public Color flashColor = Color.white;       // Color del parpadeo
    public float flashDuration = 0.1f;           // Duración de cada flash
    public int flashCount = 2;                   // Cuántas veces parpadea

    private Renderer rend;
    private Material matInstance;
    private Color originalColor;

    private void Start()
    {
        currentHealth = maxHealth;

        // Crear instancia única del material (para no afectar a otros objetos)
        rend = GetComponent<Renderer>();
        matInstance = rend.material;
        ColorUtility.TryParseHtmlString("#FF474C", out originalColor);
        matInstance.color = originalColor;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} recibió {damage} de daño. Vida restante: {currentHealth}");

        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
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

    private void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto.");
        // Aquí puedes poner animación o destruir el objeto
        // Destroy(gameObject);
    }
}
