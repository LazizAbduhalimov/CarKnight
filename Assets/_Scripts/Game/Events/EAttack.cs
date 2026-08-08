using UnityEngine;

public struct EAttackEvent
{
    public Transform Attacker;
    public Transform Target;
    public IAttackRuntime AttackType;

    public EAttackEvent(Transform attacker, Transform target, IAttackRuntime attackType)
    {
        Attacker = attacker;
        Target = target;
        AttackType = attackType;
    }
}
