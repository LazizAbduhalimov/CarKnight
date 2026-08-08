using System.Collections;
using System.Collections.Generic;
using Modules.Coroutines;
using UnityEngine;

public sealed class ForwardSphereBurstRaycastAttack : AttackTypeBase
{
    [Header("Forward Sphere Burst")]
    public float Range = 30f;
    public float Radius = 0.5f;
    public float HitHeight = 8f;
    public float BurstDuration = 2f;
    public int SphereCastCount = 10;
    public float MuzzleHeight = 1f;
    public bool FlattenDirectionToGround = true;
    public LayerMask HitMask = ~0;
    public bool DrawDebugRays;

    [Header("Beam Visual")]
    public Transform BeamPrefab;

    [Header("Damage")]
    public DamagesProContainer.DamageNumberPreset DamagePreset = DamagesProContainer.DamageNumberPreset.NeonText;

    private readonly HashSet<HealthComponent> _uniqueTargetsPerCast = new();

    private LayerMask _resolvedHitMask;
    private bool _isBurstActive;
    private int _burstVersion;
    private Transform _runtimeBeam;

    public override void PostInit()
    {
        _resolvedHitMask = LayerAssigner.ResolveTargetUnitMask(Owner != null ? Owner.gameObject.layer : -1, HitMask);

        if (BeamPrefab != null)
        {
            _runtimeBeam = Owner != null
                ? Object.Instantiate(BeamPrefab, Owner)
                : Object.Instantiate(BeamPrefab);
            _runtimeBeam.gameObject.SetActive(false);
        }
    }

    public override void Tick()
    {
        if (Owner == null)
        {
            return;
        }

        PassedTime += Time.deltaTime;
        while (PassedTime >= CooldownDuration)
        {
            PassedTime -= CooldownDuration;
            Attack();
        }
    }

    protected override bool ExecuteAttack()
    {
        if (Owner == null || _isBurstActive)
        {
            return false;
        }

        _burstVersion++;
        CoroutineRunner.Run(BurstRoutine(_burstVersion));
        return false;
    }

    public override void Dispose()
    {
        _burstVersion++;
        _isBurstActive = false;
        _uniqueTargetsPerCast.Clear();

        if (_runtimeBeam != null)
        {
            Object.Destroy(_runtimeBeam.gameObject);
            _runtimeBeam = null;
        }
    }

    private IEnumerator BurstRoutine(int burstVersion)
    {
        _isBurstActive = true;

        var castsTotal = Mathf.Max(1, SphereCastCount);
        var castInterval = BurstDuration > 0f && castsTotal > 1 ? BurstDuration / (castsTotal - 1) : 0f;
        var castDistance = Mathf.Max(0.01f, Range);
        var castRadius = Mathf.Max(0f, Radius);
        var damagePerCast = Damage / castsTotal;
        ShowBeamVisual(castRadius);

        for (var castIndex = 0; castIndex < castsTotal; castIndex++)
        {
            if (burstVersion != _burstVersion || Owner == null)
            {
                break;
            }

            var origin = Owner.position + Vector3.up * MuzzleHeight;
            var direction = ResolveForwardDirection();
            if (direction.sqrMagnitude < 0.0001f)
            {
                continue;
            }

            UpdateBeamRotation(direction);
            RegisterHits(origin, direction, castRadius, castDistance, damagePerCast);
            PlayShotSound(origin);

            if (DrawDebugRays)
            {
                Debug.DrawRay(origin, direction * castDistance, Color.cyan, castInterval > 0f ? castInterval : 0.1f);
            }

            if (castIndex < castsTotal - 1 && castInterval > 0f)
            {
                yield return new WaitForSeconds(castInterval);
            }
        }

        if (burstVersion == _burstVersion)
        {
            _isBurstActive = false;
        }

        SetBeamVisible(false);
    }

    private Vector3 ResolveForwardDirection()
    {
        if (Owner == null)
        {
            return Vector3.zero;
        }

        var direction = Owner.forward;
        if (FlattenDirectionToGround)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector3.forward;
        }

        return direction.normalized;
    }

    private void RegisterHits(Vector3 origin, Vector3 direction, float castRadius, float castDistance, float damagePerCast)
    {
        var castDamage = Mathf.RoundToInt(damagePerCast);
        if (castDamage <= 0)
        {
            return;
        }

        var effectiveHitHeight = Mathf.Max(castRadius * 2f, HitHeight);
        var verticalOffset = Mathf.Max(0f, effectiveHitHeight * 0.5f - castRadius);
        var capsuleStart = origin + Vector3.up * verticalOffset;
        var capsuleEnd = origin - Vector3.up * verticalOffset;
        var hits = Physics.CapsuleCastAll(
            capsuleStart,
            capsuleEnd,
            castRadius,
            direction,
            castDistance,
            _resolvedHitMask,
            QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
        {
            return;
        }

        _uniqueTargetsPerCast.Clear();
        for (var i = 0; i < hits.Length; i++)
        {
            var health = hits[i].collider.GetComponentInParent<HealthComponent>();
            if (health == null || !_uniqueTargetsPerCast.Add(health))
            {
                continue;
            }

            health.TakeDamage(castDamage, DamagePreset);
        }
    }

    private void ShowBeamVisual(float castRadius)
    {
        if (_runtimeBeam == null || Owner == null)
        {
            return;
        }

        if (_runtimeBeam.parent != Owner)
        {
            _runtimeBeam.SetParent(Owner, false);
        }

        SetBeamVisible(true);

        var beamWidth = castRadius * 2f;
        var beamLength = Mathf.Max(0.01f, Range);
        var ownerScale = Owner.lossyScale;
        var inverseScaleX = 1f / Mathf.Max(0.0001f, Mathf.Abs(ownerScale.x));
        var inverseScaleY = 1f / Mathf.Max(0.0001f, Mathf.Abs(ownerScale.y));
        var inverseScaleZ = 1f / Mathf.Max(0.0001f, Mathf.Abs(ownerScale.z));
        _runtimeBeam.localPosition = Vector3.zero;
        _runtimeBeam.localScale = new Vector3(
            beamWidth * inverseScaleX,
            beamWidth * inverseScaleY,
            beamLength * inverseScaleZ);
        UpdateBeamRotation(ResolveForwardDirection());
    }

    private void UpdateBeamRotation(Vector3 forwardDirection)
    {
        if (_runtimeBeam == null || Owner == null)
        {
            return;
        }

        if (forwardDirection.sqrMagnitude < 0.0001f)
        {
            forwardDirection = Vector3.forward;
        }

        var worldRotation = Quaternion.LookRotation(forwardDirection.normalized, Vector3.up);
        _runtimeBeam.localRotation = Quaternion.Inverse(Owner.rotation) * worldRotation;
    }

    private void SetBeamVisible(bool visible)
    {
        if (_runtimeBeam != null && _runtimeBeam.gameObject.activeSelf != visible)
        {
            _runtimeBeam.gameObject.SetActive(visible);
        }
    }
}
