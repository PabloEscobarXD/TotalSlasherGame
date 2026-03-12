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
    private bool attackWaveInProgress = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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

        // Filtrar candidatos válidos
        List<EnemyController> candidates = meleeEnemies.FindAll(e =>
            e != null && !e.IsDead()
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
}