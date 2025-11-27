using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Damageable))]
public class EnemyController : MonoBehaviour
{
    [Header("Percepción")]
    public float meleeRange = 20f;
    public float rangedRange = 10f;
    public float stopDistance = 5f;   // Mantener distancia mínima

[Header("Movimiento")]
    public float moveSpeed = 17f;

    [HideInInspector] public StateMachine fsm;
    private Transform player;
    private Rigidbody rb;
    private Damageable health;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Damageable>();

        // Evento de muerte → cambiar estado
        health.OnDeath += () =>
        {
            if (fsm != null)
                fsm.ChangeState(new DeadState(this));
        };

        // Buscar un player en la escena
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Crear FSM
        fsm = new StateMachine();
        fsm.ChangeState(new EvaluateState(this));
    }

    void Update()
    {
        fsm.Update();
    }

    // ================================
    //           MÉTODOS DE IA
    // ================================

    public bool IsDead()
    {
        return health != null && health.IsDead();
    }

    public bool IsPlayerClose()
    {
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= meleeRange;
    }

    public bool IsPlayerInRanged()
    {
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= rangedRange;
    }

    public void MoveTowardsPlayer()
    {
        if (!player) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Si está dentro de la distancia mínima, no avanzar más
        if (distance <= stopDistance)
            return;

        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        rb.MovePosition(transform.position + dir * moveSpeed * Time.deltaTime);

        // Girar hacia el jugador
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            0.1f
        );
    }

    public Transform GetPlayer()
    {
        return player;
    }

}
