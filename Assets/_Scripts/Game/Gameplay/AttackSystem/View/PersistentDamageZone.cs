using System.Collections.Generic;
using UnityEngine;

public sealed class PersistentDamageZone : MonoBehaviour
{
    [SerializeField] private float _radius = 3f;
    [SerializeField] private float _duration = 3f;
    [SerializeField] private float _tickInterval = 0.5f;
    [SerializeField] private LayerMask _targetMask = ~0;
    [SerializeField] private DamagesProContainer.DamageNumberPreset _damagePreset = DamagesProContainer.DamageNumberPreset.Flames;
    [SerializeField] private SphereCollider _triggerCollider;

    private readonly HashSet<HealthComponent> _uniqueTargets = new();
    private float _elapsed;
    private float _tickElapsed;
    private int _damagePerTick;

    public void Configure(
        int ownerLayer,
        float radius,
        float duration,
        float tickInterval,
        int damagePerTick,
        DamagesProContainer.DamageNumberPreset damagePreset)
    {
        _targetMask = LayerAssigner.ResolveTargetUnitMask(ownerLayer, _targetMask);
        _radius = Mathf.Max(0.01f, radius);
        _duration = Mathf.Max(0.01f, duration);
        _tickInterval = Mathf.Max(0.01f, tickInterval);
        _damagePerTick = Mathf.Max(0, damagePerTick);
        _damagePreset = damagePreset;

        if (_triggerCollider != null)
        {
            _triggerCollider.isTrigger = true;
            _triggerCollider.radius = _radius;
        }

        var currentScale = transform.localScale;
        transform.localScale = new Vector3(_radius, currentScale.y, _radius);
    }

    private void OnEnable()
    {
        _elapsed = 0f;
        _tickElapsed = 0f;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        _tickElapsed += Time.deltaTime;

        if (_tickElapsed >= _tickInterval)
        {
            _tickElapsed -= _tickInterval;
            ApplyTickDamage();
        }

        if (_elapsed >= _duration)
        {
            gameObject.SetActive(false);
        }
    }

    private void ApplyTickDamage()
    {
        if (_damagePerTick <= 0 || _targetMask.value == 0)
        {
            return;
        }

        var hits = Physics.OverlapSphere(transform.position, _radius, _targetMask, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            return;
        }

        _uniqueTargets.Clear();
        for (var i = 0; i < hits.Length; i++)
        {
            var health = hits[i].GetComponentInParent<HealthComponent>();
            if (health == null || !_uniqueTargets.Add(health))
            {
                continue;
            }

            health.TakeDamage(_damagePerTick, _damagePreset);
        }
    }
}
