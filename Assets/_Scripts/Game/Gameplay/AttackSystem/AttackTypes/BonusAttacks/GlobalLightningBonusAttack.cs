using System.Collections.Generic;
using PoolSystem.Alternative;
using UnityEngine;

public sealed class GlobalLightningBonusAttack : AttackTypeBase
{
    public enum LightningImpactColor
    {
        Yellow = 1,
        Blue = 2,
        Red = 3
    }

    public float GlobalRadius = 250f;
    public LightningImpactColor ImpactColor = LightningImpactColor.Red;

    private readonly List<HealthComponent> _candidates = new();
    private readonly HashSet<int> _visitedTargetIds = new();

    protected override bool ExecuteAttack()
    {
        if (Owner == null || EnemiesNearby == null)
        {
            Debug.LogWarning("GlobalLightningBonusAttack: Owner or EnemiesNearby is null");
            return false;
        }

        CollectTargets();
        if (_candidates.Count == 0)
        {
            return false;
        }
        Debug.Log($"GlobalLightningBonusAttack: Striking {_candidates.Count} targets");
        for (var i = 0; i < _candidates.Count; i++)
        {
            StrikeTarget(_candidates[i]);
        }

        return true;
    }

    public override void Tick()
    {
        PassedTime += Time.deltaTime;
        if (PassedTime < CooldownDuration)
        {
            return;
        }

        Attack();
        if (_candidates.Count == 0)
        {
            return;
        }
        PassedTime = 0f;
    }

    private void CollectTargets()
    {
        _candidates.Clear();
        _visitedTargetIds.Clear();
        var radius = Mathf.Max(0.01f, GlobalRadius);
        var hits = Physics.OverlapSphere(Owner.position, radius, EnemiesNearby.LayerMask);
        Debug.Log($"GlobalLightningBonusAttack: Found {hits.Length} colliders in radius {radius}");
        if (hits == null || hits.Length == 0)
        {
            return;
        }

        for (var i = 0; i < hits.Length; i++)
        {
            var health = hits[i].GetComponentInParent<HealthComponent>();
            if (health == null)
            {
                continue;
            }

            var targetId = health.GetInstanceID();
            if (!_visitedTargetIds.Add(targetId))
            {
                continue;
            }

            _candidates.Add(health);
        }
    }

    private void StrikeTarget(HealthComponent health)
    {
        if (health == null)
        {
            return;
        }

        SpawnZapImpact(health.transform.position);
        health.TakeDamage(Mathf.RoundToInt(Damage), ResolveDamagePresetByLightningColor());
    }

    private void SpawnZapImpact(Vector3 impactPoint)
    {
        var pools = VFXPools.Instance;
        if (pools == null)
        {
            return;
        }

        var pool = GetImpactPool(pools);
        if (pool == null)
        {
            return;
        }

        pool.GetFromPool(impactPoint);
    }

    private PoolContainer GetImpactPool(VFXPools pools)
    {
        return ImpactColor switch
        {
            LightningImpactColor.Blue => pools.ZapBluePool,
            LightningImpactColor.Red => pools.ZapRedPool,
            _ => pools.ZapYellowPool
        };
    }

    private DamagesProContainer.DamageNumberPreset ResolveDamagePresetByLightningColor()
    {
        return ImpactColor switch
        {
            LightningImpactColor.Blue => DamagesProContainer.DamageNumberPreset.Mana,
            LightningImpactColor.Red => DamagesProContainer.DamageNumberPreset.RedGlow,
            _ => DamagesProContainer.DamageNumberPreset.Gold
        };
    }

}
