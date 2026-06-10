using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIShake : MonoBehaviour
{
    [Header("Configuración")]
    public List<RectTransform> elementsToShake = new List<RectTransform>();
    public float speed = 40f;
    public float amount = 8f;
    public float duration = 0.4f;

    [Header("Post Procesado")]
    public Volume globalVolume;
    public Color vignetteHitColor = new Color(1f, 0.4f, 0f);

    private List<Vector2> originalPositions = new List<Vector2>();
    private Coroutine shakeCoroutine;
    private Vignette vignette;
    private Color originalVignetteColor;

    void OnEnable()
    {
        // Buscar elementos por tag si las referencias del Inspector se perdieron
        if (elementsToShake.Count == 0 || elementsToShake[0] == null)
        {
            elementsToShake.Clear();
            // Buscar por tag — asigná el tag "ShakeElement" a los objetos en la escena
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("ShakeElement"))
            {
                RectTransform rt = go.GetComponent<RectTransform>();
                if (rt != null) elementsToShake.Add(rt);
            }
        }

        originalPositions.Clear();
        foreach (RectTransform rt in elementsToShake)
            if (rt != null) originalPositions.Add(rt.anchoredPosition);

        if (globalVolume != null)
            globalVolume.profile.TryGet(out vignette);
        if (vignette != null)
            originalVignetteColor = vignette.color.value;
    }
    void Start()
    {
        foreach (RectTransform rt in elementsToShake)
            originalPositions.Add(rt.anchoredPosition);

        if (globalVolume != null)
            globalVolume.profile.TryGet(out vignette);

        if (vignette != null)
            originalVignetteColor = vignette.color.value;
    }

    public void TriggerShake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        if (vignette != null)
            vignette.color.Override(vignetteHitColor);

        while (elapsed < duration)
        {
            float offsetX = Mathf.Sin(elapsed * speed) * amount;
            float offsetY = Mathf.Cos(elapsed * speed * 0.7f) * amount * 0.5f;

            for (int i = 0; i < elementsToShake.Count; i++)
            {
                if (elementsToShake[i] != null)
                    elementsToShake[i].anchoredPosition = originalPositions[i] + new Vector2(offsetX, offsetY);
            }

            float t = elapsed / duration;
            if (vignette != null)
                vignette.color.Override(Color.Lerp(vignetteHitColor, originalVignetteColor, t));

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        for (int i = 0; i < elementsToShake.Count; i++)
        {
            if (elementsToShake[i] != null)
                elementsToShake[i].anchoredPosition = originalPositions[i];
        }

        if (vignette != null)
            vignette.color.Override(originalVignetteColor);

        shakeCoroutine = null;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        elementsToShake.Clear();
        originalPositions.Clear();

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("ShakeElement"))
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                elementsToShake.Add(rt);
                originalPositions.Add(rt.anchoredPosition);
            }
        }

        globalVolume = FindAnyObjectByType<Volume>();
        if (globalVolume != null)
            globalVolume.profile.TryGet(out vignette);
        if (vignette != null)
            originalVignetteColor = vignette.color.value;
    }
}