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
    public GameObject playerPrefab;
    private Vector3 initialPlayerPosition;
    private Quaternion initialPlayerRotation;

    [Header("Configuración de rondas")]
    public float timeBetweenRounds = 3f;

    [Header("Cinemática")]
    public CameraFollow cameraFollow;
    public ObjectiveBanner objectiveBanner;
    public float cinematicDuration = 3f;  // cuánto tiempo muestra la oleada
    public float returnDuration = 1.5f;   // cuánto tarda en volver al jugador

    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;

    private bool isSpawning = false;

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
        Cursor.visible = false;
        initialPlayerPosition = playerPrefab.transform.position;
        initialPlayerRotation = playerPrefab.transform.rotation;

        playerMovement = playerPrefab.GetComponent<PlayerMovement>();
        playerCombat = playerPrefab.GetComponent<PlayerCombat>();

        AudioManager.GetOrCreate().PlayMusic("music1");

        SetPlayerInputEnabled(false); // bloquear antes de la primera cinemática
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

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            // Matar todos los enemigos activos para triggear NextRoundRoutine naturalmente
            foreach (var e in new List<GameObject>(activeEnemies))
            {
                if (e != null)
                {
                    Damageable dmg = e.GetComponent<Damageable>();
                    if (dmg != null) dmg.TakeDamage(9999f, Vector3.zero, "Debug", Damageable.AttackType.Normal);
                }
            }
        }

        if (currentRound > 0 && !waitingForNextRound && !isSpawning && AllEnemiesDead())
            StartCoroutine(NextRoundRoutine());
    }

    private void TriggerVictory()
    {
        PlayerDamageReceiver player = FindAnyObjectByType<PlayerDamageReceiver>();
        if (player != null)
            ScoreManager.Instance?.RegisterHP(player.currentHP, player.maxHP);
        ScoreManager.Instance?.CalculateScore();

        StartCoroutine(VictoryTransition());
    }

    private IEnumerator VictoryTransition()
    {
        // Congelar enemigos
        foreach (EnemyController enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
            enemy.enabled = false;

        SetPlayerInputEnabled(false);

        // Cámara lenta
        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        AudioManager.GetOrCreate().StopMusic();
        AudioManager.GetOrCreate().PlaySFX("victory"); // ← tu sonido de victoria

        yield return new WaitForSecondsRealtime(2.5f);  // duración de la cámara lenta

        // Restaurar tiempo antes de cambiar escena
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        Cursor.lockState = CursorLockMode.None;
        
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
        AudioManager.GetOrCreate().PlaySFX("roundClear");
        yield return new WaitForSeconds(timeBetweenRounds);
        // Teletransportar jugador a posición inicial ANTES de que la cámara llegue
        Debug.Log($"Teletransportando a: {initialPlayerPosition}, posición actual: {playerPrefab.transform.position}");
        playerPrefab.transform.position = initialPlayerPosition;
        playerPrefab.transform.rotation = initialPlayerRotation;

        Rigidbody playerRb = playerPrefab.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.position = initialPlayerPosition; // forzar también por física
        }

        

        currentRound = nextIndex + 1;
        SpawnRound(nextIndex);
        waitingForNextRound = false;
    }

    private void SpawnRound(int index)
    {
        StartCoroutine(SpawnRoundCinematic(index));
    }

    private IEnumerator SpawnRoundCinematic(int index)
    {
        isSpawning = true;
        SetPlayerInputEnabled(false);
        if (index >= waveContainers.Length || waveContainers[index] == null)
        {
            isSpawning = true;
            yield break;
        }
        // 1. Activar enemigos pero congelar su IA
        waveContainers[index].SetActive(true);
        EnemyController[] enemies = waveContainers[index].GetComponentsInChildren<EnemyController>(true);
        int rangedCount = 0;
        activeEnemies.Clear();

        foreach (EnemyController controller in enemies)
        {
            controller.gameObject.SetActive(true);
            controller.enabled = false; // congelar IA
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

        // 2. Calcular centro de la oleada
        Vector3 waveCenter = Vector3.zero;
        foreach (EnemyController e in enemies)
            waveCenter += e.transform.position;
        waveCenter /= enemies.Length;

        // 3. Paneo cinemático hacia la oleada
        cameraFollow?.StartCinematic(waveCenter, cinematicDuration);
        yield return new WaitForSeconds(cinematicDuration);

        // 4. Volver al jugador
        cameraFollow?.EndCinematic();
        yield return new WaitForSeconds(returnDuration);

        // Resetear velocidad del rigidbody para evitar inercia
        Rigidbody playerRb = playerPrefab.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        // 5. Mostrar banner "Ronda X"
        if (objectiveBanner != null)
        {
            objectiveBanner.gameObject.SetActive(true);
            objectiveBanner?.Play($"Ronda {index + 1}");
        }

        SetPlayerInputEnabled(true);
        // 6. Descongelar IA
        foreach (EnemyController controller in enemies)
            controller.enabled = true;

        Debug.Log($"Ronda {index + 1} iniciada — {enemies.Length} enemigos ({rangedCount} ranged)");
        isSpawning = false;
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

    private void SetPlayerInputEnabled(bool enabled)
    {
        if (playerMovement != null) playerMovement.enabled = enabled;
        if (playerCombat != null) playerCombat.enabled = enabled;
    }
}