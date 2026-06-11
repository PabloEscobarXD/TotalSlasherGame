using UnityEngine;

public class VoidTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Damageable damageable = other.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.KillSilently(); // sin puntaje ni combo
            return;
        }

        PlayerDamageReceiver player = other.GetComponent<PlayerDamageReceiver>();
        if (player != null)
            player.TakeDamage(transform.position, 99999f);
    }
}