using UnityEngine;

public struct EAfterFullBurstAttackEvent
{
    public Transform Attacker;
    public Transform Target;
    public IAttackRuntime AttackType;

    public EAfterFullBurstAttackEvent(Transform attacker, Transform target, IAttackRuntime attackType)
    {
        Attacker = attacker;
        Target = target;
        AttackType = attackType;
    }
}
