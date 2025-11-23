using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public LayerMask enemyLayer;

    private HashSet<Damageable> alreadyHit = new HashSet<Damageable>();

    private void OnEnable()
    {
        alreadyHit.Clear(); // Limpiar registro al activarse
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("HOLA");
        // Verificar capa del enemigo
        if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        Damageable dmg = other.GetComponent<Damageable>();
        if (dmg != null && !alreadyHit.Contains(dmg))
        {
            alreadyHit.Add(dmg);
            dmg.TakeDamage(damage);
        }
    }
}
