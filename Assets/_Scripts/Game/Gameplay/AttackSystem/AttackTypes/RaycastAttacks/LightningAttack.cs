using System.Collections;
using System.Collections.Generic;
using System.Text;
using Modules.Coroutines;
using PoolSystem.Alternative;
using UnityEngine;

public class LightningAttack : AttackTypeBase
{
    public float ChainRadius = 10f;
    public float ChainInterval = 0.2f;
    public int MaxBounces = 3;
    public float ImpactRadius = 2f;
    public LayerMask EnemyMask;
    public bool LogHitZones = true;

    public LightningImpactColor ImpactColor = LightningImpactColor.Yellow;

    public enum LightningImpactColor
    {
        Yellow = 1,
        Blue = 2,
        Red = 3
    }

    public override void PostInit()
    {
        EnemyMask = LayerAssigner.ResolveTargetUnitMask(Owner != null ? Owner.gameObject.layer : -1, EnemyMask);
    }

    protected override bool ExecuteAttack()
    {
        if (Owner == null || Target == null)
            return false;

        CoroutineRunner.Run(LightningChainRoutine(Target));
        return true;
    }

    private IEnumerator LightningChainRoutine(Transform firstTarget)
    {
        var currentTarget = firstTarget;
        var hitTargetIds = new HashSet<int>();

        for (int i = 0; i < MaxBounces && currentTarget != null; i++)
        {
            var impactPoint = currentTarget.position;
            SpawnZapImpact(impactPoint);
            ApplyAreaDamage(impactPoint, i + 1, hitTargetIds);

            var currentHealth = currentTarget.GetComponentInParent<HealthComponent>();
            if (currentHealth != null)
            {
                hitTargetIds.Add(currentHealth.GetInstanceID());
            }

            var nextTarget = FindNextTarget(currentTarget, hitTargetIds);

            if (nextTarget == null)
            {
                yield break;
            }

            yield return new WaitForSeconds(ChainInterval);
            currentTarget = nextTarget;
        }
    }

    private Transform FindNextTarget(Transform fromTarget, HashSet<int> alreadyHitIds)
    {
        var hits = Physics.OverlapSphere(fromTarget.position, ChainRadius, EnemyMask);
        Transform bestTargetTransform = null;
        float bestSqrDist = float.MaxValue;

        foreach (var col in hits)
        {
            var health = col.GetComponentInParent<HealthComponent>();
            if (health == null)
            {
                continue;
            }

            var targetId = health.GetInstanceID();
            if (alreadyHitIds.Contains(targetId))
            {
                continue;
            }

            var t = health.transform;

            var sqrDist = (t.position - fromTarget.position).sqrMagnitude;
            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;
                bestTargetTransform = t;
            }
        }

        return bestTargetTransform;
    }

    private void ApplyAreaDamage(Vector3 impactPoint, int bounceIndex, HashSet<int> alreadyHitInChain)
    {
        var colliders = Physics.OverlapSphere(impactPoint, ImpactRadius, EnemyMask, QueryTriggerInteraction.Ignore);
        if (colliders == null || colliders.Length == 0)
        {
            if (LogHitZones)
            {
                Debug.Log($"[LightningAttack] Bounce {bounceIndex}: no targets in impact radius.");
            }
            return;
        }

        var uniqueTargetsInZone = new HashSet<int>();
        var logBuilder = LogHitZones ? new StringBuilder() : null;
        if (logBuilder != null)
        {
            logBuilder.Append($"[LightningAttack] Bounce {bounceIndex} targets in zone:");
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            var health = colliders[i].GetComponentInParent<HealthComponent>();
            if (health == null)
            {
                continue;
            }

            var targetId = health.GetInstanceID();
            if (!uniqueTargetsInZone.Add(targetId))
            {
                continue;
            }

            if (alreadyHitInChain.Contains(targetId))
            {
                if (logBuilder != null)
                {
                    logBuilder.Append($"\n- {health.name}: skipped (already hit in chain)");
                }
                continue;
            }

            health.TakeDamage(Mathf.RoundToInt(Damage), ResolveDamagePresetByLightningColor());
            alreadyHitInChain.Add(targetId);

            if (logBuilder != null)
            {
                logBuilder.Append($"\n- {health.name}: damaged ({Mathf.RoundToInt(Damage)})");
            }
        }

        if (logBuilder != null)
        {
            Debug.Log(logBuilder.ToString());
        }
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
