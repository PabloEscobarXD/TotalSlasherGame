using UnityEngine;

public class MeleeState : EnemyState
{
    public MeleeState(EnemyController enemy) : base(enemy) { }
    public override void Enter()
    {
        // Aquí puedes poner animación de caminar/atacar
        Debug.Log("Conehead: Modo Melee");
    }

    public override void Update()
    {
        // Si muere, cambiamos a Dead
        if (enemy.IsDead())
        {
            enemy.fsm.ChangeState(new DeadState(enemy));
            return;
        }

        // Si el jugador está lejos
        if (!enemy.IsPlayerClose())
        {
            enemy.fsm.ChangeState(new EvaluateState(enemy));
            return;
        }

        // Movimiento hacia el jugador
        enemy.MoveTowardsPlayer();

        // Aquí podrías añadir:
        // - Lógica de atacar según distancia
        // - Ritmo de ataque
        // - Bloquear si recibe X golpes
    }

    public override void Exit()
    {
        // Reset de animaciones si hace falta
    }
}
