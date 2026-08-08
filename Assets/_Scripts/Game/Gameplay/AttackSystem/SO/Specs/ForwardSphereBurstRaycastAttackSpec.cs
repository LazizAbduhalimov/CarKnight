using UnityEngine;

[CreateAssetMenu(fileName = "ForwardSphereBurstRaycastAttackSpec", menuName = "AttackSystem/Specs/Raycast/Forward Sphere Burst")]
public sealed class ForwardSphereBurstRaycastAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Raycast;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public float DefaultAttackRate = 2f;
    public float DefaultDamage = 2f;

    [Header("Forward Sphere Burst")]
    public float Range = 30f;
    public float Radius = 0.5f;
    public float BurstDuration = 2f;
    public int SphereCastCount = 10;
    public float MuzzleHeight = 1f;
    public bool FlattenDirectionToGround = true;
    public LayerMask HitMask = ~0;
    public Transform BeamPrefab;
    public DamagesProContainer.DamageNumberPreset DamagePreset = DamagesProContainer.DamageNumberPreset.NeonText;
    public bool DrawDebugRays;
}
