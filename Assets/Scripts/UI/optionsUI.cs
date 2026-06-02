using UnityEngine;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;

    void OnEnable()
    {
        InitSliders();
    }

    private void InitSliders()
    {
        if (AudioManager.Instance == null) return;

        // Setear valores
        musicSlider.value = AudioManager.Instance.musicVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;

        // Conectar listeners por código para evitar referencias rotas entre escenas
        musicSlider.onValueChanged.RemoveAllListeners();
        sfxSlider.onValueChanged.RemoveAllListeners();

        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
    }
}