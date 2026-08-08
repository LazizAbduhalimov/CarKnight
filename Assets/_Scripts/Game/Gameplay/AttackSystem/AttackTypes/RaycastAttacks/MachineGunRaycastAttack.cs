using PoolSystem.Alternative;
using UnityEngine;

public sealed class MachineGunRaycastAttack : AttackTypeBase
{
    [Header("Machine Gun")]
    public float Range = 30f;
    public float BulletRadius = 0.05f;
    public float SpreadRadius = 0.25f;
    [Range(0f, 180f)] public float ForwardConeHalfAngle = 45f;
    public float MuzzleHeight = 1f;
    public LayerMask HitMask = ~0;
    public bool DrawDebugRays;

    [Header("Tracer VFX")]
    public float TracerTravelDuration = 0.1f;
    public float TracerLifetime = 0.08f;

    [Header("Damage")]
    public DamagesProContainer.DamageNumberPreset DamagePreset = DamagesProContainer.DamageNumberPreset.NeonText;

    [Header("Fallbacks")]
    public bool UseDefaultImpactIfMissing = true;

    private LayerMask _resolvedHitMask;

    public override void PostInit()
    {
        _resolvedHitMask = LayerAssigner.ResolveTargetUnitMask(Owner != null ? Owner.gameObject.layer : -1, HitMask);
    }

    protected override bool ExecuteAttack()
    {
        if (Owner == null)
        {
            return false;
        }

        var targetInCone = FindNearestTargetInCone();
        if (targetInCone == null)
        {
            return false;
        }

        var origin = Owner.position + Vector3.up * MuzzleHeight;
        var direction = BuildShotDirection(origin, targetInCone.position);
        if (direction.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        SpawnMuzzleVfx(origin, direction);

        var hasHit = Physics.SphereCast(
            origin,
            Mathf.Max(0f, BulletRadius),
            direction,
            out var hit,
            Mathf.Max(0.01f, Range),
            _resolvedHitMask,
            QueryTriggerInteraction.Ignore);

        if (hasHit)
        {
            SpawnTracerVfx(origin, hit.point);

            var health = hit.collider.GetComponentInParent<HealthComponent>();
            if (health != null)
            {
                health.TakeDamage(Mathf.RoundToInt(Damage), DamagePreset);
            }

            SpawnImpactVfx(hit.point, hit.normal);
        }

        if (DrawDebugRays)
        {
            var endPoint = hasHit ? hit.point : origin + direction * Mathf.Max(0.01f, Range);
            Debug.DrawLine(origin, endPoint, hasHit ? Color.red : Color.yellow, 0.25f);
        }

        return true;
    }

    private Transform FindNearestTargetInCone()
    {
        var enemies = EnemiesNearby?.Enemies;
        if (enemies == null || enemies.Count == 0)
        {
            return IsTargetValidForCone(Target) ? Target : null;
        }

        Transform nearest = null;
        var ownerPosition = Owner.position;
        var maxRangeSqr = Mathf.Max(0.01f, Range);
        maxRangeSqr *= maxRangeSqr;
        var bestSqrDistance = float.MaxValue;

        for (var i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            if (!IsTargetValidForCone(enemy))
            {
                continue;
            }

            var toEnemy = enemy.position - ownerPosition;
            toEnemy.y = 0f;
            var sqrDistance = toEnemy.sqrMagnitude;
            if (sqrDistance > maxRangeSqr || sqrDistance >= bestSqrDistance)
            {
                continue;
            }

            bestSqrDistance = sqrDistance;
            nearest = enemy;
        }

        return nearest;
    }

    private bool IsTargetValidForCone(Transform candidate)
    {
        if (candidate == null || !candidate.gameObject.activeInHierarchy || Owner == null)
        {
            return false;
        }

        var toTarget = candidate.position - Owner.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f)
        {
            return true;
        }

        var forward = Owner.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        var angle = Vector3.Angle(forward.normalized, toTarget.normalized);
        return angle <= Mathf.Max(0f, ForwardConeHalfAngle);
    }

