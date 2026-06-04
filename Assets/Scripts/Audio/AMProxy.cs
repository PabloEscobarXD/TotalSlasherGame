using UnityEngine;

public class AudioManagerProxy : MonoBehaviour
{
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