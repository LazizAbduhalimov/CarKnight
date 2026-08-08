using UnityEngine;

[CreateAssetMenu(fileName = "LowHealthDamageBonusAttackSpec", menuName = "AttackSystem/Specs/Bonus/Low Health Damage Bonus")]
public sealed class LowHealthDamageBonusAttackSpec : AttackSpecBase, IAttackStatDefaults, IHiddenInUI
{
    public override AttackSpecKind Kind => AttackSpecKind.FieldAoe;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public float DefaultAttackRate = 0.1f;
    public float DefaultDamage = 0f;

    [Header("Low HP Bonus")]
    [Range(0f, 1f)] public float HealthThresholdNormalized = 0.3f;
    public float BonusDamagePercent = 60f;
    public bool ScaleByMissingHealth = true;
    public float RecheckInterval = 0.1f;
}
