using System.Security.Cryptography;
using UnityEngine;
using static Damageable;

public class Projectile : MonoBehaviour
{
    public float speed = 30f;
    public float damage = 15f;
    public float lifetime = 5f;
    public string ownerTag = "Enemy"; // asignar según quién dispara
    private Rigidbody rb;

    private Vector3 direction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Init(Vector3 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        float moveDist = speed * Time.deltaTime;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, moveDist + 0.1f))
        {
            HandleHit(hit.collider);
            return;
        }

        transform.position += direction * moveDist;
    }

    private void OnTriggerEnter(Collider other) => HandleHit(other);
    private void OnCollisionEnter(Collision other) => HandleHit(other.collider);

    private void HandleHit(Collider other)
    {
        // Verificar primero si es el ParryBox
        if (other.CompareTag("ParryBox"))
        {
            PlayerCombat combat = other.GetComponentInParent<PlayerCombat>();
            if (combat != null)
            {
                combat.OnBlockSuccess(transform.position);
                Destroy(gameObject);
                return;
            }
        }

        // Verificar jugador
        PlayerDamageReceiver player = other.GetComponentInParent<PlayerDamageReceiver>();
        if (player != null)
        {
            player.TakeDamage(transform.position);
            Destroy(gameObject);
            return;
        }

        // Verificar damageable (enemigos u otros)
        Damageable damageable = other.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, transform.position, ownerTag, AttackType.Projectile);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}