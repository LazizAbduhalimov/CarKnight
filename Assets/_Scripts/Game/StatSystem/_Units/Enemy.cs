using OnlyFun.EventBus;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class Enemy : Unit
{
    [InfoBox("Значения компонентов перезаписываются из EnemyConfig, когда конфиг назначен.", InfoMessageType.Warning, nameof(HasConfig))]
    [SerializeField] private EnemyConfig _config;

    private EventBus _eventBus;
    public long enemyTypeId;

    public long enemyId;

    private GameEvents _gameEvents;


    [Inject]
    public void Construct(EventBus eventBus, GameEvents gameEvents)
    {
        _eventBus = eventBus;
        _gameEvents = gameEvents;
    }

    protected override void Awake()
    {
        base.Awake();
        ApplyConfig();
    }

    protected override void OnEnable() 
    {
        ApplyConfig();
        base.OnEnable();
        if (HealthComponent != null)
        {
            HealthComponent.OnDeath += HandleDeath;

            HealthComponent.OnDeath += OnDeathOnce;
        }
    }

    protected void OnDisable() 
    {
        if (HealthComponent != null)
        {
            HealthComponent.OnDeath -= HandleDeath;

            HealthComponent.OnDeath -= OnDeathOnce;
        }

    }

    private void OnDeathOnce()
    {
        _gameEvents?.OnEnemyKilled(enemyTypeId);
    }

    public void SetConfig(EnemyConfig config, bool applyNow = true)
    {
        _config = config;
        if (applyNow)
        {
            ApplyConfig();
        }
    }

    public void ApplyConfig()
    {
        if (_config == null)
        {
            return;
        }

        _config.ApplyTo(this);
    }

    private bool HasConfig => _config != null;

    private void HandleDeath()
    {
        _eventBus?.Publish(new EEnemyDeath { Enemy = this });
    }
}
