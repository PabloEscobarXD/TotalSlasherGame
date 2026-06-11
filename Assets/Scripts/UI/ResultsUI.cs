using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class ResultsUI : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Text scoreText;
    public TMP_Text gradeText;
    public GameObject bonusText;
    public TMP_Text multiplierText;
    public GameObject buttonsContainer; // ← objeto que agrupa los dos botones
    public Button firstButton;          // ← botón "Volver a jugar"

    [Header("Configuración")]
    public float countSpeed = 3000f;
    public float bonusRevealDelay = 1f;
    public float gradeRevealDelay = 2f;

    private UIShake uiShake;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (ScoreManager.Instance == null) return;
        gradeText.gameObject.SetActive(false);
        bonusText.SetActive(false);
        buttonsContainer.SetActive(false);
        scoreText.text = "0";
        StartCoroutine(RevealSequence());
    }

    private IEnumerator RevealSequence()
    {
        yield return null; // ← esperar un frame para que todo esté inicializado

        uiShake = FindAnyObjectByType<UIShake>(); // ← mover aquí en vez de Start
        int rawScore = ScoreManager.Instance.GetCurrentScore();
        float hpRatio = ScoreManager.Instance.GetHPRatio();
        float hpMultiplier = ScoreManager.Instance.GetHPMultiplier(hpRatio);
        int finalScore = ScoreManager.Instance.FinalScore;
        string grade = ScoreManager.Instance.FinalGrade;

        // 1. Contar puntaje base
        float displayed = 0f;
        AudioManager.Instance.PlaySFXCancellable("scoreCounting");
        while (displayed < rawScore)
        {
            displayed = Mathf.MoveTowards(displayed, rawScore, countSpeed * Time.deltaTime);
            scoreText.text = Mathf.RoundToInt(displayed).ToString("N0");
            yield return null;
        }
        AudioManager.Instance.StopSFXCancellable();
        scoreText.text = rawScore.ToString("N0");

        yield return new WaitForSeconds(bonusRevealDelay);

        // 2. Revelar bonus y contar hasta puntaje final
        AudioManager.Instance.PlaySFX("bonusReveal");
        bonusText.SetActive(true);
        if (multiplierText != null)
            multiplierText.text = $"{hpMultiplier:F2}";

        displayed = rawScore;
        AudioManager.Instance.PlaySFXCancellable("scoreCounting");
        while (displayed < finalScore)
        {
            displayed = Mathf.MoveTowards(displayed, finalScore, countSpeed * Time.deltaTime);
            scoreText.text = Mathf.RoundToInt(displayed).ToString("N0");
            yield return null;
        }
        AudioManager.Instance.StopSFXCancellable();
        scoreText.text = finalScore.ToString("N0");

        yield return new WaitForSeconds(gradeRevealDelay);

        // 3. Revelar calificación + botones
        AudioManager.Instance.PlaySFX("gradeReveal");
        gradeText.gameObject.SetActive(true);
        gradeText.text = grade;
        buttonsContainer.SetActive(true); // ← revelar botones

        if (uiShake == null)
            uiShake = FindAnyObjectByType<UIShake>();
        yield return null; //
        uiShake?.TriggerShake();

        // Autoseleccionar botón para mando
        yield return null; // esperar un frame para que el botón esté activo
        if (firstButton != null)
        {
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(firstButton.gameObject);
        }
    }
}