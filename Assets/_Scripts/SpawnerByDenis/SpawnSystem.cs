using PoolSystem.Alternative;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class SpawnSystem : MonoBehaviour
{
    private static readonly List<EnemyForSpawner> EmptyEnemies = new List<EnemyForSpawner>(0);

    [Header("Config")]
    [Required]
    [SerializeField] private SpawnSystemConfig _config;

    [ReadOnly]
    [SerializeField, Tooltip("Enemies currently available based on current level/caps")]
    private List<EnemyForSpawner> _availableEnemies = new List<EnemyForSpawner>();

    [Header("Spawn Settings")]
    [SerializeField, Tooltip("Class that returns Transform massive")]
    private SpawnerPointSetter _pointer;

    [Header("Runtime Control")]
    [SerializeField] private bool _canSpawn = true;
    [SerializeField] private Transform _contaier;

    [ShowInInspector, ReadOnly] private int _spawnerLvl;
    [ShowInInspector, ReadOnly] private int _enemiesOnScene;
    [ShowInInspector, ReadOnly] private float _elapsedTime;
    [ShowInInspector, ReadOnly] private float _progress01;
    [ShowInInspector, ReadOnly] private float _spawnAccumulator;
    [ShowInInspector, ReadOnly] private float _currentSpawnRate;
    [ShowInInspector, ReadOnly] private float _currentSpawnIntervalSeconds;
    [ShowInInspector, ReadOnly] private int _currentAliveCap;
    [ShowInInspector, ReadOnly] private int _aliveCapBonus;
    [ShowInInspector, ReadOnly] private int _manualLevelBonus;

    private PoolService _poolService;
    private Dictionary<MonoBehaviour, IPool<MonoBehaviour>> _pools = new Dictionary<MonoBehaviour, IPool<MonoBehaviour>>();
    private Dictionary<long, long> _enemiesCount = new();
    private Dictionary<EnemyForSpawner, long> _enemyTypeIds = new();
    private GameEvents _events;

    [Inject]
    private void Construct(PoolService poolService, GameEvents events)
    {
        _poolService = poolService;
        _events = events;
    }

    private void OnEnable()
    {
        if (_events != null)
            _events.EnemyKilled += EnemyDeath;
    }

    private void OnDisable()
    {
        if (_events != null)
            _events.EnemyKilled -= EnemyDeath;
    }

    private void Start()
    {
        ResolveSceneDependencies();
        if (_poolService == null)
        {
            Debug.LogError($"{nameof(SpawnSystem)} on '{name}' could not resolve {nameof(PoolService)}.", this);
            enabled = false;
            return;
        }

        if (_config == null)
        {
            Debug.LogError($"{nameof(SpawnSystem)} on '{name}' has no {nameof(SpawnSystemConfig)} assigned.");
            enabled = false;
            return;
        }

        ApplyConfig(_config, true);
    }

    private void InitializePools()
    {
        Transform parentObj = _contaier == null ? transform : _contaier;
        _pools.Clear();
        _enemiesCount.Clear();
        _enemyTypeIds.Clear();

        foreach (EnemyForSpawner enemy in GetEnemyList())
        {
            if (enemy == null || enemy.enemyPrefab == null) continue;

            var pool = _poolService.RegisterPool(enemy.enemyPrefab, enemy.poolSize, parentObj, enemy.autoExpand);
            _pools[enemy.enemyPrefab] = pool;

            long enemyTypeId = ResolveEnemyTypeId(enemy);
            if (!_enemiesCount.ContainsKey(enemyTypeId))
                _enemiesCount.Add(enemyTypeId, 0);
        }
    }

    private void ResolveSceneDependencies()
    {
        if (_poolService != null) return;

        var context = FindFirstObjectByType<SceneContext>();
        if (context == null || !context.HasResolved) return;

        _poolService = context.Container.TryResolve<PoolService>();
        _events ??= context.Container.TryResolve<GameEvents>();
    }

    private void ResetPacing()
    {
        _elapsedTime = 0f;
        _progress01 = 0f;
        _spawnAccumulator = 0f;
        _spawnerLvl = 0;
        _manualLevelBonus = 0;
        _enemiesOnScene = 0;
        _currentSpawnRate = 0f;
        _currentSpawnIntervalSeconds = 0f;
        _currentAliveCap = 0;
        _availableEnemies.Clear();
    }

    private void Update()
    {
        if (_config == null) return;

        if (!_canSpawn)
        {
            _spawnAccumulator = 0f;
            return;
        }

        UpdatePacing(Time.deltaTime);
        RefreshAvailableEnemies();

        if (_enemiesOnScene >= _currentAliveCap) return;

        _spawnAccumulator += _currentSpawnRate * Time.deltaTime;
        if (_spawnAccumulator < 1f) return;

        if (TrySpawnOne())
            _spawnAccumulator -= 1f;
    }

    private void UpdatePacing(float dt)
    {
        _elapsedTime += dt;

        _progress01 = Mathf.Clamp01(_elapsedTime / _config.TimeToMaxDifficulty);

        if (_config.AutoLevelByTime)
        {
            _spawnerLvl = Mathf.FloorToInt(_elapsedTime / _config.SecondsPerLevel) + _manualLevelBonus;
        }
        else
        {
            _spawnerLvl = _manualLevelBonus;
        }

        _currentSpawnIntervalSeconds = _config.EvaluateSpawnInterval(_progress01);
        _currentSpawnRate = _currentSpawnIntervalSeconds > 0f && !float.IsInfinity(_currentSpawnIntervalSeconds)
            ? 1f / _currentSpawnIntervalSeconds
            : 0f;

        int curveCap = _config.EvaluateAliveCap(_progress01);
        _currentAliveCap = Mathf.Max(0, curveCap + _aliveCapBonus);
    }

    private void RefreshAvailableEnemies()
    {
        _availableEnemies.Clear();

        foreach (var enemy in GetEnemyList())
        {
            if (enemy == null || enemy.enemyPrefab == null) continue;

            bool levelOk = _spawnerLvl >= enemy.levelForSpawn;
            bool onlyOneLevelOk = !enemy.onlyForOneLvl || _spawnerLvl == enemy.levelForSpawn;
            bool chanceOk = enemy.spawnChance > 0f;
            bool countOk = IsEnemyTypeCountAvailable(enemy);

            if (levelOk && onlyOneLevelOk && chanceOk && countOk)
                _availableEnemies.Add(enemy);
        }
    }

    private bool IsEnemyTypeCountAvailable(EnemyForSpawner enemyData)
    {
        long enemyTypeId = ResolveEnemyTypeId(enemyData);

        if (!_enemiesCount.ContainsKey(enemyTypeId))
            _enemiesCount[enemyTypeId] = 0;

        if (enemyData.maxEnemyCount <= 0) return false;

        return _enemiesCount[enemyTypeId] < enemyData.maxEnemyCount;
    }

    private bool TrySpawnOne()
    {
        if (_availableEnemies.Count == 0) return false;

        var pickedEnemy = PickEnemyByWeight();
        if (pickedEnemy == null) return false;

        return Spawn(pickedEnemy);
    }

    private EnemyForSpawner PickEnemyByWeight()
    {
        float totalChance = 0f;
        foreach (var enemy in _availableEnemies)
        {
            totalChance += Mathf.Max(0f, enemy.spawnChance);
        }

        if (totalChance <= 0f) return null;

        float randomPoint = Random.Range(0f, totalChance);
        float currentSum = 0f;

        foreach (var enemy in _availableEnemies)
        {
            currentSum += Mathf.Max(0f, enemy.spawnChance);
            if (randomPoint <= currentSum)
                return enemy;
        }

        return _availableEnemies[_availableEnemies.Count - 1];
    }

    private bool Spawn(EnemyForSpawner enemyData)
    {
        MonoBehaviour prefab = enemyData.enemyPrefab;
        if (_pointer == null || _poolService == null) return false;

        if (!_pools.TryGetValue(prefab, out var pool))
        {
            var parent = _contaier == null ? transform : _contaier;
            pool = _poolService.GetOrRegisterPool(prefab, enemyData.poolSize, parent, enemyData.autoExpand);
            _pools[prefab] = pool;
        }

        var posMassive = _pointer.GetSpawnPoint();
        if (posMassive == null || posMassive.Length == 0) return false;

        Transform spawnTransform = posMassive[Random.Range(0, posMassive.Length)];
        MonoBehaviour instance;
        try
        {
            instance = pool.GetFreeElement(spawnTransform.position, spawnTransform.rotation, false);
        }
        catch
        {
            return false;
        }

        if (instance == null) return false;

        instance.transform.position = spawnTransform.position;
        instance.transform.rotation = spawnTransform.rotation;
        SpawnedEnemyMarkerUtility.MarkIfEnemy(instance);

        NavMeshAgent agent = instance.GetComponent<NavMeshAgent>();
        if (agent != null && !agent.enabled)
            agent.enabled = true;

        instance.gameObject.SetActive(true);

        long enemyTypeId = ResolveEnemyTypeId(enemyData);
        if (!_enemiesCount.ContainsKey(enemyTypeId))
            _enemiesCount[enemyTypeId] = 0;

        _enemiesCount[enemyTypeId]++;
        _enemiesOnScene++;
        return true;
    }

    private void EnemyDeath(long id)
    {
        if (_enemiesCount.ContainsKey(id) && _enemiesCount[id] > 0)
        {
            _enemiesCount[id] = _enemiesCount[id] - 1;
            _enemiesOnScene = Mathf.Max(0, _enemiesOnScene - 1);
        }
    }

    public void LvlUp(int lvlForUp = 1)
    {
        int delta = Mathf.Max(0, lvlForUp);
        _manualLevelBonus += delta;
        _spawnerLvl += delta;

        if (delta > 0)
            SpawnCountUpgrader();
    }

    public void SpawnCountUpgrader(int howMuch = 0)
    {
        if (_config == null) return;

        if (howMuch == 0)
        {
            _aliveCapBonus += _config.SpawnEnemyCountUpgrader;
        }
        else
        {
            _aliveCapBonus += howMuch;
        }
    }

    [Button]
    public void RestartPacing()
    {
        _elapsedTime = 0f;
        _progress01 = 0f;
        _spawnAccumulator = 0f;
        _enemiesOnScene = 0;
        _currentSpawnRate = 0f;
        _currentSpawnIntervalSeconds = 0f;
        _currentAliveCap = 0;

        var keys = new List<long>(_enemiesCount.Keys);
        foreach (var key in keys)
            _enemiesCount[key] = 0;
    }

    public void SetSpawnEnabled(bool isEnabled)
    {
        _canSpawn = isEnabled;
    }

    public void ApplyConfig(SpawnSystemConfig config, bool resetProgress = true)
    {
        if (config == null)
        {
            Debug.LogWarning($"{nameof(SpawnSystem)}: tried to apply null config.");
            return;
        }

        _config = config;
        _canSpawn = _config.CanSpawnByDefault;
        InitializePools();

        if (resetProgress)
            ResetPacing();
    }

    private IReadOnlyList<EnemyForSpawner> GetEnemyList()
    {
        return _config == null ? EmptyEnemies : _config.EnemyList;
    }

    private long ResolveEnemyTypeId(EnemyForSpawner enemyData)
    {
        if (enemyData == null || enemyData.enemyPrefab == null)
            return 0;

        if (_enemyTypeIds.TryGetValue(enemyData, out long cachedTypeId))
            return cachedTypeId;

        long enemyTypeId = enemyData.enemyPrefab.GetInstanceID();
        Enemy enemyComponent = enemyData.enemyPrefab.GetComponent<Enemy>();
        if (enemyComponent != null)
            enemyTypeId = enemyComponent.enemyTypeId;

        _enemyTypeIds[enemyData] = enemyTypeId;

        return enemyTypeId;
    }
}
