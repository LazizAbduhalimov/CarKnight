using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class SelectionMenuMb : MonoBehaviour
{
    public Action<int> OnChoiceChosen;
    public AbilityChoiceButton[] AbilityChoiceButtons;

    public int SelectedChoiceIndex { get; private set; } = -1;

    private void OnValidate()
    {
        if (AbilityChoiceButtons == null || AbilityChoiceButtons.Length == 0)
        {
            AbilityChoiceButtons = GetComponentsInChildren<AbilityChoiceButton>();
        }
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        transform.localScale = Vector3.zero;
        Tween.Scale(transform, Vector3.one, duration: 0.15f, ease: Ease.InOutSine, useUnscaledTime: true);
        ListenMouseSelection();
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        RemoveListenMouseSelection();
    }


    public void UpdateView(IReadOnlyList<CardChoiceViewData> choices)
    {
        SelectedChoiceIndex = choices.Count > 0 ? 0 : -1;

        for (var i = 0; i < AbilityChoiceButtons.Length; i++)
        {
            var button = AbilityChoiceButtons[i];
            var hasChoice = i < choices.Count;
            button.gameObject.SetActive(hasChoice);
            if (!hasChoice)
            {
                continue;
            }

            var choice = choices[i];
            button.AbilityImage.sprite = choice.Icon;
            button.AbilityNameText.text = choice.Title;
            button.AbilityDescriptionText.text = choice.Description;
            button.transform.localScale = Vector3.one;
        }

        if (SelectedChoiceIndex != -1)
        {
            PreSelectChoice();
        }
    }

    private void ConfirmSelection()
    {
        if (SelectedChoiceIndex < 0)
        {
            return;
        }

        Debug.Log($"Choice at index {SelectedChoiceIndex} confirmed.");
        OnChoiceChosen?.Invoke(SelectedChoiceIndex);
    }

    private void ListenMouseSelection()
    {
        for (var i = 0; i < AbilityChoiceButtons.Length; i++)
        {
            var index = i;
            AbilityChoiceButtons[i].Hovered += () =>
            {
                if (!AbilityChoiceButtons[index].gameObject.activeSelf)
                {
                    return;
                }

                SelectedChoiceIndex = index;
                PreSelectChoice();
            };

            AbilityChoiceButtons[i].Button.onClick.AddListener(() =>
            {
                if (!AbilityChoiceButtons[index].gameObject.activeSelf)
                {
                    return;
                }

                Debug.Log($"Choice at index {index} selected via mouse.");
                SelectedChoiceIndex = index;
                PreSelectChoice();
                ConfirmSelection();
            });
        }
    }

    private void RemoveListenMouseSelection()
    {
        for (var i = 0; i < AbilityChoiceButtons.Length; i++)
        {
            AbilityChoiceButtons[i].ClearHoverSubscribers();
            AbilityChoiceButtons[i].Button.onClick.RemoveAllListeners();
        }
    }

    private void PreSelectChoice()
    {
        var activeIndices = GetActiveIndices();
        if (activeIndices.Count == 0)
        {
            SelectedChoiceIndex = -1;
            return;
        }

        var selectedActiveIndex = GetNearestActiveChoiceIndex(activeIndices, SelectedChoiceIndex);
        SelectedChoiceIndex = activeIndices[Mathf.Clamp(selectedActiveIndex, 0, activeIndices.Count - 1)];

        for (var i = 0; i < AbilityChoiceButtons.Length; i++)
        {
            var button = AbilityChoiceButtons[i];
            if (!button.gameObject.activeSelf)
            {
                continue;
            }

            var isSelected = i == SelectedChoiceIndex;
            var targetScale = isSelected ? Vector3.one * 1.1f : Vector3.one;
            Tween.Scale(button.transform, targetScale, duration: 0.1f, ease: Ease.InOutSine, useUnscaledTime: true);
        }
    }

    private List<int> GetActiveIndices()
    {
        var result = new List<int>();
        for (var i = 0; i < AbilityChoiceButtons.Length; i++)
        {
            if (AbilityChoiceButtons[i].gameObject.activeSelf)
            {
                result.Add(i);
            }
        }

        return result;
    }

    private static int GetNearestActiveChoiceIndex(List<int> activeIndices, int currentIndex)
    {
        if (activeIndices.Count == 0)
        {
            return -1;
        }

        if (currentIndex <= activeIndices[0])
        {
            return 0;
        }

        for (var i = 0; i < activeIndices.Count; i++)
        {
            if (activeIndices[i] >= currentIndex)
            {
                return i;
            }
        }

        return activeIndices.Count - 1;
    }
}
