using UnityEngine;
using static Damageable;

public class Projectile : MonoBehaviour
{
    public float speed = 30f;
    public float damage = 15f;
    public float lifetime = 5f;
    public string ownerTag = "Enemy"; // asignar según quién dispara

    private Vector3 direction;

    public void Init(Vector3 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other) => HandleHit(other);
    private void OnCollisionEnter(Collision other) => HandleHit(other.collider);

    private void HandleHit(Collider other)
    {
        // Intentar hacer daño a cualquier Damageable
        Damageable damageable = other.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, transform.position, ownerTag, AttackType.Projectile);
            Destroy(gameObject);
            return;
        }

        // Intentar hacer daño al jugador
        PlayerDamageReceiver player = other.GetComponent<PlayerDamageReceiver>();
        if (player != null)
        {
            player.TakeDamage(transform.position);
            Destroy(gameObject);
            return;
        }

        // Impactar contra geometría
        Destroy(gameObject);
    }
}