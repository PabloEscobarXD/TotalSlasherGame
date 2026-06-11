using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource sfxCancellableSource;
    [Header("Sound Banks")]
    public SoundBank[] banks;

    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private Dictionary<string, SoundBank.SoundEntry> cache;
    private Dictionary<string, float> sfxCooldowns = new Dictionary<string, float>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        if (sfxCancellableSource == null)
            sfxCancellableSource = gameObject.AddComponent<AudioSource>();

        BuildCache();

        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    void Start()
    {
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    private void BuildCache()
    {
        cache = new Dictionary<string, SoundBank.SoundEntry>();
        foreach (var bank in banks)
        {
            if (bank == null) continue;
            foreach (var entry in bank.sounds)
            {
                if (!cache.ContainsKey(entry.id))
                    cache[entry.id] = entry;
                else
                    Debug.LogWarning($"AudioManager: id duplicado '{entry.id}'");
            }
        }
    }

    // -------------------- Música --------------------

    public void PlayMusic(string id)
    {
        if (!cache.TryGetValue(id, out var entry)) return;

        // ← CAMBIO: también reproducir si está detenida aunque sea el mismo clip
        if (musicSource.clip == entry.clip && musicSource.isPlaying) return;

        musicSource.clip = entry.clip;
        musicSource.volume = musicVolume * entry.volume;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();
    public void PauseMusic() => musicSource.Pause();
    public void ResumeMusic() => musicSource.UnPause();

    // -------------------- SFX --------------------

    public void PlaySFX(string id)
    {
        if (!cache.TryGetValue(id, out var entry)) return;
        if (!CanPlaySFX(id, entry.minInterval)) return;
        sfxSource.pitch = entry.pitch;
        sfxSource.PlayOneShot(entry.clip, sfxVolume * entry.volume);
    }

    // -------------------- Volumen --------------------

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        // Respetar el volumen individual del clip actual
        if (musicSource.clip != null && cache != null)
        {
            foreach (var entry in cache.Values)
            {
                if (entry.clip == musicSource.clip)
                {
                    musicSource.volume = musicVolume * entry.volume;
                    break;
                }
            }
        }
        else
        {
            musicSource.volume = value;
        }

        if (sfxCancellableSource.isPlaying && sfxCancellableSource.loop)
            sfxCancellableSource.volume = musicVolume;

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private bool CanPlaySFX(string id, float minInterval)
    {
        float now = Time.unscaledTime;
        if (sfxCooldowns.TryGetValue(id, out float lastTime))
            if (now - lastTime < minInterval) return false;
        sfxCooldowns[id] = now;
        return true;
    }
    public void PlaySFX3D(string id, Vector3 position)
    {
        if (!cache.TryGetValue(id, out var entry)) return;
        if (!CanPlaySFX(id, entry.minInterval)) return;

        GameObject tempGO = new GameObject($"SFX3D_{id}");
        tempGO.transform.position = position;
        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = entry.clip;
        source.volume = sfxVolume * entry.volume;
        source.pitch = entry.pitch;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.maxDistance = 30f;
        source.Play();
        Destroy(tempGO, entry.clip.length / entry.pitch);
    }

    // Reproducir un sonido cancelable (usa Play en vez de PlayOneShot)
    public void PlaySFXCancellable(string id)
    {
        if (!cache.TryGetValue(id, out var entry)) return;
        sfxCancellableSource.clip = entry.clip;
        sfxCancellableSource.pitch = entry.pitch;
        sfxCancellableSource.volume = sfxVolume * entry.volume;
        sfxCancellableSource.Play();
    }

    // Cancelar el sonido cancelable actual
    public void StopSFXCancellable()
    {
        sfxCancellableSource.Stop();
    }

    // -------------------- Lazy creation --------------------

    public static AudioManager GetOrCreate()
    {
        if (Instance != null) return Instance;
        return new GameObject("AudioManager").AddComponent<AudioManager>();
    }

    public void PlayMusicOverlay(string id)
    {
        if (!cache.TryGetValue(id, out var entry)) return;
        sfxCancellableSource.pitch = 1f; // ← resetear pitch antes de reproducir
        sfxCancellableSource.clip = entry.clip;
        sfxCancellableSource.loop = true;
        sfxCancellableSource.volume = musicVolume * entry.volume;
        sfxCancellableSource.Play();
    }

    public void StopMusicOverlay()
    {
        sfxCancellableSource.loop = false;
        sfxCancellableSource.Stop();
    }
}