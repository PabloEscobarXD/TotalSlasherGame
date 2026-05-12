using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject enemyPrefab;

    [Header("Área de spawn")]
    public Transform spawnAreaCenter;
    public Vector2 spawnAreaSize = new Vector2(20f, 20f); // ancho x largo

    [Header("Configuración de rondas")]
    public float timeBetweenRounds = 3f;

    private int currentRound = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private bool waitingForNextRound = false;


    // Configuración por ronda: (totalEnemigos, maxRanged)
    private (int total, int ranged)[] roundConfig = new (int, int)[]
    {
        (10,  2),  // Ronda 1
        (14,  3),  // Ronda 2
        (15, 4),  // Ronda 3
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        StartRound(1);
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame) StartRound(1);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) StartRound(2);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) StartRound(3);

        // Debug manual para ir a resultados
        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            TriggerVictory();

        if (currentRound > 0 && !waitingForNextRound && AllEnemiesDead())
            StartCoroutine(NextRoundRoutine());
    }

    private void TriggerVictory()
    {
        PlayerDamageReceiver player = FindAnyObjectByType<PlayerDamageReceiver>();
        PlayerCombat combat = FindAnyObjectByType<PlayerCombat>();

        if (player != null)
            ScoreManager.Instance?.RegisterHP(player.currentHP, player.maxHP);

        if (combat != null)
            ScoreManager.Instance?.RegisterCombo(combat.comboCount); // combo activo al terminar

        ScoreManager.Instance?.CalculateScore();
        SceneManager.LoadScene("GameEnded");
    }

    public void StartRound(int round)
    {
        if (round < 1 || round > roundConfig.Length) return;
        StopAllCoroutines();
        ClearEnemies();
        currentRound = round;
        SpawnRound(round - 1);
    }

    private IEnumerator NextRoundRoutine()
    {
        waitingForNextRound = true;

        int nextIndex = currentRound; // currentRound es 1-based, así que currentRound == nextIndex en 0-based

        if (nextIndex >= roundConfig.Length)
        {
            TriggerVictory();
            yield break;
        }

        yield return new WaitForSeconds(timeBetweenRounds);

        currentRound = nextIndex + 1;
        SpawnRound(nextIndex);
        waitingForNextRound = false;
    }

    private void SpawnRound(int index)
    {
        var config = roundConfig[index];
        EnemyManager.Instance?.SetMaxRanged(config.ranged);
        activeEnemies.Clear();

        for (int i = 0; i < config.total; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            activeEnemies.Add(enemy);

            Damageable dmg = enemy.GetComponent<Damageable>();
            if (dmg != null)
                dmg.OnDeath += () => activeEnemies.Remove(enemy);
        }

        // Todos spawneados ? asignar ranged ahora
        StartCoroutine(AssignRangedAfterSpawn(config.ranged));
        Debug.Log($"Ronda {index + 1} iniciada — {config.total} enemigos ({config.ranged} ranged)");
    }

    private IEnumerator AssignRangedAfterSpawn(int rangedCount)
    {
        yield return null; // esperar un frame a que los EnemyController hagan su Start

        EnemyManager.Instance?.AssignRanged(rangedCount);
    }

    private void ClearEnemies()
    {
        foreach (var e in activeEnemies)
            if (e != null) Destroy(e);
        activeEnemies.Clear();
        EnemyManager.Instance?.ClearAllEnemies();
    }

    private bool AllEnemiesDead()
    {
        activeEnemies.RemoveAll(e => e == null);
        return currentRound > 0 && activeEnemies.Count == 0;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
        float z = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
        return spawnAreaCenter.position + new Vector3(x, 0, z);
    }

    private int GetCurrentRoundIndex()
    {
        return currentRound - 1;
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnAreaCenter == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(spawnAreaCenter.position,
            new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y));
    }
}