using UnityEngine;

public class PatternedMissileAttack : MissileSpawnAttackBase<PatternedTrailMissile>
{
    public float Amplitude = 1f;
    public float NoiseScale = 2f;
    public float NoiseSpeed = 3f;

    protected override bool ExecuteAttack()
    {
        if (Target == null || Owner == null)
        {
            return false;
        }

        var direction = Target.position - Owner.position;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Owner.forward;
        }

        var normalizedDirection = direction.normalized;
        var up = Mathf.Abs(Vector3.Dot(normalizedDirection, Vector3.up)) > 0.999f ? Vector3.forward : Vector3.up;
        var rotation = Quaternion.LookRotation(normalizedDirection, up);
        var missile = MissilesPool.GetFreeElement(Owner.position.AddY(1f), rotation, false);
        missile.Init(MissileSpeed);
        missile.SetDamage(Damage);
        missile.Amplitude = Amplitude;
        missile.NoiseScale = NoiseScale;
        missile.NoiseSpeed = NoiseSpeed;
        LayerAssigner.AssignLayer(Owner.gameObject.layer, missile.gameObject);
        missile.gameObject.SetActive(true);
        return true;
    }
}
