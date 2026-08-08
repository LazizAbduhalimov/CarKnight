using UnityEngine;

public sealed class MovementModeSelectionOption : ICardSelectionOption
{
    private readonly MovementMode _mode;
    private readonly MovementFsmController _controller;
    private readonly Sprite _icon;
    private readonly string _title;
    private readonly string _description;

    public MovementModeSelectionOption(
        MovementMode mode,
        MovementFsmController controller,
        Sprite icon,
        string title,
        string description)
    {
        _mode = mode;
        _controller = controller;
        _icon = icon;
        _title = title;
        _description = description;
    }

    public CardChoiceViewData ViewData => new(
        _icon,
        _title,
        _description);

    public bool CanSelect()
    {
        return _controller != null && _controller.CurrentMode != _mode && !_controller.chosenOnce;
    }

    public void Select()
    {
        if (!CanSelect())
        {
            Debug.LogWarning($"Режим {_mode} уже активен или контроллер недоступен.");
            return;
        }

        _controller.SwitchTo(_mode);
        _controller.chosenOnce = true;

        Debug.Log($"<color=cyan>[Choice]</color> Игрок выбрал новый режим движения: <b>{_mode}</b>");
    }
}