using UnityEngine;

public class EvaluateState : EnemyState
{
    private float timer;
    private float evaluateCooldown = 1f;   // 🔹 Evaluar solo cada 1 segundo

    public EvaluateState(EnemyController enemy) : base(enemy) 
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        timer = 0f;
        Debug.Log("Cone-head: Evaluando");
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer < evaluateCooldown) return;
        timer = 0f;

        if (enemy.IsDead())
        {
            enemy.fsm.ChangeState(new DeadState(enemy));
            return;
        }

        // Enemigo que prefiere distancia: volver a ranged si hay aliados rodeando
        if (enemy.prefersRanged)
        {
            int meleeCount = EnemyManager.Instance?.GetMeleeCount() ?? 0;
            bool hitRecently = enemy.GetTimeSinceLastHit() < 5f;
            Debug.Log($"prefersRanged eval: timeSinceHit={enemy.GetTimeSinceLastHit():F1} hitRecently={hitRecently} meleeCount={meleeCount}");

            if (!hitRecently && meleeCount >= 1)
            {
                enemy.fsm.ChangeState(new RangedState(enemy));
                return;
            }
            enemy.fsm.ChangeState(new MeleeState(enemy));
            return;
        }

        if (enemy.IsPlayerClose())
        {
            enemy.fsm.ChangeState(new MeleeState(enemy));
            return;
        }

        enemy.fsm.ChangeState(new MeleeState(enemy));
    }

}
