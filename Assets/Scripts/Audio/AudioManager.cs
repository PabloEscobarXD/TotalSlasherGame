using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Sound Banks")]
    public SoundBank[] banks;

    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Referencias")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private Dictionary<string, SoundBank.SoundEntry> cache;

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
        if (musicSource.clip == entry.clip) return;
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
        sfxSource.pitch = entry.pitch;
        sfxSource.PlayOneShot(entry.clip, sfxVolume * entry.volume);
    }

    // -------------------- Volumen --------------------

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        musicSource.volume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    // -------------------- Lazy creation --------------------

    public static AudioManager GetOrCreate()
    {
        if (Instance != null) return Instance;
        return new GameObject("AudioManager").AddComponent<AudioManager>();
    }
}