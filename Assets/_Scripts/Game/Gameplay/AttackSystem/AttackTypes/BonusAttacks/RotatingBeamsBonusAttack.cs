using System.Collections.Generic;
using UnityEngine;

public sealed class RotatingBeamsBonusAttack : AttackTypeBase
{
    [Header("Rotating Beams")]
    public int BeamCount = 5;
    public float RotationSpeed = 120f;
    public float Range = 18f;
    public float BeamRadius = 0.35f;
    public float HitHeight = 5f;
    public float MuzzleHeight = 1f;
    public float BurstDuration = 2f;
    public float RaycastInterval = 0.2f;
    public bool DrawDebugRays;

    [Header("Beam Visual")]
    public Transform BeamPrefab;
    public DamagesProContainer.DamageNumberPreset DamagePreset = DamagesProContainer.DamageNumberPreset.NeonText;

    private readonly HashSet<HealthComponent> _uniqueTargetsPerBeam = new();
    private readonly List<Transform> _runtimeBeams = new();

    private float _currentAngle;
    private float _burstElapsed;
    private float _raycastElapsed;
    private bool _isBurstActive;

    public override void PostInit()
    {
        CreateBeamVisuals();
        SetBeamsVisible(false);
    }

    public override void Tick()
    {
        if (Owner == null)
        {
            return;
        }

        if (_isBurstActive)
        {
            TickActiveBurst();
            return;
        }

        PassedTime += Time.deltaTime;
        if (PassedTime >= CooldownDuration)
        {
            StartBurst();
        }
    }

    protected override bool ExecuteAttack()
    {
        if (!_isBurstActive || Owner == null || EnemiesNearby == null)
        {
            return false;
        }

        var beamsCount = Mathf.Max(1, BeamCount);
        var origin = Owner.position + Vector3.up * MuzzleHeight;
        for (var i = 0; i < beamsCount; i++)
        {
            var direction = ResolveBeamDirection(i, beamsCount);
            RegisterHits(origin, direction);

            if (DrawDebugRays)
            {
                Debug.DrawRay(origin, direction * Mathf.Max(0.01f, Range), Color.cyan, Mathf.Max(0.01f, RaycastInterval));
            }
        }

        return true;
    }

    public override void Dispose()
    {
        _isBurstActive = false;
        _burstElapsed = 0f;
        _raycastElapsed = 0f;

        for (var i = 0; i < _runtimeBeams.Count; i++)
        {
            if (_runtimeBeams[i] != null)
            {
                Object.Destroy(_runtimeBeams[i].gameObject);
            }
        }

        _runtimeBeams.Clear();
    }

    private void RegisterHits(Vector3 origin, Vector3 direction)
    {
        var castDamage = Mathf.RoundToInt(Damage);
        if (castDamage <= 0)
        {
            return;
        }

        var castDistance = Mathf.Max(0.01f, Range);
        var castRadius = Mathf.Max(0f, BeamRadius);
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
            EnemiesNearby.LayerMask,
            QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
        {
            return;
        }

        _uniqueTargetsPerBeam.Clear();
        for (var i = 0; i < hits.Length; i++)
        {
            var health = hits[i].collider.GetComponentInParent<HealthComponent>();
            if (health == null || !_uniqueTargetsPerBeam.Add(health))
            {
                continue;
            }

            health.TakeDamage(castDamage, DamagePreset);
        }
    }

    private Vector3 ResolveBeamDirection(int beamIndex, int beamsCount)
    {
        var beamAngle = _currentAngle + beamIndex * (360f / Mathf.Max(1, beamsCount));
        var rotation = Quaternion.AngleAxis(beamAngle, Vector3.up);
        return rotation * Vector3.forward;
    }

    private void CreateBeamVisuals()
    {
        _runtimeBeams.Clear();
        if (BeamPrefab == null || Owner == null)
        {
            return;
        }

        var beamsCount = Mathf.Max(1, BeamCount);
        for (var i = 0; i < beamsCount; i++)
        {
            var beam = Object.Instantiate(BeamPrefab, Owner);
            if (beam == null)
            {
                continue;
            }

            _runtimeBeams.Add(beam);
        }
    }

    private void UpdateBeamVisuals()
    {
        if (_runtimeBeams.Count == 0 || Owner == null)
        {
            return;
        }

        var beamsCount = Mathf.Max(1, BeamCount);
        var origin = Owner.position + Vector3.up * MuzzleHeight;
        var beamWidth = Mathf.Max(0f, BeamRadius) * 2f;
        var beamLength = Mathf.Max(0.01f, Range);

        for (var i = 0; i < _runtimeBeams.Count; i++)
        {
            var beam = _runtimeBeams[i];
            if (beam == null)
            {
                continue;
            }

            var direction = ResolveBeamDirection(i, beamsCount);
            beam.position = origin;
            beam.rotation = Quaternion.LookRotation(direction, Vector3.up);
            beam.localScale = new Vector3(beamWidth, beamWidth, beamLength);
        }
    }

    private void TickActiveBurst()
    {
        _burstElapsed += Time.deltaTime;
        _raycastElapsed += Time.deltaTime;
        _currentAngle += RotationSpeed * Time.deltaTime;
        UpdateBeamVisuals();

        var interval = Mathf.Max(0.01f, RaycastInterval);
        while (_raycastElapsed >= interval)
        {
            _raycastElapsed -= interval;
            Attack();
        }

        if (_burstElapsed >= Mathf.Max(0.01f, BurstDuration))
        {
            StopBurst();
        }
    }

    private void StartBurst()
    {
        if (_isBurstActive || Owner == null)
        {
            return;
        }

        PassedTime = 0f;
        _isBurstActive = true;
        _burstElapsed = 0f;
        _raycastElapsed = 0f;
        SetBeamsVisible(true);
        Attack();
    }

    private void StopBurst()
    {
        PassedTime = 0f;
        _isBurstActive = false;
        _burstElapsed = 0f;
        _raycastElapsed = 0f;
        SetBeamsVisible(false);
    }

    private void SetBeamsVisible(bool visible)
    {
        for (var i = 0; i < _runtimeBeams.Count; i++)
        {
            var beam = _runtimeBeams[i];
            if (beam != null && beam.gameObject.activeSelf != visible)
            {
                beam.gameObject.SetActive(visible);
            }
        }
    }
}
