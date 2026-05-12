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

        float bannerWidth = banner.rect.width;
        float centerX = -bannerWidth / 2f + 500f;
        float exitX = canvasWidth / 2f + bannerWidth;

        Vector2 startPos = new Vector2(-canvasWidth / 2f - bannerWidth, banner.anchoredPosition.y);
        banner.anchoredPosition = startPos;

        // ——— ENTRADA RÁPIDA hasta llegar al centro
        while (banner.anchoredPosition.x < centerX)
        {
            banner.anchoredPosition += Vector2.right * fastSpeed * Time.deltaTime;
            yield return null;
        }
        banner.anchoredPosition = new Vector2(centerX, banner.anchoredPosition.y);

        // ——— MOVIMIENTO LENTO por duración fija (esto sí tiene sentido que sea tiempo)
        float timer = 0f;
        while (timer < slowDuration)
        {
            banner.anchoredPosition += Vector2.right * slowSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        // ——— SALIDA RÁPIDA hasta salir de pantalla
        while (banner.anchoredPosition.x < exitX)
        {
            banner.anchoredPosition += Vector2.right * fastSpeed * Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
