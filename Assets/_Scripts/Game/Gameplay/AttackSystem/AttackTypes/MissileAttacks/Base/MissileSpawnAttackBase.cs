using PoolSystem.Alternative;
using UnityEngine;

public class MissileSpawnAttackBase<T> : AttackTypeBase where T : MissileBase
{
    [Header("Missile Base Settings")]
    public T MissilePrefab;
    public float MissileSpeed = 15f;
    protected IPool<T> MissilesPool;

    public override void PostInit()
    {
        MissilesPool = PoolService.GetOrRegisterPool(MissilePrefab, 10);
    }

    protected override bool ExecuteAttack()
    {
        if (Target == null || Owner == null)
        {
            Debug.LogWarning("Target or Owner is null in Attack()");
            return false;
        }

        var direction = (Target.position - Owner.position).WithY(0f);
        var rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Debug.DrawRay(Owner.position, direction.normalized * 5f, Color.yellow, 2f);
        var missile = MissilesPool.GetFreeElement(Owner.position.AddY(1f), rotation, false);
        LayerAssigner.AssignLayer(Owner.gameObject.layer, missile.gameObject);
        missile.Init(MissileSpeed);
        missile.SetDamage(Damage);
        missile.gameObject.SetActive(true);
        return true;
    }
}
