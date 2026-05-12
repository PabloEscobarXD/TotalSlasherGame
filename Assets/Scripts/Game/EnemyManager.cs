using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Configuración de ataque")]
    public int maxAttackers = 3;
    public float minAttackInterval = 0.3f;
    public float maxAttackInterval = 1.0f;
    public float globalCooldown = 2.0f; // espera entre olas de ataques

    private List<EnemyController> meleeEnemies = new List<EnemyController>();
    private List<EnemyController> rangedEnemies = new List<EnemyController>();
    private List<EnemyController> allEnemies = new List<EnemyController>();
    private bool attackWaveInProgress = false;

    [Header("Configuración")]
    public int maxRangedEnemies = 2;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public int GetMeleeCount()
    {
        return meleeEnemies.Count;
    }

    public void RegisterMeleeEnemy(EnemyController enemy)
    {
        if (!meleeEnemies.Contains(enemy))
            meleeEnemies.Add(enemy);
    }

    public void UnregisterMeleeEnemy(EnemyController enemy)
    {
        meleeEnemies.Remove(enemy);
    }

    public void RequestAttackWave()
    {
        if (attackWaveInProgress) return;
        StartCoroutine(AttackWaveCoroutine());
    }

    private IEnumerator AttackWaveCoroutine()
    {
        attackWaveInProgress = true;

        // Excluir enemigos que están bloqueando
        List<EnemyController> candidates = meleeEnemies.FindAll(e =>
            e != null && !e.IsDead() && !e.isBlocking
        );

        if (candidates.Count == 0)
        {
            attackWaveInProgress = false;
            yield break;
        }

        // Mezclar y tomar hasta maxAttackers
        Shuffle(candidates);
        int count = Mathf.Min(maxAttackers, candidates.Count);

        for (int i = 0; i < count; i++)
        {
            EnemyController attacker = candidates[i];

            // Re-validar antes de atacar (pudo moverse/morir durante el delay)
            if (attacker != null && !attacker.IsDead())
                attacker.ExecuteAttack();

            if (i < count - 1)
            {
                float interval = Random.Range(minAttackInterval, maxAttackInterval);
                yield return new WaitForSeconds(interval);
            }
        }

        yield return new WaitForSeconds(globalCooldown);
        attackWaveInProgress = false;
    }

    private void Shuffle(List<EnemyController> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private IEnumerator AssignRangedEnemies()
    {
        // Esperar a que todos los enemigos hagan su Start
        yield return new WaitForSeconds(0.1f);

        Debug.Log($"Total enemigos registrados: {allEnemies.Count}"); // verificar que llegan todos

        List<EnemyController> snapshot = new List<EnemyController>(allEnemies);
        Shuffle(snapshot);
        int count = Mathf.Min(maxRangedEnemies, snapshot.Count);

        for (int i = 0; i < count; i++)
        {
            snapshot[i].prefersRanged = true;
            snapshot[i].fsm.ChangeState(new RangedState(snapshot[i]));
            Debug.Log($"{snapshot[i].gameObject.name} asignado como Ranged");
        }
    }

    public void RegisterEnemy(EnemyController enemy)
    {
        if (!allEnemies.Contains(enemy))
            allEnemies.Add(enemy);
    }

    public void RegisterRangedEnemy(EnemyController enemy)
    {
        if (!rangedEnemies.Contains(enemy))
            rangedEnemies.Add(enemy);
    }

    public void UnregisterRangedEnemy(EnemyController enemy)
    {
        rangedEnemies.Remove(enemy);
    }

    public int GetRangedCount() => rangedEnemies.Count;

    public void SetMaxRanged(int value)
    {
        maxRangedEnemies = value;
    }

    public void ClearAllEnemies()
    {
        meleeEnemies.Clear();
        rangedEnemies.Clear();
        allEnemies.Clear();
        attackWaveInProgress = false;
    }

    public void AssignRanged(int count)
    {
        // Limpiar referencias nulas antes de asignar
        allEnemies.RemoveAll(e => e == null);

        List<EnemyController> snapshot = new List<EnemyController>(allEnemies);
        Shuffle(snapshot);
        int total = Mathf.Min(count, snapshot.Count);

        for (int i = 0; i < total; i++)
        {
            snapshot[i].prefersRanged = true;
            snapshot[i].fsm.ChangeState(new RangedState(snapshot[i]));
            Debug.Log($"{snapshot[i].gameObject.name} asignado como Ranged");
        }
    }
}