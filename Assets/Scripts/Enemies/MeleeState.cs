using UnityEngine;
public class MeleeState : EnemyState
{
    private EnemyController enemy;
    private float waveRequestTimer = 0f;
    private float waveRequestInterval = 2.5f;

    public MeleeState(EnemyController enemy) : base(enemy)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        EnemyManager.Instance?.RegisterMeleeEnemy(enemy);
        enemy.SetAnimRun(); // por defecto al entrar asumimos que viene corriendo
    }

    public override void Update()
    {
        if (enemy.IsDead())
        {
            enemy.fsm.ChangeState(new DeadState(enemy));
            return;
        }

        // Animación según distancia al jugador
        float distance = Vector3.Distance(enemy.transform.position, enemy.GetPlayer().position);
        float inner = enemy.stopDistance - 0.5f;
        float outer = enemy.stopDistance + 0.5f;

        if (distance < inner)
            enemy.SetAnimRetreat();
        else if (distance > outer)
            enemy.SetAnimRun();

        enemy.MoveTowardsPlayer();

        if (enemy.prefersRanged && enemy.GetTimeSinceLastHit() > 5f)
        {
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
    }
}