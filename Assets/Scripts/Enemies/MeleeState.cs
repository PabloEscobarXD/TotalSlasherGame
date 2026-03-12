using UnityEngine;

public class MeleeState : EnemyState
{
    private EnemyController enemy;
    private float waveRequestTimer = 0f;
    private float waveRequestInterval = 2.5f; // cada cuánto pide una ola
    public MeleeState(EnemyController enemy) : base(enemy) 
    {
        this.enemy = enemy;
    }
    public override void Enter()
    {
        // Aquí puedes poner animación de caminar/atacar
        EnemyManager.Instance?.RegisterMeleeEnemy(enemy);
        Debug.Log("Conehead: Modo Melee");
    }

    public override void Update()
    {
        if (enemy == null) { Debug.LogError("enemy es null"); return; }
        if (EnemyManager.Instance == null) { Debug.LogError("EnemyManager.Instance es null"); return; }
        // Si muere, cambiamos a Dead
        if (enemy.IsDead())
        {
            enemy.fsm.ChangeState(new DeadState(enemy));
            return;
        }

        // Movimiento hacia el jugador
        enemy.MoveTowardsPlayer();

        waveRequestTimer += Time.deltaTime;
        if (waveRequestTimer >= waveRequestInterval)
        {
            EnemyManager.Instance?.RequestAttackWave();
            waveRequestTimer = 0f;
        }

        // Si el jugador está lejos
        if (!enemy.IsPlayerClose())
        {
            enemy.fsm.ChangeState(new EvaluateState(enemy));
            return;
        }
    }

    public override void Exit()
    {
        EnemyManager.Instance?.UnregisterMeleeEnemy(enemy);
        // Reset de animaciones si hace falta
    }
}
