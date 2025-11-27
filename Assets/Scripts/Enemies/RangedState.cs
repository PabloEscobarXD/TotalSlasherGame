using System.Xml;
using UnityEngine;

public class RangedState : EnemyState
{
    public RangedState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        Debug.Log("Conehead: Modo Ranged");
        // Lanzar animación de apuntar, etc.
    }

    public override void Update()
    {
        if (enemy.IsDead())
        {
            enemy.fsm.ChangeState(new DeadState(enemy));
            return;
        }

        // Si el jugador se metió demasiado cerca, re-evaluar
        if (enemy.IsPlayerClose())
        {
            enemy.fsm.ChangeState(new EvaluateState(enemy));
            return;
        }

        // Aquí debería ir la lógica de disparo de proyectiles
        // enemy.ShootProjectile();
    }

    public override void Exit()
    {
    }
}
