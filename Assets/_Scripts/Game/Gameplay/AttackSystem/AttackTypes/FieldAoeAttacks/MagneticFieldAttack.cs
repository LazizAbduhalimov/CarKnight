using System.Collections.Generic;
using PoolSystem.Alternative;
using UnityEngine;

public class MagneticFieldAttack : AttackTypeBase, IHiddenInUI
{
    [Header("Magnetic Field Settings")]
    public float Radius = 6f;
    public FollowerInstant MagneticEffectPrefab;
    private LayerMask _targetMask;

    private readonly HashSet<HealthComponent> _damagedThisTick = new();

    public override void PostInit()
    {
        _targetMask = ResolveTargetMask();
        var effect = PoolService.GetOrRegisterPool(MagneticEffectPrefab, 5).GetFreeElement(Owner.position, Quaternion.identity, true);
        effect.SetTarget(Owner);
        effect.transform.localScale = 2f * Radius * Vector3.one;
    }

    public override void Tick()
    {
        if (Owner == null)
            return;
        
        var interval = Mathf.Max(0.01f, AttackRate);
        PassedTime += Time.deltaTime;

        while (PassedTime >= interval)
        {
            PassedTime -= interval;
            Attack();
        }
    }

    protected override bool ExecuteAttack()
    {
        if (Owner == null || Radius <= 0f)
            return false;

        var hits = Physics.OverlapSphere(Owner.position, Radius, _targetMask, QueryTriggerInteraction.Collide);
        if (hits.Length == 0)
            return false;

        _damagedThisTick.Clear();
        foreach (var hit in hits)
        {
            var health = hit.GetComponentInParent<HealthComponent>();
            if (health == null || !_damagedThisTick.Add(health))
            {
                continue;
            }

            health.TakeDamage((int)Damage, DamagesProContainer.DamageNumberPreset.Poison);
        }

        return true;
    }

    private int ResolveTargetMask()
    {
        return LayerAssigner.ResolveTargetUnitMask(Owner.gameObject.layer, _targetMask);
    }
}
