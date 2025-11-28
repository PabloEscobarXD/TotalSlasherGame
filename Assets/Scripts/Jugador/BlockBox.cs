using UnityEngine;

public class BlockBox : MonoBehaviour
{
    public PlayerCombat combat;

    private void OnTriggerEnter(Collider other)
    {
        // ¿El objeto que golpea tiene Damageable o HurtBox?
        HurtCube hurt = other.GetComponent<HurtCube>();
        if (hurt != null)
        {
            // Pasar la posición del ataque para orientar al jugador
            combat.OnBlockSuccess(hurt.transform.position);

            // Consumir el ataque / impedir que siga dañando
            hurt.OnBlocked();
        }
    }
}
