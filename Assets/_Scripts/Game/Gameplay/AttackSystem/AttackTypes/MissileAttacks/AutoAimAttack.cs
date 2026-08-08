using UnityEngine;

public class AutoAimAttack : MissileSpawnAttackBase<AutoAimMissile>
{
    public float TurnRatePerSecond = 180f;

    protected override bool ExecuteAttack()
    {
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
        missile.Target = Target;
        missile.TurnRatePerSecond = TurnRatePerSecond;
        LayerAssigner.AssignLayer(Owner.gameObject.layer, missile.gameObject);
        missile.gameObject.SetActive(true);
        return true;
    }
}
