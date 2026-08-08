using OnlyFun.EventBus;
using PoolSystem.Alternative;
using UnityEngine;

public interface IAttackProgressionServiceFactory
{
    IAttackProgressionService Create(
        Transform owner,
        string ownerName,
        PoolService poolService,
        EventBus eventBus,
        EnemiesNearbyFinder enemiesNearby,
        AttackRuntimeFactory runtimeFactory);
}
