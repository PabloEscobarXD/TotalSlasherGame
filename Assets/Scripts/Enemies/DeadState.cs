using UnityEngine;
public class DeadState : EnemyState
{
    public DeadState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        Debug.Log("Conehead: Muerto");
        // Animación de muerte
        // Deshabilitar colisiones, IA, NavMesh, etc.
    }

    public override void Update()
    {
        // No hace nada
    }
}
