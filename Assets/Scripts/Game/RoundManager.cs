using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject enemyPrefab;

    [Header("Configuración de rondas")]
    public float timeBetweenRounds = 3f;

    [System.Serializable]
    public struct RoundConfig
    {
        public int meleeCount;
        public int rangedCount;
    }

    public RoundConfig[] rounds = new RoundConfig[]
    {
        new RoundConfig { meleeCount = 8,  rangedCount = 2 },  // Ronda 1
        new RoundConfig { meleeCount = 11, rangedCount = 3 },  // Ronda 2
        new RoundConfig { meleeCount = 11, rangedCount = 4 },  // Ronda 3
    };

    [Header("Oleadas")]
    public GameObject[] waveContainers;

    private int currentRound = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();

    private bool waitingForNextRound = false;

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
        if (round < 1 || round > rounds.Length) return;
        StopAllCoroutines();
        ClearEnemies();
        currentRound = round;
        SpawnRound(round - 1);
    }

    private IEnumerator NextRoundRoutine()
    {
        waitingForNextRound = true;

        int nextIndex = currentRound; // currentRound es 1-based, así que currentRound == nextIndex en 0-based

        if (nextIndex >= rounds.Length)
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
        if (index >= waveContainers.Length || waveContainers[index] == null) return;

        waveContainers[index].SetActive(true);

        EnemyController[] enemies = waveContainers[index].GetComponentsInChildren<EnemyController>(true);
        int rangedCount = 0;

        activeEnemies.Clear();

        foreach (EnemyController controller in enemies)
        {
            controller.gameObject.SetActive(true);
            activeEnemies.Add(controller.gameObject);

            if (controller.startsAsRanged) rangedCount++;

            Damageable dmg = controller.GetComponent<Damageable>();
            if (dmg != null)
            {
                GameObject go = controller.gameObject;
                dmg.OnDeath += () => activeEnemies.Remove(go);
            }
        }

        EnemyManager.Instance?.SetMaxRanged(rangedCount);
        Debug.Log($"Ronda {index + 1} — {enemies.Length} enemigos ({rangedCount} ranged)");
    }
    private void ClearEnemies()
    {
        foreach (var container in waveContainers)
            if (container != null) container.SetActive(false);
        activeEnemies.Clear();
        EnemyManager.Instance?.ClearAllEnemies();
    }

    private bool AllEnemiesDead()
    {
        activeEnemies.RemoveAll(e => e == null);
        return currentRound > 0 && activeEnemies.Count == 0;
    }

    private int GetCurrentRoundIndex()
    {
        return currentRound - 1;
    }
}