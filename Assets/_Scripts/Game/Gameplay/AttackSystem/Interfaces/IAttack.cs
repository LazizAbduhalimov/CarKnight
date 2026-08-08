using OnlyFun.EventBus;
using PoolSystem.Alternative;
using UnityEngine;

public readonly struct AttackRuntimeContext
{
    public readonly PoolService PoolService;
    public readonly EnemiesNearbyFinder EnemiesNearby;
    public readonly Transform Owner;
    public readonly EventBus EventBus;
    public readonly IGameAudioService Audio;

    public AttackRuntimeContext(
        PoolService poolService,
        EnemiesNearbyFinder enemiesNearby,
        Transform owner,
        EventBus eventBus,
        IGameAudioService audio)
    {
        PoolService = poolService;
        EnemiesNearby = enemiesNearby;
        Owner = owner;
        EventBus = eventBus;
        Audio = audio;
    }
}

public interface IAttackRuntime
{
    void Initialize(AttackRuntimeContext context);
    void Tick();
    void Attack();
    void Dispose();
    float CooldownDuration { get; }
    float CooldownElapsed { get; }
    float CooldownRemaining { get; }
    float CooldownRemainingNormalized { get; }
    float CooldownProgressNormalized { get; }
}
