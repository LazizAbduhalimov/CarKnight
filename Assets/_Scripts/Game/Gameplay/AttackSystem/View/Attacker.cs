using System;
using System.Collections.Generic;
using OnlyFun.EventBus;
using PoolSystem.Alternative;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class Attacker: MonoBehaviour
{
    private static readonly IReadOnlyList<OwnedAttack> EmptyOwnedAttacks = Array.Empty<OwnedAttack>();
    private static readonly IReadOnlyList<AttackSO> EmptyAttackConfigs = Array.Empty<AttackSO>();

    [SerializeField] private float _enemySearchUpdateInterval = 0.5f;
    [SerializeField] private float _enemyDetectionRange = 30f;
    [SerializeField] private LayerMask _enemyLayerMask;

    [FormerlySerializedAs("AttackSOList")]
    [SerializeField] private List<AttackSO> _startingAttackConfigs;

    [SerializeField, ReadOnly] private int _ownedAttacksCount;

    public IReadOnlyList<OwnedAttack> OwnedAttacks => _progressionService?.OwnedAttacks ?? EmptyOwnedAttacks;
    public IAttackProgressionService ProgressionService => _progressionService;
    public IReadOnlyList<AttackSO> StartingAttackConfigs => _startingAttackConfigs ?? EmptyAttackConfigs;
    public IReadOnlyList<AttackSO> AttackConfigs => StartingAttackConfigs;
    public IReadOnlyList<AttackSO> AttackSOList => StartingAttackConfigs;

    public event Action OwnedAttacksChanged;

    private PoolService _poolService;
    private EventBus _eventBus;
    private readonly AttackRuntimeFactory _runtimeFactory = new AttackRuntimeFactory();
    private EnemySearchService _enemySearchService;
    private IAttackProgressionService _progressionService;
    private IAttackProgressionServiceFactory _attackProgressionServiceFactory;

    [Inject]
    private void Construct(
        PoolService poolService,
        EventBus eventBus,
        [InjectOptional] IAttackProgressionServiceFactory attackProgressionServiceFactory)
    {
        _poolService = poolService;
        _eventBus = eventBus;
        _attackProgressionServiceFactory = attackProgressionServiceFactory ?? new AttackProgressionServiceFactory();
        InitializeAbilities();
    }

    private void OnDestroy()
    {
        if (_progressionService != null)
        {
            _progressionService.OwnedAttacksChanged -= HandleOwnedAttacksChanged;
            _progressionService.Dispose();
            _progressionService = null;
        }
    }

    private void Update()
    {
        _enemySearchService?.Tick(transform.position, Time.deltaTime);
        _progressionService?.Tick();
    }

    private void InitializeAbilities()
    {
        _enemySearchService = new EnemySearchService(_enemyLayerMask, _enemySearchUpdateInterval, _enemyDetectionRange);

        if (_progressionService != null)
        {
            _progressionService.OwnedAttacksChanged -= HandleOwnedAttacksChanged;
            _progressionService.Dispose();
        }

        _progressionService = _attackProgressionServiceFactory.Create(
            transform,
            name,
            _poolService,
            _eventBus,
            _enemySearchService.Finder,
            _runtimeFactory);
        _progressionService.OwnedAttacksChanged += HandleOwnedAttacksChanged;
        _progressionService.InitializeStartingAttacks(_startingAttackConfigs);
    }

    public void SetStartingAttackConfigs(IReadOnlyList<AttackSO> attackConfigs, bool reinitializeIfReady = true)
    {
        _startingAttackConfigs ??= new List<AttackSO>();
        _startingAttackConfigs.Clear();

        if (attackConfigs != null)
        {
            for (var i = 0; i < attackConfigs.Count; i++)
            {
                var attackConfig = attackConfigs[i];
                if (attackConfig != null)
                {
                    _startingAttackConfigs.Add(attackConfig);
                }
            }
        }

        if (reinitializeIfReady && IsReadyToInitialize())
        {
            InitializeAbilities();
        }
    }

    public void SetEnemySearchSettings(float updateInterval, float detectionRange, LayerMask enemyLayerMask, bool reinitializeIfReady = true)
    {
        _enemySearchUpdateInterval = Mathf.Max(0.01f, updateInterval);
        _enemyDetectionRange = Mathf.Max(0f, detectionRange);
        _enemyLayerMask = enemyLayerMask;

        if (reinitializeIfReady && IsReadyToInitialize())
        {
            InitializeAbilities();
        }
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var range = _enemySearchService == null ? _enemyDetectionRange : _enemySearchService.DetectionRange;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    private void HandleOwnedAttacksChanged()
    {
        _ownedAttacksCount = OwnedAttacks.Count;
        OwnedAttacksChanged?.Invoke();
    }

    private bool IsReadyToInitialize()
    {
        return _poolService != null && _eventBus != null && _attackProgressionServiceFactory != null;
    }
}
