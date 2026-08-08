using UnityEngine;

[CreateAssetMenu(fileName = "MachineGunRaycastAttackSpec", menuName = "AttackSystem/Specs/Raycast/Machine Gun")]
public sealed class MachineGunRaycastAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Raycast;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public float DefaultAttackRate = 0.1f;
    public float DefaultDamage = 2f;

    [Header("Machine Gun")]
    public float Range = 30f;
    public float BulletRadius = 0.05f;
    public float SpreadRadius = 0.25f;
    [Range(0f, 180f)] public float ForwardConeHalfAngle = 45f;
    public float MuzzleHeight = 1f;
    public LayerMask HitMask = ~0;
    public DamagesProContainer.DamageNumberPreset DamagePreset = DamagesProContainer.DamageNumberPreset.NeonText;
    public bool DrawDebugRays;

    [Header("Tracer VFX")]
    public float TracerTravelDuration = 0.1f;
    public float TracerLifetime = 0.08f;

    [Header("Fallbacks")]
    public bool UseDefaultImpactIfMissing = true;
}
