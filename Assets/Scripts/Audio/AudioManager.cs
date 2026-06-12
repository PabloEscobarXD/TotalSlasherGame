using UnityEngine;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volumen")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // -------------------- Instancias persistentes --------------------
    private EventInstance musicInstance;      // event:/Music
    private EventInstance pauseSnapshot;      // snapshot:/Pause
    private EventInstance sfxCancellable;     // para area_charge (loop)

    private bool musicStarted = false;
    private bool pauseActive = false;

    // -------------------- Cooldowns SFX --------------------
    private Dictionary<string, float> sfxCooldowns = new Dictionary<string, float>();

    // -------------------- Paths --------------------
    // Música
    private const string MUSIC_PATH = "event:/Music";
    private const string PAUSE_SNAPSHOT = "snapshot:/Pause";

    // PlayerSFX
    private const string SFX_AREA_CHARGE = "event:/PlayerSFX/area_charge";
    private const string SFX_AREA_RELEASE = "event:/PlayerSFX/area_release";
    private const string SFX_FURY_AREA = "event:/PlayerSFX/furyAreaAttack";
    private const string SFX_FURY_SINGLE = "event:/PlayerSFX/furySingleAttack";
    private const string SFX_PLAYER_HIT = "event:/PlayerSFX/hit";
    private const string SFX_MAX_RAGE = "event:/PlayerSFX/maxRage";
    private const string SFX_ATTACK_SINGLE = "event:/PlayerSFX/player_attack_single";
    private const string SFX_BLOCK_SUCCESS = "event:/PlayerSFX/player_blockSuccess";

    // EnemySFX
    private const string SFX_ENEMY_ATTACK = "event:/EnemySFX/enemy_attack";
    private const string SFX_ENEMY_RANGED_ATTACK = "event:/EnemySFX/enemy_rangedAttack";
    private const string SFX_ENEMY_BLOCK = "event:/EnemySFX/enemy_block";
    private const string SFX_ENEMY_HIT = "event:/EnemySFX/enemy_hit";
    private const string SFX_ENEMY_DEATH = "event:/EnemySFX/enemyDeath";

    // UISFX
    private const string SFX_BONUS_REVEAL = "event:/UISFX/bonusReveal";
    private const string SFX_ROUND_CLEAR = "event:/UISFX/roundClear";
    private const string SFX_SCORE_COUNTING = "event:/UISFX/scoreCounting";
    private const string SFX_WIN = "event:/UISFX/win";
    private const string SFX_BUTTON_CLICK = "event:/UISFX/button_click";
    private const string SFX_BUTTON_SWITCH = "event:/UISFX/button_switch";
    private const string SFX_GRADE_REVEAL = "event:/UISFX/gradeReveal";

    // Mapa de ids legacy → paths FMOD (compatibilidad con scripts existentes)
    private Dictionary<string, string> sfxMap;

    // -------------------- Lifecycle --------------------

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        BuildSFXMap();
    }

    private void BuildSFXMap()
    {
        sfxMap = new Dictionary<string, string>
        {
            // PlayerSFX
            { "area_charge",          SFX_AREA_CHARGE },
            { "area_release",         SFX_AREA_RELEASE },
            { "furyAreaAttack",       SFX_FURY_AREA },
            { "furySingleAttack",     SFX_FURY_SINGLE },
            { "player_hit",           SFX_PLAYER_HIT },
            { "maxRage",              SFX_MAX_RAGE },
            { "player_attack_single", SFX_ATTACK_SINGLE },
            { "block_success",        SFX_BLOCK_SUCCESS },

            // EnemySFX
            { "enemy_attack",         SFX_ENEMY_ATTACK },
            { "enemy_rangedAttack",   SFX_ENEMY_RANGED_ATTACK},
            { "enemy_block",          SFX_ENEMY_BLOCK },
            { "enemy_hit",            SFX_ENEMY_HIT },
            { "enemyDeath",           SFX_ENEMY_DEATH },

            // UISFX
            { "bonusReveal",          SFX_BONUS_REVEAL },
            { "roundClear",           SFX_ROUND_CLEAR },
            { "scoreCounting",        SFX_SCORE_COUNTING },
            { "victory",              SFX_WIN },        // legacy → win
            { "win",                  SFX_WIN },
            { "button_click",         SFX_BUTTON_CLICK },
            { "button_switch",        SFX_BUTTON_SWITCH },
            { "gradeReveal",          SFX_GRADE_REVEAL },
        };
    }

    void OnDestroy()
    {
        // Liberar instancias al destruir
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
        if (pauseSnapshot.isValid())
        {
            pauseSnapshot.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            pauseSnapshot.release();
        }
        if (sfxCancellable.isValid())
        {
            sfxCancellable.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            sfxCancellable.release();
        }
    }

    // -------------------- Música --------------------

    public void PlayMusic(string id = "music")
    {
        // Solo tenemos un evento de música, ignoramos el id legacy
        if (!musicStarted)
        {
            musicInstance = RuntimeManager.CreateInstance(MUSIC_PATH);
            musicInstance.setVolume(musicVolume);
            musicInstance.start();
            musicStarted = true;
        }
        else
        {
            // Si estaba detenida, reanudar
            PLAYBACK_STATE state;
            musicInstance.getPlaybackState(out state);
            if (state == PLAYBACK_STATE.STOPPED)
            {
                musicInstance.start();
            }
        }
    }

    public void StopMusic()
    {
        if (musicInstance.isValid())
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void PauseMusic()
    {
        if (musicInstance.isValid())
            musicInstance.setPaused(true);
    }

    public void ResumeMusic()
    {
        if (musicInstance.isValid())
            musicInstance.setPaused(false);
    }

    // -------------------- Parámetros de música --------------------

    /// <summary>
    /// Cambia el estado del juego en la música dinámica.
    /// Estados: "Menu", "Round1", "Round2", "Round3", "GameEnd"
    /// </summary>
    public void SetGameState(string stateName)
    {
        if (!musicInstance.isValid()) return;
        musicInstance.setParameterByNameWithLabel("GameState", stateName);
    }

    /// <summary>
    /// Actualiza el nivel de furia (0 a 1) para la música dinámica.
    /// </summary>
    public void SetFuryLevel(float value)
    {
        if (!musicInstance.isValid()) return;
        musicInstance.setParameterByName("FuryLevel", Mathf.Clamp01(value));
    }

    /// <summary>
    /// Actualiza la pérdida de HP (0 = full HP, 1 = muerto) para el lowpass.
    /// </summary>
    public void SetPlayerHPLoss(float value)
    {
        if (!musicInstance.isValid()) return;
        musicInstance.setParameterByName("PlayerHPLoss", Mathf.Clamp01(value));
    }

    // -------------------- Pausa (Snapshot) --------------------

    public void PauseMusicSnapshot()
    {
        if (pauseActive) return;
        pauseSnapshot = RuntimeManager.CreateInstance(PAUSE_SNAPSHOT);
        pauseSnapshot.start();
        pauseActive = true;
        PauseMusic();
    }

    public void ResumeMusicSnapshot()
    {
        if (!pauseActive) return;
        if (pauseSnapshot.isValid())
        {
            pauseSnapshot.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            pauseSnapshot.release();
        }
        pauseActive = false;
        ResumeMusic();
    }

    // Métodos legacy para compatibilidad con PauseManager
    public void PlayMusicOverlay(string id) => PauseMusicSnapshot();
    public void StopMusicOverlay() => ResumeMusicSnapshot();

    // -------------------- SFX --------------------

    public void PlaySFX(string id)
    {
        if (!sfxMap.TryGetValue(id, out string path))
        {
            Debug.LogWarning($"AudioManager: SFX '{id}' no encontrado en el mapa.");
            return;
        }
        if (!CanPlaySFX(id)) return;
        RuntimeManager.PlayOneShot(path);
    }

    public void PlaySFX3D(string id, Vector3 position, int instanceID = 0)
    {
        if (!sfxMap.TryGetValue(id, out string path))
        {
            Debug.LogWarning($"AudioManager: SFX3D '{id}' no encontrado.");
            return;
        }
        string cooldownKey = $"{id}_3d_{instanceID}";
        if (!CanPlaySFX(cooldownKey))
        {
            Debug.Log($"[SFX3D] Bloqueado por cooldown: {cooldownKey}");
            return;
        }
        Debug.Log($"[SFX3D] Reproduciendo: {path} en {position}");
        RuntimeManager.PlayOneShot(path, position);
    }

    // -------------------- SFX Cancellable (area_charge loop) --------------------

    public void PlaySFXCancellable(string id)
    {
        StopSFXCancellable();

        if (!sfxMap.TryGetValue(id, out string path)) return;

        sfxCancellable = RuntimeManager.CreateInstance(path);
        sfxCancellable.setVolume(sfxVolume);
        sfxCancellable.start();
    }

    public void StopSFXCancellable()
    {
        if (sfxCancellable.isValid())
        {
            sfxCancellable.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            sfxCancellable.release();
        }
    }

    // -------------------- Volumen --------------------

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (musicInstance.isValid())
            musicInstance.setVolume(musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    // -------------------- Cooldown SFX --------------------

    private bool CanPlaySFX(string id, float minInterval = 0.05f)
    {
        float now = Time.unscaledTime;
        if (sfxCooldowns.TryGetValue(id, out float lastTime))
            if (now - lastTime < minInterval) return false;
        sfxCooldowns[id] = now;
        return true;
    }

    // -------------------- Lazy creation --------------------

    public static AudioManager GetOrCreate()
    {
        if (Instance != null) return Instance;
        return new GameObject("AudioManager").AddComponent<AudioManager>();
    }
}