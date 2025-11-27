public abstract class EnemyState
{
    protected EnemyController enemy;

    public EnemyState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    // Se ejecuta una vez cuando entro al estado
    public virtual void Enter() { }

    // Se ejecuta cada frame
    public virtual void Update() { }

    // Se ejecuta cuando salgo del estado
    public virtual void Exit() { }
}
