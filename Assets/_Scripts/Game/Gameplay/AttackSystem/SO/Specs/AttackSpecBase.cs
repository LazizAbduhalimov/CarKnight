using UnityEngine;

public enum AttackSpecKind
{
    Missile = 0,
    Raycast = 1,
    FieldAoe = 2
}

public interface IAttackStatDefaults
{
    float DefaultAttackRateValue { get; }
    float DefaultDamageValue { get; }
}

public abstract class AttackSpecBase : ScriptableObject
{
    public abstract AttackSpecKind Kind { get; }
}
