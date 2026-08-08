using UnityEngine;

public class MortarAttack : BurstMissileAttackBase<MortarMissile>
{
    [Header("Mortar Attack Settings")]
    public float ExplosionDiameter = 15f;
    public float RadiusIncreasePerShot = 0f;

    [Tooltip("Height above launch point where shell waits before drop.")]
    public float MaxHeight = 10f;

    [Tooltip("Delay in seconds before shell starts falling.")]
    public float MinTravelTime = 0.5f;

    private float _currentExplosionDiameter;

    public override void PostInit()
    {
        base.PostInit();
        // Debug.Log($"Post Initializing MissileSpawnAttackBase for {Owner.name}");
        _currentExplosionDiameter = ExplosionDiameter;
    }

    protected override void OnBurstStarted()
    {
        _currentExplosionDiameter = ExplosionDiameter;
    }

    protected override bool ExecuteAttack()
    {
        if (Target == null || Owner == null)
        {
            Debug.LogWarning("Target or Owner is null in MortarAttack");
            return false;
        }

        var direction = (Target.position - Owner.position).WithY(0f);
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Owner.forward;
        }

        var rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        var missile = MissilesPool.GetFreeElement(Owner.position.AddY(1f), rotation, false);
        LayerAssigner.AssignLayer(Owner.gameObject.layer, missile.gameObject);

        missile.TargetPosition = Target.position;
        missile.MaxHeight = MaxHeight;
        missile.Diameter = _currentExplosionDiameter;
        missile.MinTravelTime = MinTravelTime;
        missile.TimeToImpact = -1f;
        missile.UseMissileExplosionPool = true;

        missile.Init(MissileSpeed);
        missile.SetDamage(Damage);
        missile.gameObject.SetActive(true);

        _currentExplosionDiameter += RadiusIncreasePerShot * 2f;
        return true;
    }
}
