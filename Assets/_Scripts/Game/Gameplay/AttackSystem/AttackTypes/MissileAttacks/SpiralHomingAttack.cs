using UnityEngine;

public class SpiralHomingAttack : MissileSpawnAttackBase<SpiralHomingMissile>
{
    [Header("Spiral Homing Settings")]
    public float MinFlightTime = 0.8f;

    protected override bool ExecuteAttack()
    {
        if (Target == null || Owner == null)
        {
            Debug.LogWarning("Target or Owner is null in SpiralHomingAttack");
            return false;
        }

        var spawnPosition = Owner.position.AddY(1f);
        var spawnRotation = Owner.rotation;
        var missile = MissilesPool.GetFreeElement(spawnPosition, spawnRotation, false);

        missile.TargetPosition = Target.position;
        missile.MinFlightTime = MinFlightTime;
        missile.Init(MissileSpeed);
        missile.SetDamage(Damage);

        LayerAssigner.AssignLayer(Owner.gameObject.layer, missile.gameObject);
        missile.gameObject.SetActive(true);
        return true;
    }
}
