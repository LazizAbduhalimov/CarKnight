using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySlotItem : MonoBehaviour
{
    [SerializeField] private Image IconImage;
    [SerializeField] private Image CooldownFillImage;
    [SerializeField] private TMP_Text CooldownText;
    
    public void SetIcon(Sprite icon)
    {
        IconImage.sprite = icon;
    }

    public void SetCooldownFill(float fillAmount)
    {
        CooldownFillImage.fillAmount = fillAmount;
    }

    public void SetCooldownText(string text)
    {
        CooldownText.text = text;
    }
}