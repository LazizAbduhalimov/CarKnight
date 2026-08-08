using UnityEngine;

public class ArcAttack : BurstMissileAttackBase<ArcMissile>
{
    [Header("Arc Attack Settings")]
    public float ExplosionDiameter = 2f;
    public float RadiusIncreasePerShot = 0f;
    
    [Tooltip("Высота дуги в метрах")]
    public float ArcHeight = 5f;

    private float _currentExplosionDiameter;

    public override void PostInit()
    {
        base.PostInit();
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
            Debug.LogWarning("Target or Owner is null in ArcAttack");
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
        missile.ArcHeight = ArcHeight;
        missile.Diameter = _currentExplosionDiameter;

        missile.Init(MissileSpeed);
        missile.SetDamage(Damage);
        missile.gameObject.SetActive(true);

        _currentExplosionDiameter += RadiusIncreasePerShot * 2f;
        return true;
    }
}
