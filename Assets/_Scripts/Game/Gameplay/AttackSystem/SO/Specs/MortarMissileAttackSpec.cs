using UnityEngine;

[CreateAssetMenu(fileName = "MortarMissileAttackSpec", menuName = "AttackSystem/Specs/Missile/Mortar")]
public sealed class MortarMissileAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Missile;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public MortarMissile MissilePrefab;
    public float DefaultMissileSpeed = 15f;
    public float DefaultAttackRate = 1f;
    public float DefaultDamage = 2f;
    public int DefaultProjectilesPerAttack = 1;
    public float DefaultDelayBetweenProjectiles;
    public float DefaultRadius = 2f;

    [Header("Mortar")]
    public float RadiusIncreasePerShot;
    [Tooltip("Height above launch point where shell waits before drop.")]
    public float MaxHeight = 10f;
    [Tooltip("Delay in seconds before shell starts falling.")]
    public float MinTravelTime = 0.5f;
}
