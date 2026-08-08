using UnityEngine;

[CreateAssetMenu(fileName = "RadialArcMissileAttackSpec", menuName = "AttackSystem/Specs/Missile/Radial Arc")]
public sealed class RadialArcMissileAttackSpec : AttackSpecBase, IAttackStatDefaults
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

    [Header("Radial Arc")]
    public float ArcHeight = 5f;
    public int ProjectilesPerCircle = 8;
    public float DelayBetweenCircleProjectiles;
    public float CircleRadius = 6f;
    public float AngleOffsetDegrees;
    public bool UseOwnerForwardAsBaseAngle = true;
}
