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
}
