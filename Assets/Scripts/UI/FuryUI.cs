using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FuryUI : MonoBehaviour
{
    public FurySystem fury;
    public Image furyBarFill;

    [Header("Vignette")]
    public Volume globalVolume;
    public Color furyReadyColor = new Color(1f, 0.6f, 0.1f); // naranja claro
    public Color defaultColor = Color.black;

    private Vignette vignette;

    void Start()
    {
        if (globalVolume != null)
            globalVolume.profile.TryGet(out vignette);
    }

    void Update()
    {
        if (fury != null)
            furyBarFill.fillAmount = fury.fury;

        if (vignette != null)
            vignette.color.value = fury.fury >= 0.7f ? furyReadyColor : defaultColor;
    }

    public void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        fury = FindAnyObjectByType<FurySystem>();
        furyBarFill = GameObject.FindGameObjectWithTag("FuryFill")?.GetComponent<Image>();
        globalVolume = FindAnyObjectByType<Volume>();

        if (globalVolume != null)
            globalVolume.profile.TryGet(out vignette);
    }
}