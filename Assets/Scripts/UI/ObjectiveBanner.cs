using UnityEngine;
using TMPro;
using System.Collections;

public class ObjectiveBanner : MonoBehaviour
{
    [Header("UI Element")]
    public RectTransform banner;          // El texto o el contenedor del texto
    public TextMeshProUGUI message;

    [Header("Movement Settings")]
    public float fastSpeed = 1400f;
    public float slowSpeed = 100f;

    [Header("Timing")]
    public float slowDownDelay = 0.7f;     // Tiempo antes de ir lento
    public float slowDuration = 1.5f;        // Cuánto tiempo permanece lento
    public float fastExitDelay = 0.5f;     // Tiempo antes de salir rápido

    private float canvasWidth;

    void Start()
    {
        // Obtener el ancho del canvas para calcular entrada/salida
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        RectTransform canvasRT = parentCanvas.GetComponent<RectTransform>();
        canvasWidth = canvasRT.rect.width;

        StartCoroutine(PlayBannerAnimation());
    }

    public void SetMessage(string text)
    {
        message.text = text;
    }

    private IEnumerator PlayBannerAnimation()
    {
        yield return new WaitForSeconds(fastExitDelay);
        // ——— 1. POSICIÓN INICIAL (Fuera de la izquierda)
        Vector2 startPos = new Vector2(-canvasWidth - 300f, banner.anchoredPosition.y);
        Vector2 endCenter = new Vector2(0, banner.anchoredPosition.y);
        Vector2 exitPos = new Vector2(canvasWidth + 300f, banner.anchoredPosition.y);

        banner.anchoredPosition = startPos;

        // ——— 2. ENTRADA RÁPIDA
        float timer = 0f;
        while (timer < slowDownDelay)
        {
            banner.anchoredPosition = Vector2.Lerp(startPos, endCenter, timer / slowDownDelay);
            timer += Time.deltaTime * (fastSpeed / 500f);
            yield return null;
        }

        // Asegurar que quedó cerca del centro
        banner.anchoredPosition = endCenter;

        // ——— 3. MOVIMIENTO LENTO (lectura)
        timer = 0f;
        while (timer < slowDuration)
        {
            banner.anchoredPosition += Vector2.right * slowSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        // ——— 5. SALIDA RÁPIDA
        while (banner.anchoredPosition.x < exitPos.x)
        {
            banner.anchoredPosition += Vector2.right * fastSpeed * Time.deltaTime;
            yield return null;
        }

        // Al terminar puedes destruirlo o desactivarlo
        gameObject.SetActive(false);
    }
}
