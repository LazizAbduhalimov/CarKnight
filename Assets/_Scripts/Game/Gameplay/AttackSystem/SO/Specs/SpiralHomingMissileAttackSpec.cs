using UnityEngine;

[CreateAssetMenu(fileName = "SpiralHomingMissileAttackSpec", menuName = "AttackSystem/Specs/Missile/Spiral Homing")]
public sealed class SpiralHomingMissileAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Missile;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public SpiralHomingMissile MissilePrefab;
    public float DefaultMissileSpeed = 15f;
    public float DefaultAttackRate = 1f;
    public float DefaultDamage = 2f;

    [Header("Flight")]
    [Min(0.1f)] public float MinFlightTime = 0.8f;
}
