using UnityEngine;

public class FurySystem : MonoBehaviour
{
    [Header("Furia")]
    [Range(0f, 1f)]
    public float fury = 0f;
    public float furyGainPerHit = 0.15f;
    public float furyDecayRate = 0.25f;
    public float timeBeforeDecay = 2f;

private float timeSinceLastHit = 0f;

    void Update()
    {
        HandleDecay();
    }

    public void AddFury()
    {
        fury = Mathf.Clamp01(fury + furyGainPerHit);
        timeSinceLastHit = 0f;

        Debug.Log($"[FURY] Furia actual: {fury}");
    }

    void HandleDecay()
    {
        timeSinceLastHit += Time.deltaTime;

        if (timeSinceLastHit >= timeBeforeDecay && fury > 0f)
        {
            fury -= furyDecayRate * Time.deltaTime;
            fury = Mathf.Clamp01(fury);

            Debug.Log($"[FURY] Decay: {fury}");
        }
    }

}
