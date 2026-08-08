using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class UnitComponentConfigData
{
    [SerializeField] private bool _overrideMaxHealth;
    [SerializeField, MinValue(1)] private int _maxHealth = 100;
    [Space]
    [SerializeField] private bool _overrideAttackerSearchSettings;
    [SerializeField, MinValue(0.01f)] private float _enemySearchUpdateInterval = 0.5f;
    [SerializeField, MinValue(0f)] private float _enemyDetectionRange = 30f;
    [SerializeField] private LayerMask _enemyLayerMask;

    [Space]
    [SerializeField] private bool _overrideStartingAttacks;
    [SerializeField] private List<AttackSO> _startingAttackConfigs = new();
    [SerializeField] private bool _overrideDropTable;
    [SerializeField] private List<DropItem> _drops = new();

    public void Apply(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (_overrideMaxHealth && target.TryGetComponent(out HealthComponent healthComponent))
        {
            healthComponent.SetMaxHealth(_maxHealth, resetCurrentHealth: true);
        }

        if (target.TryGetComponent(out Attacker attacker))
        {
            if (_overrideAttackerSearchSettings)
            {
                var shouldReinitialize = !_overrideStartingAttacks;
                attacker.SetEnemySearchSettings(
                    _enemySearchUpdateInterval,
                    _enemyDetectionRange,
                    _enemyLayerMask,
                    reinitializeIfReady: shouldReinitialize);
            }

            if (_overrideStartingAttacks)
            {
                attacker.SetStartingAttackConfigs(_startingAttackConfigs, reinitializeIfReady: true);
            }
        }

        if (_overrideDropTable && target.TryGetComponent(out DropTable dropTable))
        {
            dropTable.SetDrops(_drops);
        }
    }
}
