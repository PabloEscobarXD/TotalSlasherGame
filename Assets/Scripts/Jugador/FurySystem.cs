using UnityEngine;
using System.Collections;

public class FurySystem : MonoBehaviour
{
    [Header("Furia")]
    [Range(0f, 1f)]
    public float fury = 0f;                   // Nivel actual
    public float furyGainPerHit = 0.15f;      // Carga por golpe
    public float furyDecayRate = 0.25f;       // Decaimiento por segundo
    public float timeBeforeDecay = 2f;        // Tiempo sin hits antes de decaer

    [Header("Modo Furia")]
    public float furyDamageMultiplier = 2.5f; // Multiplicador de daño
    public float slowmoTime = 2f;             // Duración de slowmo
    public float slowmoScale = 0.4f;          // Escala de tiempo en slowmo
    public bool furyModeActive = false;       // Activado mediante input

    private float timeSinceLastHit = 0f;
    private bool slowmoActive = false;
    private Coroutine slowmoRoutine;

    private bool maxRageSoundPlayed = false;

    void Update()
    {
        HandleDecay();

        if (fury >= 0.7f && !maxRageSoundPlayed)
        {
            AudioManager.Instance.PlaySFX("maxRage");
            maxRageSoundPlayed = true;
        }
        else if (fury < 0.7f)
        {
            maxRageSoundPlayed = false; // reset para que suene de nuevo la próxima vez
        }
    }

    // -------------------- Furia Base --------------------
    public void AddFury()
    {
        fury = Mathf.Clamp01(fury + furyGainPerHit);
        timeSinceLastHit = 0f;
    }

    private void HandleDecay()
    {
        timeSinceLastHit += Time.deltaTime;

        if (timeSinceLastHit >= timeBeforeDecay && fury > 0f)
        {
            fury -= furyDecayRate * Time.deltaTime;
            fury = Mathf.Clamp01(fury);
        }
    }

    // -------------------- Estado de Furia --------------------
    public bool IsFuryReady()
    {
        return furyModeActive && fury >= 0.7f;
    }

    public float GetDamageMultiplier()
    {
        return IsFuryReady() ? furyDamageMultiplier : 1f;
    }
    public bool CanFuryDash(float cost)
    {
        return fury >= cost;
    }

    public void ConsumeFury()
    {
        fury = 0f;
    }

    public void SetFuryMode(bool active)
    {
        furyModeActive = active;
    }

    // -------------------- Slow Motion --------------------
    public void TriggerSlowmo()
    {
        if (!IsFuryReady())
            return;

        if (slowmoActive && slowmoRoutine != null)
            StopCoroutine(slowmoRoutine);

        slowmoRoutine = StartCoroutine(SlowmoRoutine());
    }

    private IEnumerator SlowmoRoutine()
    {
        slowmoActive = true;

        Time.timeScale = slowmoScale;
        Time.fixedDeltaTime = 0.02f * slowmoScale;

        yield return new WaitForSecondsRealtime(slowmoTime);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        slowmoActive = false;
    }

    public void ConsumeFuryPartial(float amount)
    {
        fury = Mathf.Clamp01(fury - amount);
    }
}
