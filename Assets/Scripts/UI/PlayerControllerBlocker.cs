using UnityEngine;

public class PlayerControllerBlocker : MonoBehaviour
{
    [Header("Scripts que controlan al jugador")]
    public MonoBehaviour[] playerScripts;

    private bool isPaused = false;

    public void SetPaused(bool paused)
    {
        isPaused = paused;

        foreach (var script in playerScripts)
            script.enabled = !paused;
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}
