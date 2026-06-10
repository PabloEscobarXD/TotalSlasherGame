using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    private UIShake uiShake;
    private PlayerHealthUI playerHealthUI;
    private FuryUI furyUI;
    private FilmGrainController filmGrain;
    private ScoreManager scoreManager;

    void Awake()
    {
        uiShake = GetComponent<UIShake>();
        playerHealthUI = GetComponent<PlayerHealthUI>();
        furyUI = GetComponent<FuryUI>();
        filmGrain = GetComponent<FilmGrainController>();
        scoreManager = GetComponent<ScoreManager>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        uiShake?.OnSceneLoaded(scene, mode);
        playerHealthUI?.OnSceneLoaded(scene, mode);
        furyUI?.OnSceneLoaded(scene, mode);
        filmGrain?.OnSceneLoaded(scene, mode);
        scoreManager?.OnSceneLoaded(scene, mode);
    }
}