using UnityEngine;

[CreateAssetMenu(fileName = "MagneticFieldAttackSpec", menuName = "AttackSystem/Specs/Field AOE/Magnetic Field")]
public sealed class MagneticFieldAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.FieldAoe;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public float DefaultAttackRate = 0.2f;
    public float DefaultDamage = 2f;

    [Header("Magnetic Field")]
    public float Radius = 6f;
    public FollowerInstant MagneticEffectPrefab;
}
