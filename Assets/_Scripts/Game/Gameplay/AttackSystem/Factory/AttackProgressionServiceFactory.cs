using OnlyFun.EventBus;
using PoolSystem.Alternative;
using UnityEngine;
using Zenject;

public sealed class AttackProgressionServiceFactory : IAttackProgressionServiceFactory
{
    private readonly IGameAudioService _audio;

    [Inject]
    public AttackProgressionServiceFactory([InjectOptional] IGameAudioService audio = null)
    {
        _audio = audio;
    }

    public IAttackProgressionService Create(
        Transform owner,
        string ownerName,
        PoolService poolService,
        EventBus eventBus,
        EnemiesNearbyFinder enemiesNearby,
        AttackRuntimeFactory runtimeFactory)
    {
        var abilityContainer = new AbilityContainer(owner, poolService, eventBus, enemiesNearby, runtimeFactory, _audio);
        return new AttackProgressionService(abilityContainer, ownerName);
    }
}
