using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private float finalHP;
    private float maxHP;
    public TMP_Text scoreText;
    public int FinalScore { get; private set; }
    public string FinalGrade { get; private set; }

    [Header("Combo")]
    public float comboTimeLimit = 5f;
    private float comboMultiplier = 1f;
    private float comboTimer = 0f;
    private bool comboActive = false;
    private int currentComboKills = 0;

    [Header("Puntaje")]
    public float scorePerKill = 100f;
    private float totalScore = 0f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (comboActive)
        {
            comboTimer += Time.deltaTime;
            if (comboTimer >= comboTimeLimit)
                ResetCombo();
        }

        if (scoreText != null)
            scoreText.text = Mathf.RoundToInt(totalScore).ToString("N0");
    }

    // Llamar cuando muere un enemigo
    public void RegisterKill()
    {
        if (!comboActive)
            comboActive = true;

        comboTimer = 0f; // resetear timer del combo
        currentComboKills++;

        // Aplicar puntaje con multiplicador actual
        totalScore += scorePerKill * comboMultiplier;

        // Incrementar multiplicador para el próximo kill
        comboMultiplier += 0.1f;
    }

    public float GetCurrentMultiplier() => comboMultiplier;
    public int GetCurrentComboKills() => currentComboKills;

    private void ResetCombo()
    {
        comboMultiplier = 1f;
        comboTimer = 0f;
        comboActive = false;
        currentComboKills = 0;
    }

    public int GetCurrentScore() => Mathf.RoundToInt(totalScore);

    public void RegisterHP(float current, float max)
    {
        finalHP = current;
        maxHP = max;
    }

    public void CalculateScore()
    {
        float hpRatio = maxHP > 0 ? finalHP / maxHP : 0f;
        float hpMultiplier = GetHPMultiplier(hpRatio);
        FinalScore = Mathf.RoundToInt(totalScore * hpMultiplier);
        FinalGrade = GetGrade(FinalScore);
    }

    public float GetHPMultiplier(float ratio)
    {
        if (ratio >= 0.75f) return 1.4f;
        if (ratio >= 0.50f) return 1.2f;
        if (ratio >= 0.35f) return 1.0f;
        if (ratio >= 0.25f) return 0.8f;
        return 0.90f;
    }

    private string GetGrade(int score)
    {
        if (score >= 7400) return "S+";
        if (score >= 7000) return "S";
        if (score >= 6700) return "A+";
        if (score >= 6400) return "A";
        if (score >= 6100) return "A-";
        if (score >= 5800) return "B+";
        if (score >= 5500) return "B";
        if (score >= 5200) return "B-";
        if (score >= 4900) return "C+";
        if (score >= 4600) return "C";
        if (score >= 4300) return "C-";
        if (score >= 4000) return "D+";
        if (score >= 3900) return "D";
        return "D-";
    }

    public void ResetScore()
    {
        totalScore = 0f;
        finalHP = 0;
        maxHP = 0;
        FinalScore = 0;
        FinalGrade = "";
        ResetCombo();
    }

    public void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Resetear score al entrar a la escena de juego
        if (scene.name == "Nivel1")
            ResetScore();

        GameObject scoreObj = GameObject.FindGameObjectWithTag("ScoreText");
        if (scoreObj != null)
            scoreText = scoreObj.GetComponent<TMP_Text>();
    }

    public float GetHPRatio() => maxHP > 0 ? finalHP / maxHP : 0f;
}