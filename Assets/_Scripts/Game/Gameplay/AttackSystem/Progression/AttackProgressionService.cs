using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AttackProgressionService : IAttackProgressionService
{
    private readonly AbilityContainer _abilityContainer;
    private readonly string _ownerName;

    public AttackProgressionService(AbilityContainer abilityContainer, string ownerName)
    {
        _abilityContainer = abilityContainer;
        _ownerName = ownerName;
    }

    public IReadOnlyList<OwnedAttack> OwnedAttacks => _abilityContainer?.OwnedAttacks ?? Array.Empty<OwnedAttack>();
    public event Action OwnedAttacksChanged;
    public float GlobalDamageMultiplier => _abilityContainer?.GlobalDamageMultiplier ?? 1f;

    public bool Has(AttackSO attackConfig)
    {
        return _abilityContainer != null && _abilityContainer.Has(attackConfig);
    }

    public OwnedAttack GetOwnedAttack(AttackSO attackConfig)
    {
        if (_abilityContainer == null || attackConfig == null)
        {
            return null;
        }

        var ownedAttacks = _abilityContainer.OwnedAttacks;
        for (var i = 0; i < ownedAttacks.Count; i++)
        {
            var ownedAttack = ownedAttacks[i];
            if (ownedAttack != null && ReferenceEquals(ownedAttack.Config, attackConfig))
            {
                return ownedAttack;
            }
        }

        return null;
    }

    public int GetLevel(AttackSO attackConfig)
    {
        return _abilityContainer?.GetLevel(attackConfig) ?? 0;
    }

    public bool TryUnlock(AttackSO attackConfig)
    {
        if (_abilityContainer == null || !_abilityContainer.Unlock(attackConfig))
        {
            return false;
        }

        OwnedAttacksChanged?.Invoke();
        return true;
    }

    public bool TryUpgrade(AttackSO attackConfig)
    {
        if (_abilityContainer == null || !_abilityContainer.Upgrade(attackConfig))
        {
            return false;
        }

        OwnedAttacksChanged?.Invoke();
        return true;
    }

    public bool TryUpgrade(string attackId)
    {
        if (_abilityContainer == null || !_abilityContainer.Upgrade(attackId))
        {
            return false;
        }

        OwnedAttacksChanged?.Invoke();
        return true;
    }

    public void SetGlobalDamageMultiplier(float multiplier)
    {
        _abilityContainer?.SetGlobalDamageMultiplier(multiplier);
    }

    public void SetDamageMultiplierSource(string sourceId, float multiplier)
    {
        _abilityContainer?.SetDamageMultiplierSource(sourceId, multiplier);
    }

    public void RemoveDamageMultiplierSource(string sourceId)
    {
        _abilityContainer?.RemoveDamageMultiplierSource(sourceId);
    }

    public void InitializeStartingAttacks(IReadOnlyList<AttackSO> startingAttackConfigs)
    {
        if (_abilityContainer == null)
        {
            return;
        }

        if (startingAttackConfigs == null)
        {
            Debug.LogWarning("Starting attack config list is null. No attacks will be initialized.");
            OwnedAttacksChanged?.Invoke();
            return;
        }

        for (var i = 0; i < startingAttackConfigs.Count; i++)
        {
            var attackSO = startingAttackConfigs[i];
            if (attackSO == null)
            {
                continue;
            }

            if (!_abilityContainer.Unlock(attackSO))
            {
                Debug.LogWarning($"Unable to unlock starting attack '{attackSO.name}' on '{_ownerName}'.");
            }
        }

        OwnedAttacksChanged?.Invoke();
    }

    public void Tick()
    {
        _abilityContainer?.Tick();
    }

    public void Dispose()
    {
        _abilityContainer?.Dispose();
    }
}
