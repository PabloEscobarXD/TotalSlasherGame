using UnityEngine;
public class RangedState : EnemyState
{
    private EnemyController enemy;
    private float shootTimer = 0f;
    public float shootInterval = 4f;
    private Vector3 disperseDir;
    private float disperseTimer = 0f;
    private float disperseDuration = 1.5f;
    private bool isPreparing = false;
    private float prepareTime = 0.8f; // tiempo de animación de preparación antes de disparar

    public RangedState(EnemyController enemy) : base(enemy)
    {
        this.enemy = enemy;
    }

    public override void Enter()
    {
        EnemyManager.Instance?.RegisterRangedEnemy(enemy);
        disperseDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        disperseTimer = 0f;
        enemy.SetAnimRetreat(); // dispersión inicial = retroceder
        Debug.Log("Conehead: Modo Ranged");
    }

    public override void Update()
    {
        if (enemy.IsDead()) { enemy.fsm.ChangeState(new DeadState(enemy)); return; }

        // Dispersión inicial
        if (disperseTimer < disperseDuration)
        {
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            rb?.MovePosition(enemy.transform.position + disperseDir * enemy.retreatSpeed * Time.deltaTime);
            disperseTimer += Time.deltaTime;
            shootTimer += Time.deltaTime;
            return;
        }

        // Movimiento — alejarse si el jugador está muy cerca
        if (Vector3.Distance(enemy.transform.position, enemy.GetPlayer().position) <= enemy.rangedRange)
        {
            enemy.MoveAwayFromPlayer();
            enemy.SetAnimRetreat();
        }

        shootTimer += Time.deltaTime;

        // Preparar disparo
        if (!isPreparing && shootTimer >= shootInterval - prepareTime)
        {
            isPreparing = true;
            enemy.SetAnimRangedPrepare();
        }

        // Disparar
        if (shootTimer >= shootInterval)
        {
            enemy.SetAnimRangedShoot();
            enemy.ShootProjectile();
            shootTimer = 0f;
            isPreparing = false;
        }
    }

    public override void Exit()
    {
        EnemyManager.Instance?.UnregisterRangedEnemy(enemy);
        isPreparing = false;
    }
}