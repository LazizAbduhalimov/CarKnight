using System;
using UnityEngine;

[Serializable]
public sealed class OwnedAttack
{
    public AttackSO Config { get; private set; }
    // [SerializeReference]
    public IAttackRuntime RuntimeAttack { get; private set; }
    public int Level { get; private set; } = 1;

    public OwnedAttack(AttackSO config, IAttackRuntime runtimeAttack, int level = 1)
    {
        Config = config;
        RuntimeAttack = runtimeAttack;
        Level = level;
    }

    public void SetRuntime(IAttackRuntime runtimeAttack)
    {
        RuntimeAttack = runtimeAttack;
    }

    public void SetLevel(int level)
    {
        Level = Mathf.Max(1, level);
    }
}
