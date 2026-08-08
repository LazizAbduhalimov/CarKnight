using UnityEngine;

public readonly struct MissileImpactData
{
    public readonly Vector3 Position;
    public readonly Collider DirectHit;
    public readonly Collider[] SplashHits;
    public readonly float ExplosionRadius;
    public readonly LayerMask TargetMask;

    public MissileImpactData(
        Vector3 position,
        Collider directHit,
        Collider[] splashHits,
        float explosionRadius,
        LayerMask targetMask)
    {
        Position = position;
        DirectHit = directHit;
        SplashHits = splashHits ?? System.Array.Empty<Collider>();
        ExplosionRadius = Mathf.Max(0f, explosionRadius);
        TargetMask = targetMask;
    }
}
