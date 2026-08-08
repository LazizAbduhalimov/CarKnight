using System;
using PoolSystem.Alternative;
using Sirenix.OdinInspector;
using UnityEngine;

public class MissileColisionProvider : MonoBehaviour
{
    [SerializeField, ReadOnly] private LayerMask _targetMask;
    private bool _explodeOnTriggerEnter = true;
    private float _explosionRadius;

    public event Action<MissileImpactData> Exploded;

    private void OnTriggerEnter(Collider other)
    {
        if (!_explodeOnTriggerEnter)
        {
            return;
        }

        Boom(other, true);
    }

    public void Boom(bool useMissileExplosionPool = false)
    {
        Boom(null, useMissileExplosionPool);
    }

    private void Boom(Collider directHit, bool useMissileExplosionPool)
    {
        ResolveTargetMask();
        var targetMask = _targetMask;
        var splashHits = Array.Empty<Collider>();

        if (_explosionRadius > 0f && targetMask.value != 0)
        {
            splashHits = Physics.OverlapSphere(
                transform.position,
                _explosionRadius,
                targetMask,
                QueryTriggerInteraction.Collide);
        }

        Exploded?.Invoke(new MissileImpactData(transform.position, directHit, splashHits, _explosionRadius, targetMask));
        SpawnImpactVfx(useMissileExplosionPool, _explosionRadius);
        gameObject.SetActive(false);
    }

    public void SetExplosionDiameter(float diameter)
    {
        _explosionRadius = Mathf.Max(0f, diameter * 0.5f);
    }

    public void SetExplodeOnTriggerEnter(bool enabled)
    {
        _explodeOnTriggerEnter = enabled;
    }

    private void SpawnImpactVfx(bool useMissileExplosionPool, float explosionRadius)
    {
        Debug.Log(useMissileExplosionPool + " " + name);
        if (!useMissileExplosionPool) return;

        var vfx = VFXPools.Instance.ExplosionForMissilesPool.GetRandomElement().GetFromPool(transform.position);
        vfx.transform.localScale = Vector3.one * (explosionRadius * 0.5f); // 0.5 because vfx 2 times bigger
    }

    private void ResolveTargetMask()
    {
        var ownerLayer = LayerAssigner.ResolveOwnerLayerFromMissileLayer(gameObject.layer);
        _targetMask = ownerLayer >= 0
            ? LayerAssigner.ResolveTargetUnitMask(ownerLayer, default)
            : 0;
    }
}
