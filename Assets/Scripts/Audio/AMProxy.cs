using UnityEngine;

public class AudioManagerProxy : MonoBehaviour
{
    public void SetMusicVolume(float v) => AudioManager.GetOrCreate().SetMusicVolume(v);
    public void SetSFXVolume(float v)   => AudioManager.GetOrCreate().SetSFXVolume(v);
    public void PlaySFX(string id)
    {
        AudioManager.GetOrCreate().PlaySFX(id);
    }

    public void PlayMusic(string id)
    {
        AudioManager.GetOrCreate().PlayMusic(id);
    }

    public void StopMusic()
    {
        AudioManager.GetOrCreate().StopMusic();
    }
}