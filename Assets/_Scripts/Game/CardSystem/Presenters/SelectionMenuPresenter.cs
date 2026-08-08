using System;
using System.Collections.Generic;
using OnlyFun.EventBus;
using UnityEngine;

public class SelectionMenuPresenter : IDisposable
{
    private readonly EventBus _eventBus;
    private readonly SelectionMenuMb _view;
    private readonly ICardSelectionProvider _selectionProvider;
    private List<ICardSelectionOption> _currentOptions = new List<ICardSelectionOption>();
    private int _pendingLevelUps;

    public SelectionMenuPresenter(EventBus eventBus, SelectionMenuMb view, ICardSelectionProvider selectionProvider)
    {
        _eventBus = eventBus;
        _view = view;
        _selectionProvider = selectionProvider;
        Subscribe();
    }

    public void SetupChoice(IReadOnlyList<ICardSelectionOption> options)
    {
        _currentOptions = options != null ? new List<ICardSelectionOption>(options) : new List<ICardSelectionOption>();
        if (_currentOptions.Count == 0)
        {
            _view.SetActive(false);
            return;
        }

        _view.SetActive(true);
        UpdateView();
    }

    private void UpdateView()
    {
        var viewData = new List<CardChoiceViewData>(_currentOptions.Count);
        for (var i = 0; i < _currentOptions.Count; i++)
        {
            viewData.Add(_currentOptions[i].ViewData);
        }

        _view.UpdateView(viewData);
    }

    private void OnChoiceChosen(int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= _currentOptions.Count)
        {
            Debug.LogWarning($"Invalid card option index: {optionIndex}");
            return;
        }

        var selectedOption = _currentOptions[optionIndex];
        if (selectedOption == null || !selectedOption.CanSelect())
        {
            Debug.LogWarning($"Card option at index {optionIndex} cannot be selected.");
            return;
        }

        _view.SetActive(false);
        selectedOption.Select();
        TryShowNextLevelUpChoice();
    }

    private void OnLevelUp(ELevelUp levelUpEvent)
    {
        Debug.Log("<color=red>Level Up Choice</color>");
        _pendingLevelUps++;
        TryShowNextLevelUpChoice();
    }

    private void TryShowNextLevelUpChoice()
    {
        if (_view.gameObject.activeSelf)
        {
            return;
        }

        while (_pendingLevelUps > 0)
        {
            _pendingLevelUps--;
            var options = BuildSelectableOptions(_selectionProvider.GetOptions());
            if (options.Count == 0)
            {
                continue;
            }

            SetupChoice(options);
            return;
        }

        _view.SetActive(false);
    }

    private static List<ICardSelectionOption> BuildSelectableOptions(IReadOnlyList<ICardSelectionOption> options)
    {
        var result = new List<ICardSelectionOption>();
        if (options == null)
        {
            return result;
        }

        for (var i = 0; i < options.Count; i++)
        {
            var option = options[i];
            if (option != null && option.CanSelect())
            {
                result.Add(option);
            }
        }

        return result;
    }

    private void Subscribe()
    {
        _view.OnChoiceChosen += OnChoiceChosen;
        _eventBus.Subscribe<ELevelUp>(OnLevelUp);
    }

    private void Unsubscribe()
    {
        _view.OnChoiceChosen -= OnChoiceChosen;
        _eventBus.Unsubscribe<ELevelUp>(OnLevelUp);
    }

    public void Dispose()
    {
        Unsubscribe();
    }
}
