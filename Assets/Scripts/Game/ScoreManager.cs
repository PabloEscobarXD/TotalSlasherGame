using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private List<int> combos = new List<int>(); // cada combo registrado
    private float finalHP;
    private float maxHP;
    public TMP_Text scoreText;

    public int FinalScore { get; private set; }
    public string FinalGrade { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (scoreText != null)
            scoreText.text = GetCurrentScore().ToString("N0");
    }

    public int GetCurrentScore()
    {
        float comboScore = 0f;
        foreach (int combo in combos)
            comboScore += Mathf.Pow(combo, 1.5f);
        return Mathf.RoundToInt(comboScore);
    }

    public void RegisterCombo(int hits)
    {
        if (hits >= 2) // solo registrar combos reales
            combos.Add(hits);
    }

    public void RegisterHP(float current, float max)
    {
        finalHP = current;
        maxHP = max;
    }

    public void CalculateScore()
    {
        // Puntaje base: suma ponderada exponencial
        float comboScore = 0f;
        foreach (int combo in combos)
            comboScore += Mathf.Pow(combo, 1.5f);

        // Multiplicador de vida
        float hpRatio = maxHP > 0 ? finalHP / maxHP : 0f;
        float hpMultiplier = GetHPMultiplier(hpRatio);

        FinalScore = Mathf.RoundToInt(comboScore * hpMultiplier);
        FinalGrade = GetGrade(FinalScore);
    }

    private float GetHPMultiplier(float ratio)
    {
        if (ratio >= 1.00f) return 1.40f;
        if (ratio >= 0.75f) return 1.25f;
        if (ratio >= 0.50f) return 1.10f;
        if (ratio >= 0.25f) return 1.00f;
        return 0.90f;
    }

    private string GetGrade(int score)
    {
        if (score >= 2000) return "S";
        if (score >= 1600) return "A+";
        if (score >= 1300) return "A";
        if (score >= 1100) return "A-";
        if (score >= 900) return "B+";
        if (score >= 750) return "B";
        if (score >= 600) return "B-";
        if (score >= 480) return "C+";
        if (score >= 370) return "C";
        if (score >= 270) return "C-";
        if (score >= 190) return "D+";
        if (score >= 120) return "D";
        return "D-";
    }

    public void ResetScore()
    {
        combos.Clear();
        finalHP = 0;
        maxHP = 0;
        FinalScore = 0;
        FinalGrade = "";
    }
}