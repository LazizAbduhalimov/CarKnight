using UnityEngine;

[CreateAssetMenu(fileName = "AutoAimMissileAttackSpec", menuName = "AttackSystem/Specs/Missile/Auto Aim")]
public sealed class AutoAimMissileAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Missile;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public AutoAimMissile MissilePrefab;
    public float DefaultMissileSpeed = 15f;
    public float DefaultAttackRate = 1f;
    public float DefaultDamage = 2f;

    [Header("Auto Aim")]
    public float TurnRatePerSecond = 180f;
}
