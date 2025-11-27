using UnityEngine;

public class EvaluateState : EnemyState
{
    private float timer;
    private float evaluateCooldown = 1f;   // 🔹 Evaluar solo cada 1 segundo

    public EvaluateState(EnemyController enemy) : base(enemy) { }

    public override void Enter()
    {
        timer = 0f;
        Debug.Log("Cone-head: Evaluando");
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        // 🔹 Si aún no pasó el tiempo, no hacer nada
        if (timer < evaluateCooldown)
            return;

        // Reset timer
        timer = 0f;

        if (enemy.IsDead())
        {
            enemy.fsm.ChangeState(new DeadState(enemy));
            return;
        }

        // 🔹 Si ya está cerca → Melee
        if (enemy.IsPlayerClose())
        {
            enemy.fsm.ChangeState(new MeleeState(enemy));
            return;
        }

        // 🔹 Si no está cerca → igualmente ir a Melee pero para acercarse
        enemy.fsm.ChangeState(new MeleeState(enemy));
    }

}
