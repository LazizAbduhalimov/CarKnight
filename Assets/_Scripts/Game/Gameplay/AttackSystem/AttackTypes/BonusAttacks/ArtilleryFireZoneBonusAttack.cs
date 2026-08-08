using PoolSystem.Alternative;
using UnityEngine;

public sealed class ArtilleryFireZoneBonusAttack : AttackTypeBase
{
    [Header("Fire Zone")]
    public PersistentDamageZone FireZonePrefab;
    public int InitialPoolSize = 6;
    public float ZoneRadius = 3f;
    public float ZoneDuration = 3f;
    public float ZoneTickInterval = 0.5f;
    public float SpawnHeightOffset = 0f;
    public DamagesProContainer.DamageNumberPreset DamagePreset = DamagesProContainer.DamageNumberPreset.Flames;

    private IPool<PersistentDamageZone> _fireZonesPool;

    public override void PostInit()
    {
        if (FireZonePrefab == null)
        {
            return;
        }

        _fireZonesPool = PoolService.GetOrRegisterPool(FireZonePrefab, Mathf.Max(1, InitialPoolSize));
    }

    protected override bool ExecuteAttack()
    {
        if (Owner == null || _fireZonesPool == null)
        {
            return false;
        }

        var anchor = Target != null ? Target.position : Owner.position + Owner.forward * 2f;
        var spawnPosition = new Vector3(anchor.x, anchor.y + SpawnHeightOffset, anchor.z);
        var zone = _fireZonesPool.GetFreeElement(spawnPosition, Quaternion.identity, false);
        if (zone == null)
        {
            return false;
        }

        LayerAssigner.AssignLayer(Owner.gameObject.layer, zone.gameObject);
        zone.Configure(
            Owner.gameObject.layer,
            ZoneRadius,
            ZoneDuration,
            ZoneTickInterval,
            Mathf.RoundToInt(Damage),
            DamagePreset);
        zone.gameObject.SetActive(true);
        return true;
    }
}
