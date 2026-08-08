using UnityEngine;

[CreateAssetMenu(fileName = "BeamRaycastAttackSpec", menuName = "AttackSystem/Specs/Raycast/Beam")]
public sealed class BeamRaycastAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Raycast;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public float DefaultAttackRate = 1f;
    public float DefaultDamage = 2f;

    [Header("Beam")]
    public LineRenderer BeamLinePrefab;
    public float BeamDistance = 50f;
    public float BeamRadius = 0.5f;
    public LayerMask BeamMask = ~0;
    public float BeamLifetime = 0.3f;
}
