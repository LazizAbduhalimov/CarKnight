using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum AttackComboRewardType
{
    DamagePercentMultiplier = 0,
    MaxHealthPercent = 1,
    UnlockAttack = 2
}

[Serializable]
public sealed class AttackComboRequirement
{
    public AttackSO AttackConfig;
    [MinValue(1)] public int RequiredLevel = 1;
}

[Serializable]
public sealed class AttackComboReward
{
    public AttackComboRewardType RewardType = AttackComboRewardType.DamagePercentMultiplier;
    [Min(0f)] public float PercentValue = 25f;
    public AttackSO BonusAttackToUnlock;
    public bool RestoreHealthToFullOnApply;
}

[Serializable]
public sealed class AttackComboDefinition
{
    [SerializeField] private Sprite _comboSprite;
    [SerializeField] private string _comboId;
    [SerializeField] private string _displayName;
    [SerializeField] public string hintText;
    [SerializeField] private bool _activateOnlyOnce = true;
    [SerializeField] private List<AttackComboRequirement> _requirements = new();
    [SerializeField] private List<AttackComboReward> _rewards = new();

    public Sprite ComboSprite => _comboSprite;
    public string ComboId => _comboId;
    public string DisplayName => _displayName;
    public bool ActivateOnlyOnce => _activateOnlyOnce;
    public IReadOnlyList<AttackComboRequirement> Requirements => _requirements;
    public IReadOnlyList<AttackComboReward> Rewards => _rewards;

#if UNITY_EDITOR
    public void EnsureId()
    {
        if (!string.IsNullOrWhiteSpace(_comboId))
        {
            return;
        }

        _comboId = Guid.NewGuid().ToString("N");
    }
#endif
}

[CreateAssetMenu(fileName = "AttackComboTable", menuName = "AttackSystem/Combo/Attack Combo Table")]
public sealed class AttackComboTableSO : ScriptableObject
{
    [TableList][SerializeField] private List<AttackComboDefinition> _combos = new();

    public IReadOnlyList<AttackComboDefinition> Combos => _combos;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_combos == null)
        {
            return;
        }

        for (var i = 0; i < _combos.Count; i++)
        {
            _combos[i]?.EnsureId();
        }
    }
#endif
}