    private Vector3 BuildShotDirection(Vector3 origin, Vector3 baseAimPoint)
    {
        var direction = (baseAimPoint - origin).normalized;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Owner != null ? Owner.forward : Vector3.forward;
        }

        direction = ApplySpread(direction.normalized);
        return ClampDirectionToForwardCone(direction);
    }

    private Vector3 ApplySpread(Vector3 baseDirection)
    {
        var spreadRadius = Mathf.Max(0f, SpreadRadius);
        if (spreadRadius < 0.0001f)
        {
            return baseDirection.normalized;
        }

        var right = Vector3.Cross(Vector3.up, baseDirection);
        if (right.sqrMagnitude < 0.0001f)
        {
            right = Owner != null ? Owner.right : Vector3.right;
        }
        right.Normalize();

        var up = Vector3.Cross(baseDirection, right);
        if (up.sqrMagnitude < 0.0001f)
        {
            up = Vector3.up;
        }
        else
        {
            up.Normalize();
        }

        var randomOffset = Random.insideUnitCircle * spreadRadius;
        var spreadTarget = baseDirection * Mathf.Max(0.01f, Range) + right * randomOffset.x + up * randomOffset.y;
        return spreadTarget.sqrMagnitude < 0.0001f ? baseDirection.normalized : spreadTarget.normalized;
    }

    private Vector3 ClampDirectionToForwardCone(Vector3 direction)
    {
        if (Owner == null)
        {
            return direction.normalized;
        }

        var flatForward = Owner.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.0001f)
        {
            flatForward = Vector3.forward;
        }

        var flatDirection = direction;
        flatDirection.y = 0f;
        if (flatDirection.sqrMagnitude < 0.0001f)
        {
            return flatForward.normalized;
        }

        var signedAngle = Vector3.SignedAngle(flatForward.normalized, flatDirection.normalized, Vector3.up);
        var clampedAngle = Mathf.Clamp(signedAngle, -ForwardConeHalfAngle, ForwardConeHalfAngle);
        var clampedFlatDirection = Quaternion.AngleAxis(clampedAngle, Vector3.up) * flatForward.normalized;
        var result = new Vector3(clampedFlatDirection.x, direction.y, clampedFlatDirection.z);

        return result.sqrMagnitude < 0.0001f ? clampedFlatDirection.normalized : result.normalized;
    }

    private void SpawnMuzzleVfx(Vector3 origin, Vector3 direction)
    {
        var pool = VFXPools.Instance != null ? VFXPools.Instance.MachineGunMuzzleFlashPool : null;
        if (pool == null || Owner == null)
        {
            return;
        }

        var effect = pool.GetFromPool(origin);
        if (effect == null || direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        effect.transform.SetParent(Owner, false);
        effect.transform.localPosition = new Vector3(0f, MuzzleHeight, 0f);
        effect.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private void SpawnTracerVfx(Vector3 start, Vector3 end)
    {
        var pool = VFXPools.Instance != null ? VFXPools.Instance.MachineGunTracerPool : null;
        if (pool == null)
        {
            return;
        }

        var effect = pool.GetFromPool(start);
        if (effect == null)
        {
            return;
        }

        if (!effect.TryGetComponent<MachineGunTracerVfx>(out var tracerVfx))
        {
            tracerVfx = effect.gameObject.AddComponent<MachineGunTracerVfx>();
        }

        tracerVfx.Play(start, end, TracerTravelDuration, TracerLifetime);
    }

    private void SpawnImpactVfx(Vector3 position, Vector3 normal)
    {
        if (VFXPools.Instance == null)
        {
            return;
        }

        PoolContainer impactPool = VFXPools.Instance.MachineGunImpactPool;
        if (impactPool == null && UseDefaultImpactIfMissing)
        {
            impactPool = VFXPools.Instance.FireImpactPool;
        }

        if (impactPool == null)
        {
            return;
        }

        var effect = impactPool.GetFromPool(position);
        if (effect == null)
        {
            return;
        }

        var lookDirection = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
        effect.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }
}
