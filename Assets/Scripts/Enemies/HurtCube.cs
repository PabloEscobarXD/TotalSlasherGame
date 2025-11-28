using UnityEngine;

public class HurtCube : MonoBehaviour
{
    public float damage = 10f;
    private bool wasBlocked = false;  // para evitar múltiples daños tras bloqueo

    private void OnTriggerEnter(Collider other)
    {
        // 1. ¿Golpea el BlockBox?
        BlockBox block = other.GetComponent<BlockBox>();
        if (block != null)
        {
            // Marcar el ataque como bloqueado
            wasBlocked = true;

            // Notificar al jugador que bloqueó este ataque
            block.combat.OnBlockSuccess(transform.position);

            // Llamar a lógica de ataque bloqueado (opcional)
            OnBlocked();

            return; // no hacer daño
        }

        // 2. Si ya fue bloqueado → no hacer daño
        if (wasBlocked)
            return;

        // 3. ¿Golpea al jugador directamente?
        PlayerDamageReceiver receiver = other.GetComponent<PlayerDamageReceiver>();

        if (receiver != null)
        {
            receiver.TakeDamage(transform.position);
        }
    }

    public void OnBlocked()
    {
        Debug.Log("HurtCube: ATAQUE BLOQUEADO");

        // Aquí puedes:
        // - reproducir partículas
        // - desactivar el daño temporalmente
        // - destruir el cubo si simula un ataque que desaparece
    }
}
