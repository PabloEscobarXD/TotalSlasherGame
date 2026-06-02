using UnityEngine;

[CreateAssetMenu(fileName = "SoundBank", menuName = "Audio/Sound Bank")]
public class SoundBank : ScriptableObject
{
    [System.Serializable]
    public class SoundEntry
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitch = 1f;
        [Min(0f)] public float minInterval = 0.1f;
    }

    public SoundEntry[] sounds;

    public SoundEntry Get(string id)
    {
        foreach (var s in sounds)
            if (s.id == id) return s;
        Debug.LogWarning($"SoundBank: id '{id}' no encontrado.");
        return null;
    }
}