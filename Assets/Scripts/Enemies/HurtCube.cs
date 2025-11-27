using UnityEngine;

public class HurtCube : MonoBehaviour
{
    public float damage = 10f;

    private void OnTriggerEnter(Collider other)
    {
        PlayerDamageReceiver receiver = other.GetComponent<PlayerDamageReceiver>();

        if (receiver != null)
        {
            receiver.TakeDamage(transform.position);
        }
    }
}
