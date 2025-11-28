using UnityEngine;
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
}
