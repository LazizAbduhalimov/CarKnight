using System.Collections.Generic;
using UnityEngine;

public static class AttackDamageService
{
    public static int ApplyMissileImpactDamage(
        MissileImpactData impactData,
        int damage,
        DamagesProContainer.DamageNumberPreset damagePreset)
    {
        if (damage <= 0)
        {
            return 0;
        }

        var impactedTargets = new HashSet<HealthComponent>();

        TryCollectTarget(impactData.DirectHit, impactData.TargetMask, impactedTargets);

        var splashHits = impactData.SplashHits;
        for (var i = 0; i < splashHits.Length; i++)
        {
            TryCollectTarget(splashHits[i], impactData.TargetMask, impactedTargets);
        }

        if (impactedTargets.Count == 0)
        {
            return 0;
        }

        foreach (var health in impactedTargets)
        {
            health.TakeDamage(damage, damagePreset);
        }

        return impactedTargets.Count;
    }

    private static void TryCollectTarget(
        Collider collider,
        LayerMask targetMask,
        HashSet<HealthComponent> impactedTargets)
    {
        if (collider == null)
        {
            return;
        }

        var health = collider.GetComponentInParent<HealthComponent>();
        if (health == null)
        {
            return;
        }

        if (!IsLayerInMask(health.gameObject.layer, targetMask))
        {
            return;
        }

        impactedTargets.Add(health);
    }

    private static bool IsLayerInMask(int layer, LayerMask mask)
    {
        if (mask.value == 0)
        {
            return false;
        }

        return (mask.value & (1 << layer)) != 0;
    }
}
