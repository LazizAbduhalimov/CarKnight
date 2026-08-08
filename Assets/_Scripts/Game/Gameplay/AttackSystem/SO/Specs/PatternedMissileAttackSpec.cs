using UnityEngine;

[CreateAssetMenu(fileName = "PatternedMissileAttackSpec", menuName = "AttackSystem/Specs/Missile/Patterned")]
public sealed class PatternedMissileAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Missile;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public PatternedTrailMissile MissilePrefab;
    public float DefaultMissileSpeed = 15f;
    public float DefaultAttackRate = 1f;
    public float DefaultDamage = 2f;

    [Header("Pattern")]
    public float Amplitude = 1f;
    public float NoiseScale = 2f;
    public float NoiseSpeed = 3f;
}
