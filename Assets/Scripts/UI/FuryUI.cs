using UnityEngine;
using UnityEngine.UI;

public class FuryUI : MonoBehaviour
{
    public FurySystem fury;
    public Image furyBarFill;

    void Update()
    {
        if (fury != null)
        {
            furyBarFill.fillAmount = fury.fury;
        }
    }
    public void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        fury = FindAnyObjectByType<FurySystem>();
        furyBarFill = GameObject.FindGameObjectWithTag("FuryFill")?.GetComponent<Image>();
    }
}
