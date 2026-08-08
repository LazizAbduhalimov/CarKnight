using System.Collections.Generic;

public sealed class MovementModeCardSelectionProvider : ICardSelectionProvider
{
    private readonly MovementFsmController _controller;
    private readonly List<MovementModeDisplayData> _displayConfigs;

    public MovementModeCardSelectionProvider(MovementFsmController controller, List<MovementModeDisplayData> configs)
    {
        _controller = controller;
        _displayConfigs = configs;
    }

    public IReadOnlyList<ICardSelectionOption> GetOptions()
    {
        var options = new List<ICardSelectionOption>();

        if (_displayConfigs == null) return options;

        foreach (var config in _displayConfigs)
        {
            var option = new MovementModeSelectionOption(
                config.movementMode,
                _controller,
                config.icon,
                config.Title,
                config.Description);

            if (option.CanSelect())
            {
                options.Add(option);
            }
        }

        return options;
    }
}