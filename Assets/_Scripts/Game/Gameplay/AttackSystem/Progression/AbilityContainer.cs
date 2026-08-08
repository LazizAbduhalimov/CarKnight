using System;
using System.Collections.Generic;
using OnlyFun.EventBus;
using PoolSystem.Alternative;
using UnityEngine;

public sealed class AbilityContainer : IDisposable
{
    private const string GlobalMultiplierSourceId = "__global_multiplier";

    private readonly Transform _owner;
    private readonly PoolService _poolService;
    private readonly EventBus _eventBus;
    private readonly EnemiesNearbyFinder _enemiesNearby;
    private readonly AttackRuntimeFactory _runtimeFactory;
    private readonly IGameAudioService _audio;
    private readonly List<OwnedAttack> _ownedAttacks = new List<OwnedAttack>();
    private readonly Dictionary<AttackSO, OwnedAttack> _byConfig = new Dictionary<AttackSO, OwnedAttack>();
    private readonly Dictionary<string, OwnedAttack> _byId = new Dictionary<string, OwnedAttack>(StringComparer.Ordinal);
    private readonly Dictionary<string, float> _damageMultiplierSources = new Dictionary<string, float>(StringComparer.Ordinal);
    private float _globalDamageMultiplier = 1f;

    public IReadOnlyList<OwnedAttack> OwnedAttacks => _ownedAttacks;
    public float GlobalDamageMultiplier => _globalDamageMultiplier;

    public AbilityContainer(
        Transform owner,
        PoolService poolService,
        EventBus eventBus,
        EnemiesNearbyFinder enemiesNearby,
        AttackRuntimeFactory runtimeFactory,
        IGameAudioService audio)
    {
        _owner = owner;
        _poolService = poolService;
        _eventBus = eventBus;
        _enemiesNearby = enemiesNearby;
        _runtimeFactory = runtimeFactory;
        _audio = audio;
    }

    public bool Has(AttackSO attackConfig)
    {
        return attackConfig != null && _byConfig.ContainsKey(attackConfig);
    }

    public bool Has(string attackId)
    {
        if (string.IsNullOrWhiteSpace(attackId))
        {
            return false;
        }

        return _byId.ContainsKey(attackId);
    }

    public int GetLevel(AttackSO attackConfig)
    {
        if (attackConfig == null || !_byConfig.TryGetValue(attackConfig, out var ownedAttack))
        {
            return 0;
        }

        return ownedAttack.Level;
    }

    public int GetLevel(string attackId)
    {
        if (string.IsNullOrWhiteSpace(attackId) || !_byId.TryGetValue(attackId, out var ownedAttack))
        {
            return 0;
        }

        return ownedAttack.Level;
    }

    public IAttackRuntime GetRuntime(AttackSO attackConfig)
    {
        if (attackConfig == null || !_byConfig.TryGetValue(attackConfig, out var ownedAttack))
        {
            return null;
        }

        return ownedAttack.RuntimeAttack;
    }

    public IAttackRuntime GetRuntime(string attackId)
    {
        if (string.IsNullOrWhiteSpace(attackId) || !_byId.TryGetValue(attackId, out var ownedAttack))
        {
            return null;
        }

        return ownedAttack.RuntimeAttack;
    }

    public bool Unlock(AttackSO attackConfig, int initialLevel = 1)
    {
        if (attackConfig == null || attackConfig.Spec == null || Has(attackConfig))
        {
            return false;
        }

        var level = Mathf.Max(1, initialLevel);
        var runtime = BuildRuntime(attackConfig, level);
        if (runtime == null)
        {
            return false;
        }
        ApplyDamageMultiplier(runtime);

        var entry = new OwnedAttack(attackConfig, runtime, level);
        _ownedAttacks.Add(entry);
        _byConfig[attackConfig] = entry;
        _byId[ResolveAttackId(attackConfig)] = entry;
        return true;
    }

