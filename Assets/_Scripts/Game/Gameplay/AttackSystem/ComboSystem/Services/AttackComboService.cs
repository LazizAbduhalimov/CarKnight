using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AttackComboService : IDisposable
{
    private readonly IAttackProgressionService _progressionService;
    private readonly HealthComponent _healthComponent;
    private readonly AttackComboTableSO _comboTable;
    private readonly Dictionary<string, int> _activationCountByComboId = new(StringComparer.Ordinal);
    private readonly Dictionary<string, bool> _wasSatisfiedByComboId = new(StringComparer.Ordinal);
    private readonly int _baseMaxHealth;

    private float _totalDamagePercentBonus;
    private float _totalMaxHealthPercentBonus;
    private bool _isEvaluating;

    public event Action<AttackComboDefinition> ComboActivated;

    public AttackComboService(
        IAttackProgressionService progressionService,
        HealthComponent healthComponent,
        AttackComboTableSO comboTable)
    {
        _progressionService = progressionService;
        _healthComponent = healthComponent;
        _comboTable = comboTable;
        _baseMaxHealth = _healthComponent != null ? Mathf.Max(1, _healthComponent.MaxHealth) : 1;
    }

    public void EvaluateCombos()
    {
        if (_isEvaluating || _progressionService == null || _comboTable == null)
        {
            return;
        }

        _isEvaluating = true;
        try
        {
            var combos = _comboTable.Combos;
            if (combos == null || combos.Count == 0)
            {
                return;
            }

            for (var i = 0; i < combos.Count; i++)
            {
                var combo = combos[i];
                if (combo == null)
                {
                    continue;
                }

                var comboId = ResolveComboId(combo, i);
                var isSatisfied = IsSatisfied(combo);
                _wasSatisfiedByComboId.TryGetValue(comboId, out var wasSatisfied);

                if (!isSatisfied)
                {
                    _wasSatisfiedByComboId[comboId] = false;
                    continue;
                }

                if (wasSatisfied)
                {
                    continue;
                }

                _wasSatisfiedByComboId[comboId] = true;
                _activationCountByComboId.TryGetValue(comboId, out var currentActivationCount);

                if (combo.ActivateOnlyOnce && currentActivationCount > 0)
                {
                    continue;
                }

                _activationCountByComboId[comboId] = currentActivationCount + 1;
                ApplyRewards(combo);
                ComboActivated?.Invoke(combo);
            }
        }
        finally
        {
            _isEvaluating = false;
        }
    }

    public void Dispose()
    {
        ComboActivated = null;
        _activationCountByComboId.Clear();
        _wasSatisfiedByComboId.Clear();
    }

    private bool IsSatisfied(AttackComboDefinition combo)
    {
        var requirements = combo.Requirements;
        if (requirements == null || requirements.Count == 0)
        {
            return false;
        }

        for (var i = 0; i < requirements.Count; i++)
        {
            var requirement = requirements[i];
            if (requirement == null || requirement.AttackConfig == null)
            {
                return false;
            }

            var currentLevel = _progressionService.GetLevel(requirement.AttackConfig);
            if (currentLevel < Mathf.Max(1, requirement.RequiredLevel))
            {
                return false;
            }
        }

        return true;
    }

    private void ApplyRewards(AttackComboDefinition combo)
    {
        var rewards = combo.Rewards;
        if (rewards == null || rewards.Count == 0)
        {
            return;
        }

        for (var i = 0; i < rewards.Count; i++)
        {
            var reward = rewards[i];
            if (reward == null)
            {
                continue;
            }

            switch (reward.RewardType)
            {
                case AttackComboRewardType.DamagePercentMultiplier:
                    ApplyDamageBonus(reward.PercentValue);
                    break;
                case AttackComboRewardType.MaxHealthPercent:
                    ApplyMaxHealthBonus(reward.PercentValue, reward.RestoreHealthToFullOnApply);
                    break;
                case AttackComboRewardType.UnlockAttack:
                    if (reward.BonusAttackToUnlock != null)
                    {
                        _progressionService.TryUnlock(reward.BonusAttackToUnlock);
                    }
                    break;
            }
        }
    }

    private void ApplyDamageBonus(float percentValue)
    {
        if (percentValue <= 0f)
        {
            return;
        }

        _totalDamagePercentBonus += percentValue;
        var damageMultiplier = 1f + _totalDamagePercentBonus / 100f;
        _progressionService.SetGlobalDamageMultiplier(damageMultiplier);
    }

    private void ApplyMaxHealthBonus(float percentValue, bool restoreHealthToFullOnApply)
    {
        if (percentValue <= 0f || _healthComponent == null)
        {
            return;
        }

        _totalMaxHealthPercentBonus += percentValue;
        var maxHealthMultiplier = 1f + _totalMaxHealthPercentBonus / 100f;
        var updatedMaxHealth = Mathf.Max(1, Mathf.RoundToInt(_baseMaxHealth * maxHealthMultiplier));
        _healthComponent.SetMaxHealth(updatedMaxHealth, restoreHealthToFullOnApply);
    }

    private static string ResolveComboId(AttackComboDefinition combo, int fallbackIndex)
    {
        if (combo == null)
        {
            return fallbackIndex.ToString();
        }

        if (!string.IsNullOrWhiteSpace(combo.ComboId))
        {
            return combo.ComboId;
        }

        if (!string.IsNullOrWhiteSpace(combo.DisplayName))
        {
            return combo.DisplayName;
        }

        return fallbackIndex.ToString();
    }
}
