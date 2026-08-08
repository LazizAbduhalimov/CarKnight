using UnityEngine;
using System.Text;

public sealed class AttackUnlockSelectionOption : ICardSelectionOption
{
    private readonly AttackSO _attackConfig;
    private readonly IAttackProgressionService _progressionService;

    public AttackUnlockSelectionOption(AttackSO attackConfig, IAttackProgressionService progressionService)
    {
        _attackConfig = attackConfig;
        _progressionService = progressionService;
    }

    public CardChoiceViewData ViewData => new (
        _attackConfig != null ? _attackConfig.AbilityIcon : null,
        _attackConfig != null ? _attackConfig.AbilityName : "Unknown",
        _attackConfig != null ? _attackConfig.AbilityDescription : string.Empty);

    public bool CanSelect()
    {
        return _attackConfig != null &&
               _progressionService != null &&
               _attackConfig.Spec != null &&
               !_progressionService.Has(_attackConfig);
    }

    public void Select()
    {
        if (!CanSelect())
        {
            Debug.LogWarning("Attack unlock option cannot be selected.");
            return;
        }

        _progressionService.TryUnlock(_attackConfig);
    }
}

public sealed class AttackUpgradeSelectionOption : ICardSelectionOption
{
    private readonly AttackSO _attackConfig;
    private readonly IAttackProgressionService _progressionService;

    public AttackUpgradeSelectionOption(AttackSO attackConfig, IAttackProgressionService progressionService)
    {
        _attackConfig = attackConfig;
        _progressionService = progressionService;
    }

    public CardChoiceViewData ViewData
    {
        get
        {
            if (_attackConfig == null || _progressionService == null)
            {
                return new CardChoiceViewData(null, "Upgrade", string.Empty);
            }

            var current = _progressionService.GetLevel(_attackConfig);
            var max = _attackConfig == null ? 0 : AttackLevelTableUtility.GetMaxLevel(_attackConfig.LevelTable);
            var next = Mathf.Min(current + 1, max);

            return new CardChoiceViewData(
                _attackConfig.AbilityIcon,
                $"{_attackConfig.AbilityName} +",
                BuildUpgradeDescription(current, next));
        }
    }

    public bool CanSelect()
    {
        if (_attackConfig == null || _progressionService == null)
        {
            return false;
        }

        if (!_progressionService.Has(_attackConfig))
        {
            return false;
        }

        return _progressionService.GetLevel(_attackConfig) < AttackLevelTableUtility.GetMaxLevel(_attackConfig.LevelTable);
    }

    public void Select()
    {
        if (!CanSelect())
        {
            Debug.LogWarning("Attack upgrade option cannot be selected.");
            return;
        }

        _progressionService.TryUpgrade(_attackConfig);
    }

    private string BuildUpgradeDescription(int currentLevel, int nextLevel)
    {
        var currentData = AttackLevelTableUtility.GetLevelOrDefault(_attackConfig.LevelTable, currentLevel);
        var nextData = AttackLevelTableUtility.GetLevelOrDefault(_attackConfig.LevelTable, nextLevel);

        var builder = new StringBuilder();
        builder.Append($"Upgrade level {currentLevel} -> {nextLevel}");

        AppendChanged(builder, "Damage", currentData.Damage, nextData.Damage);
        AppendChanged(builder, "Attack rate", currentData.AttackRate, nextData.AttackRate);
        AppendChanged(builder, "Missile speed", currentData.MissileSpeed, nextData.MissileSpeed);
        AppendChanged(builder, "Radius", currentData.Radius, nextData.Radius);
        AppendChanged(builder, "Projectiles", currentData.ProjectilesPerAttack, nextData.ProjectilesPerAttack);
        AppendChanged(builder, "Delay", currentData.DelayBetweenProjectiles, nextData.DelayBetweenProjectiles);

        return builder.ToString();
    }

    private static void AppendChanged(StringBuilder builder, string label, float from, float to)
    {
        if (Mathf.Approximately(from, to))
        {
            return;
        }

        builder.Append('\n');
        builder.Append(label);
        builder.Append(": ");
        builder.Append(FormatNumber(from));
        builder.Append(" -> ");
        builder.Append(FormatNumber(to));
    }

    private static void AppendChanged(StringBuilder builder, string label, int from, int to)
    {
        if (from == to)
        {
            return;
        }

        builder.Append('\n');
        builder.Append(label);
        builder.Append(": ");
        builder.Append(from);
        builder.Append(" -> ");
        builder.Append(to);
    }

    private static string FormatNumber(float value)
    {
        return Mathf.Approximately(value, Mathf.Round(value))
            ? Mathf.RoundToInt(value).ToString()
            : value.ToString("0.##");
    }
}
