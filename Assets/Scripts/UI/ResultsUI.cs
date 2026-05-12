using UnityEngine;
using TMPro;

public class ResultsUI : MonoBehaviour
{
    public TMP_Text gradeText;
    public TMP_Text scoreText;

    void Start()
    {
        if (ScoreManager.Instance == null) return;

        gradeText.text = ScoreManager.Instance.FinalGrade;
        scoreText.text = ScoreManager.Instance.FinalScore.ToString("N0");
    }
}