    public bool Upgrade(AttackSO attackConfig)
    {
        if (attackConfig == null || !_byConfig.TryGetValue(attackConfig, out var entry))
        {
            return false;
        }

        var maxLevel = AttackLevelTableUtility.GetMaxLevel(attackConfig.LevelTable);
        if (entry.Level >= maxLevel)
        {
            return false;
        }

        var nextLevel = entry.Level + 1;
        var newRuntime = BuildRuntime(attackConfig, nextLevel);
        if (newRuntime == null)
        {
            return false;
        }
        ApplyDamageMultiplier(newRuntime);

        entry.RuntimeAttack?.Dispose();
        entry.SetRuntime(newRuntime);
        entry.SetLevel(nextLevel);
        return true;
    }

    public bool Upgrade(string attackId)
    {
        if (string.IsNullOrWhiteSpace(attackId) || !_byId.TryGetValue(attackId, out var entry))
        {
            return false;
        }

        return Upgrade(entry.Config);
    }

    public void Tick()
    {
        for (var i = 0; i < _ownedAttacks.Count; i++)
        {
            _ownedAttacks[i]?.RuntimeAttack?.Tick();
        }
    }

    public void SetGlobalDamageMultiplier(float multiplier)
    {
        SetDamageMultiplierSource(GlobalMultiplierSourceId, multiplier);
    }

    public void SetDamageMultiplierSource(string sourceId, float multiplier)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            return;
        }

        _damageMultiplierSources[sourceId] = Mathf.Max(0f, multiplier);
        RecalculateGlobalDamageMultiplier();
        ApplyDamageMultiplierToAllAttacks();
    }

    public void RemoveDamageMultiplierSource(string sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            return;
        }

        if (!_damageMultiplierSources.Remove(sourceId))
        {
            return;
        }

        RecalculateGlobalDamageMultiplier();
        ApplyDamageMultiplierToAllAttacks();
    }

    private void ApplyDamageMultiplierToAllAttacks()
    {
        for (var i = 0; i < _ownedAttacks.Count; i++)
        {
            ApplyDamageMultiplier(_ownedAttacks[i]?.RuntimeAttack);
        }
    }

    private void RecalculateGlobalDamageMultiplier()
    {
        if (_damageMultiplierSources.Count == 0)
        {
            _globalDamageMultiplier = 1f;
            return;
        }

        var computedMultiplier = 1f;
        foreach (var source in _damageMultiplierSources)
        {
            computedMultiplier *= Mathf.Max(0f, source.Value);
        }

        _globalDamageMultiplier = Mathf.Max(0f, computedMultiplier);
    }

    public void Dispose()
    {
        for (var i = 0; i < _ownedAttacks.Count; i++)
        {
            _ownedAttacks[i]?.RuntimeAttack?.Dispose();
        }

        _ownedAttacks.Clear();
        _byConfig.Clear();
        _byId.Clear();
        _damageMultiplierSources.Clear();
        _globalDamageMultiplier = 1f;
    }

    private IAttackRuntime BuildRuntime(AttackSO attackConfig, int level)
    {
        var context = new AttackRuntimeContext(_poolService, _enemiesNearby, _owner, _eventBus, _audio);
        var runtime = _runtimeFactory.CreateRuntime(attackConfig, level, context);
        if (runtime == null)
        {
            Debug.LogWarning($"Failed to create runtime attack for '{attackConfig?.name}'. Check spec and level table setup.");
        }

        return runtime;
    }

    private static string ResolveAttackId(AttackSO attackConfig)
    {
        if (attackConfig == null)
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(attackConfig.AttackId) ? attackConfig.name : attackConfig.AttackId;
    }

    private void ApplyDamageMultiplier(IAttackRuntime runtime)
    {
        if (runtime is AttackTypeBase attackRuntime)
        {
            attackRuntime.SetDamageMultiplier(_globalDamageMultiplier);
        }
    }
}
