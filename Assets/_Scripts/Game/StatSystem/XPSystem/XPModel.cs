using System;
using UnityEngine;

public class XPModel
{
    private readonly int _baseXpToNextLevel;
    private readonly PlayerUnitConfig _playerUnitConfig;

    public event Action<int, int> OnXPChanged;
    public event Action<int> OnLevelChanged;

    public int CurrentXP { get; private set; }
    public int Level { get; private set; } = 1;
    public int XPToNextLevel { get; private set; } = 30;

    public XPModel(int startXp, int startLevel, int xpToNextLevel, PlayerUnitConfig playerUnitConfig = null)
    {
        CurrentXP = startXp;
        Level = startLevel;
        _baseXpToNextLevel = Mathf.Max(1, xpToNextLevel);
        _playerUnitConfig = playerUnitConfig;
        XPToNextLevel = ResolveXpToNextLevel(Level);
    }

    public void AddXP(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Attempted to add non-positive XP amount.");
            return;
        }

        if (IsAtMaxLevel())
        {
            return;
        }

        CurrentXP += amount;

        while (TryLevelUpOnce())
        {
        }

        OnXPChanged?.Invoke(CurrentXP, XPToNextLevel);
    }

    private bool TryLevelUpOnce()
    {
        if (XPToNextLevel <= 0 || CurrentXP < XPToNextLevel)
        {
            return false;
        }

        Level++;
        CurrentXP -= XPToNextLevel;
        XPToNextLevel = ResolveXpToNextLevel(Level);

        if (XPToNextLevel <= 0)
        {
            CurrentXP = 0;
        }

        Debug.Log("Level Up!");
        OnLevelChanged?.Invoke(Level);
        return true;
    }

    private bool IsAtMaxLevel()
    {
        return _playerUnitConfig != null
               && _playerUnitConfig.HasXpLevels
               && Level >= _playerUnitConfig.MaxLevel;
    }

    private int ResolveXpToNextLevel(int level)
    {
        if (_playerUnitConfig != null && _playerUnitConfig.HasXpLevels)
        {
            return _playerUnitConfig.EvaluateXpToNextLevel(level);
        }

        var safeLevel = Mathf.Max(1, level);
        return Mathf.Max(1, _baseXpToNextLevel * safeLevel);
    }
}
