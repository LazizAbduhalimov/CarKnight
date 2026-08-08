using UnityEngine;

[CreateAssetMenu(fileName = "LightningRaycastAttackSpec", menuName = "AttackSystem/Specs/Raycast/Lightning")]
public sealed class LightningRaycastAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Raycast;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public float DefaultAttackRate = 1f;
    public float DefaultDamage = 2f;

    [Header("Lightning")]
    public float ChainRadius = 10f;
    public float ChainInterval = 0.2f;
    public int MaxBounces = 3;
    public float ImpactRadius = 2f;
    public LayerMask EnemyMask = ~0;
}
