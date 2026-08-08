using UnityEngine;

[CreateAssetMenu(fileName = "BonusRandomMissileAttackSpec", menuName = "AttackSystem/Specs/Bonus/Random Missile Volley")]
public sealed class BonusRandomMissileAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Missile;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public MortarMissile MissilePrefab;
    public float DefaultMissileSpeed = 15f;
    public float DefaultAttackRate = 1f;
    public float DefaultDamage = 3f;
    public float DefaultRadius = 15f;

    [Header("Explosion Radius Random")]
    [Tooltip("Minimum explosion diameter per missile.")]
    public float MinTargetRadius = 0f;
    [Tooltip("Maximum explosion diameter per missile.")]
    public float MaxTargetRadius = 10f;

    [Header("Mortar")]
    public float SpawnHeight = 1f;
    [Tooltip("Height above launch point where shell waits before drop.")]
    public float MaxHeight = 10f;
    [Tooltip("Seconds until impact.")]
    public float MinTravelTime = 0.5f;
    public bool UseMissileExplosionPool = true;
}
