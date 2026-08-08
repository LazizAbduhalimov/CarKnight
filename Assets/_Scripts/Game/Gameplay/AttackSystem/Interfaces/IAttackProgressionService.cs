using System;
using System.Collections.Generic;

public interface IAttackProgressionService : IDisposable
{
    IReadOnlyList<OwnedAttack> OwnedAttacks { get; }
    event Action OwnedAttacksChanged;
    float GlobalDamageMultiplier { get; }

    bool Has(AttackSO attackConfig);
    OwnedAttack GetOwnedAttack(AttackSO attackConfig);
    int GetLevel(AttackSO attackConfig);
    bool TryUnlock(AttackSO attackConfig);
    bool TryUpgrade(AttackSO attackConfig);
    bool TryUpgrade(string attackId);
    void SetGlobalDamageMultiplier(float multiplier);
    void SetDamageMultiplierSource(string sourceId, float multiplier);
    void RemoveDamageMultiplierSource(string sourceId);
    void InitializeStartingAttacks(IReadOnlyList<AttackSO> startingAttackConfigs);
    void Tick();
}
