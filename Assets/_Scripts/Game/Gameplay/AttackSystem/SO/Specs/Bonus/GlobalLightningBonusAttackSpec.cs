using UnityEngine;

[CreateAssetMenu(fileName = "GlobalLightningBonusAttackSpec", menuName = "AttackSystem/Specs/Bonus/Global Lightning")]
public sealed class GlobalLightningBonusAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Raycast;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public float DefaultAttackRate = 1.2f;
    public float DefaultDamage = 4f;

    [Header("Global Strike")]
    public float GlobalRadius = 250f;
    public GlobalLightningBonusAttack.LightningImpactColor ImpactColor = GlobalLightningBonusAttack.LightningImpactColor.Red;
}
