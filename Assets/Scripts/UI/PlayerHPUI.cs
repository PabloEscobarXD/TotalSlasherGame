using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerDamageReceiver player;
    public Image healthBarFill;

    void Update()
    {
        if (player != null)
        {
            healthBarFill.fillAmount = player.currentHP / player.maxHP;
        }
    }
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = FindAnyObjectByType<PlayerDamageReceiver>();
        healthBarFill = GameObject.FindGameObjectWithTag("HPFill")?.GetComponent<Image>();
    }
}
