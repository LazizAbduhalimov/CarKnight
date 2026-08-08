using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityChoiceButton : MonoBehaviour, IPointerEnterHandler
{
    public Image AbilityImage;
    public Button Button;
    public TMP_Text AbilityNameText;
    public TMP_Text AbilityDescriptionText;

    public event Action Hovered;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Hovered?.Invoke();
    }

    public void ClearHoverSubscribers()
    {
        Hovered = null;
    }
}
