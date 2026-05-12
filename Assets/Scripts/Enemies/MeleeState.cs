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
        EnemyManager.Instance?.RegisterMeleeEnemy(enemy); // faltaba esto
        Debug.Log("Conehead: Modo Melee");
    }

    public override void Update()
    {
        if (enemy == null) { Debug.LogError("enemy es null"); return; }
        if (EnemyManager.Instance == null) { Debug.LogError("EnemyManager.Instance es null"); return; }

        if (enemy.IsDead())
        {
            enemy.fsm.ChangeState(new DeadState(enemy));
            return;
        }

        enemy.MoveTowardsPlayer();

        // Si prefiere ranged y pasaron X segundos sin recibir daño ? re-evaluar
        if (enemy.prefersRanged && enemy.GetTimeSinceLastHit() > 5f)
        {
            Debug.Log("Ranged sin daño reciente ? EvaluateState");
            enemy.fsm.ChangeState(new EvaluateState(enemy));
            return;
        }

        waveRequestTimer += Time.deltaTime;
        if (waveRequestTimer >= waveRequestInterval)
        {
            if (enemy.IsPlayerInAttackRange())
                EnemyManager.Instance?.RequestAttackWave();
            waveRequestTimer = 0f;
        }

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
