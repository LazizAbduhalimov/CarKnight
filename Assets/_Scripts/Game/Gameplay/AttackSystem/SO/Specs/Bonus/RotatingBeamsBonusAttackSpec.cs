using UnityEngine;

[CreateAssetMenu(fileName = "RotatingBeamsBonusAttackSpec", menuName = "AttackSystem/Specs/Bonus/Rotating Beams")]
public sealed class RotatingBeamsBonusAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Raycast;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public float DefaultAttackRate = 0.2f;
    public float DefaultDamage = 2f;
    public float BurstCooldown = 2f;

    [Header("Beam Ring")]
    public int BeamCount = 5;
    public float RotationSpeed = 120f;
    public float Range = 18f;
    public float BeamRadius = 0.35f;
    public float HitHeight = 5f;
    public float MuzzleHeight = 1f;
    public float BurstDuration = 2f;
    public bool DrawDebugRays;
    public Transform BeamPrefab;
    public DamagesProContainer.DamageNumberPreset DamagePreset = DamagesProContainer.DamageNumberPreset.NeonText;
}
