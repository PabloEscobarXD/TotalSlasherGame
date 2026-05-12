using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FilmGrainController : MonoBehaviour
{
    [Header("Referencias")]
    public Volume globalVolume;
    public PlayerDamageReceiver player;

    [Header("Configuración")]
    public float responseOscillationSpeed = 3f;

    private FilmGrain filmGrain;

    void Start()
    {
        if (globalVolume != null)
            globalVolume.profile.TryGet(out filmGrain);
    }

    void Update()
    {
        if (filmGrain == null || player == null) return;

        float healthRatio = player.currentHP / player.maxHP;

        // Intensity inversamente proporcional a la vida
        float intensity = 1f - healthRatio;
        filmGrain.intensity.Override(intensity);

        // Response oscila solo si no está al máximo de vida
        if (healthRatio < 1f)
        {
            float response = Mathf.PingPong(Time.time * responseOscillationSpeed, 0.5f);
            filmGrain.response.Override(response);
        }
        else
        {
            filmGrain.response.Override(0f);
        }
    }
}