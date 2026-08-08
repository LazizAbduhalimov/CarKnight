using UnityEngine;

[CreateAssetMenu(fileName = "ArtilleryFireZoneBonusAttackSpec", menuName = "AttackSystem/Specs/Bonus/Artillery Fire Zone")]
public sealed class ArtilleryFireZoneBonusAttackSpec : AttackSpecBase, IAttackStatDefaults
{
    public override AttackSpecKind Kind => AttackSpecKind.FieldAoe;
    public float DefaultAttackRateValue => DefaultAttackRate;
    public float DefaultDamageValue => DefaultDamage;

    [Header("Shared")]
    public float DefaultAttackRate = 1f;
    public float DefaultDamage = 2f;

    [Header("Zone")]
    public PersistentDamageZone FireZonePrefab;
    public int InitialPoolSize = 6;
    public float ZoneRadius = 3f;
    public float ZoneDuration = 3f;
    public float ZoneTickInterval = 0.5f;
    public float SpawnHeightOffset;
    public DamagesProContainer.DamageNumberPreset DamagePreset = DamagesProContainer.DamageNumberPreset.Flames;
}
