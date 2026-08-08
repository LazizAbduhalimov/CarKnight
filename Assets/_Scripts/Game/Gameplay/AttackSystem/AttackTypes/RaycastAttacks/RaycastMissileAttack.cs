using System.Collections;
using System.Collections.Generic;
using Modules.Coroutines;
using UnityEngine;

public class RaycastMissileAttack : AttackTypeBase
{
    public LineRenderer RayLine;
    public float RayDistance = 50f;
    public float RayRadius = 0.5f;
    public LayerMask RayMask = ~0;
    public float RayLifetime = 0.3f;

    private LineRenderer _runtimeLine;

    public override void PostInit()
    {
        RayMask = LayerAssigner.ResolveTargetUnitMask(Owner != null ? Owner.gameObject.layer : -1, RayMask);

        if (RayLine != null)
        {
            _runtimeLine = Object.Instantiate(RayLine);
            _runtimeLine.positionCount = 0;
        }
    }

    protected override bool ExecuteAttack()
    {
        if (Owner == null || Target == null)
            return false;

        var direction = (Target.position - Owner.position).WithY(0f).normalized;
        var origin = Owner.position.AddY(1f);

        var hits = Physics.SphereCastAll(origin, RayRadius, direction, RayDistance, RayMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        var damagedTargets = new HashSet<HealthComponent>();

        foreach (var hit in hits)
        {
            var health = hit.collider.GetComponentInParent<HealthComponent>();
            if (health != null && damagedTargets.Add(health))
            {
                health.TakeDamage(Mathf.RoundToInt(Damage), DamagesProContainer.DamageNumberPreset.NeonText);
            }
        }

        if (_runtimeLine != null)
        {
            _runtimeLine.widthCurve = new AnimationCurve(new Keyframe(0, RayRadius), new Keyframe(1, RayRadius));
            _runtimeLine.positionCount = 2;
            _runtimeLine.SetPosition(0, origin);
            _runtimeLine.SetPosition(1, origin + direction * RayDistance);
            CoroutineRunner.Run(AttackRayEffectDisableAfterDelay(RayLifetime));
        }

        Debug.DrawRay(origin, direction * RayDistance, Color.red, 0.5f);
        return true;
    }

    private IEnumerator AttackRayEffectDisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (_runtimeLine != null)
        {
            _runtimeLine.positionCount = 0;
        }
    }

    public override void Dispose()
    {
        if (_runtimeLine != null)
        {
            Object.Destroy(_runtimeLine.gameObject);
            _runtimeLine = null;
        }
    }
}
