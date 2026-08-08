using UnityEngine;

[CreateAssetMenu(fileName = "ArcMissileAttackSpec", menuName = "AttackSystem/Specs/Missile/Arc")]
public sealed class ArcMissileAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.Missile;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public ArcMissile MissilePrefab;
    public float DefaultMissileSpeed = 15f;
    public float DefaultAttackRate = 1f;
    public float DefaultDamage = 2f;
    public int DefaultProjectilesPerAttack = 1;
    public float DefaultDelayBetweenProjectiles;
    public float DefaultRadius = 2f;

    [Header("Arc")]
    public float ArcHeight = 5f;
    public float RadiusIncreasePerShot;
}
