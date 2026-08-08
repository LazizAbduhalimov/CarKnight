using UnityEngine;

public sealed class BonusRandomMissileAttack : MissileSpawnAttackBase<MortarMissile>
{
    [Header("Explosion Radius Random")]
    [Tooltip("Minimum explosion diameter per missile.")]
    public float MinTargetRadius = 0f;
    [Tooltip("Maximum explosion diameter per missile.")]
    public float MaxTargetRadius = 8f;

    [Header("Target Selection")]
    [Tooltip("Random strike area radius around owner.")]
    public float StrikeRadius = 10f;

    [Header("Mortar Settings")]
    public float ExplosionDiameter = 15f;
    public float SpawnHeight = 1f;
    public float MaxHeight = 10f;
    [Tooltip("Seconds until impact.")]
    public float MinTravelTime = 0.5f;
    public bool UseMissileExplosionPool = true;

    public override void Tick()
    {
        if (Owner == null)
        {
            return;
        }

        var interval = Mathf.Max(0.01f, CooldownDuration);
        PassedTime += Time.deltaTime;
        while (PassedTime >= interval)
        {
            PassedTime -= interval;
            Attack();
        }
    }

    protected override bool ExecuteAttack()
    {
        if (Owner == null || MissilesPool == null)
        {
            return false;
        }

        var strikeTargetPosition = ResolveRandomStrikePoint();
        var direction = (strikeTargetPosition - Owner.position).WithY(0f);
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Owner.forward;
        }

        var rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        var spawnPosition = Owner.position + Vector3.up * SpawnHeight;
        var missile = MissilesPool.GetFreeElement(spawnPosition, rotation, false);
        if (missile == null)
        {
            return false;
        }

        LayerAssigner.AssignLayer(Owner.gameObject.layer, missile.gameObject);
        missile.TargetPosition = strikeTargetPosition;
        missile.MaxHeight = MaxHeight;
        missile.Diameter = ResolveExplosionDiameter();
        missile.MinTravelTime = 0f;
        missile.TimeToImpact = Mathf.Max(0f, MinTravelTime);
        missile.UseMissileExplosionPool = UseMissileExplosionPool;
        missile.Init(MissileSpeed);
        missile.SetDamage(Damage);
        missile.gameObject.SetActive(true);
        return true;
    }

    private Vector3 ResolveRandomStrikePoint()
    {
        var maxRadius = Mathf.Max(0f, StrikeRadius);
        if (maxRadius <= 0.0001f)
        {
            return Owner.position;
        }

        var distance = Random.Range(0f, maxRadius);
        var angle = Random.Range(0f, Mathf.PI * 2f);
        var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
        return Owner.position + offset;
    }

    private float ResolveExplosionDiameter()
    {
        var min = Mathf.Max(0f, MinTargetRadius);
        var max = Mathf.Max(min, MaxTargetRadius);
        if (max - min <= 0.0001f)
        {
            return ExplosionDiameter;
        }

        return Random.Range(min, max);
    }
}
