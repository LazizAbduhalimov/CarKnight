using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnSystemConfig", menuName = "Game/Spawner/Spawn System Config")]
public class SpawnSystemConfig : ScriptableObject
{
    [Header("Enemies settings")]
    [SerializeField, TableList] private List<EnemyForSpawner> _enemyList = new List<EnemyForSpawner>();

    [Header("Pacing")]
    [SerializeField, Tooltip("Time in seconds to reach max difficulty point on curves")]
    private float _timeToMaxDifficulty = 480f;
    [SerializeField, Tooltip("Automatic level-up by time. This unlocks enemies with higher levelForSpawn")]
    private bool _autoLevelByTime = true;
    [SerializeField, Tooltip("Seconds required for each automatic spawner level")]
    private float _secondsPerLevel = 60f;

    [Header("Spawn Rate (Remap)")]
    [SerializeField, Tooltip("If enabled: normalized curve (0..1) is remapped to interval seconds, then converted to enemies/sec")]
    private bool _useSpawnIntervalRemap = true;
    [SerializeField, Tooltip("0..1 pacing curve (0 = slowest interval, 1 = fastest interval)")]
    private AnimationCurve _spawnRateNormalizedCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.2f, 0.15f),
        new Keyframe(0.55f, 0.6f),
        new Keyframe(1f, 1f)
    );
    [SerializeField, MinValue(0.05f), Tooltip("Slowest spawn interval in seconds (start when normalized=0)")]
    private float _slowestSpawnIntervalSeconds = 8f;
    [SerializeField, MinValue(0.05f), Tooltip("Fastest spawn interval in seconds (end when normalized=1)")]
    private float _fastestSpawnIntervalSeconds = 0.8f;

    [Header("Spawn Rate (Legacy)")]
    [SerializeField, Tooltip("Used only when Spawn Interval Remap is disabled. Units: enemies per second")]
    private AnimationCurve _spawnRateCurve = new AnimationCurve(
        new Keyframe(0f, 0.2f),
        new Keyframe(0.2f, 0.4f),
        new Keyframe(0.55f, 0.9f),
        new Keyframe(1f, 1.7f)
    );
    [SerializeField, Tooltip("Alive enemies cap curve by progress 0..1")]
    private AnimationCurve _aliveCapCurve = new AnimationCurve(
        new Keyframe(0f, 3f),
        new Keyframe(0.2f, 5f),
        new Keyframe(0.55f, 9f),
        new Keyframe(1f, 14f)
    );

    [Header("Runtime Defaults")]
    [SerializeField] private bool _canSpawnByDefault = true;

    [Header("Legacy Controls")]
    [SerializeField] private int _spawnEnemyCountUpgrader;

    public IReadOnlyList<EnemyForSpawner> EnemyList => _enemyList;
    public bool AutoLevelByTime => _autoLevelByTime;
    public bool CanSpawnByDefault => _canSpawnByDefault;
    public int SpawnEnemyCountUpgrader => _spawnEnemyCountUpgrader;

    public float TimeToMaxDifficulty => Mathf.Max(1f, _timeToMaxDifficulty);
    public float SecondsPerLevel => Mathf.Max(1f, _secondsPerLevel);

    public float EvaluateSpawnRate(float progress01)
    {
        float intervalSeconds = EvaluateSpawnInterval(progress01);
        if (intervalSeconds <= 0f || float.IsInfinity(intervalSeconds)) return 0f;
        return 1f / intervalSeconds;
    }

    public float EvaluateSpawnInterval(float progress01)
    {
        float t = Mathf.Clamp01(progress01);

        if (_useSpawnIntervalRemap)
        {
            float normalized = 0f;
            if (_spawnRateNormalizedCurve != null && _spawnRateNormalizedCurve.length > 0)
                normalized = Mathf.Clamp01(_spawnRateNormalizedCurve.Evaluate(t));

            float slow = Mathf.Max(0.05f, _slowestSpawnIntervalSeconds);
            float fast = Mathf.Max(0.05f, _fastestSpawnIntervalSeconds);
            return Mathf.Max(0.05f, Mathf.Lerp(slow, fast, normalized));
        }

        float legacyRate = EvaluateLegacySpawnRate(t);
        if (legacyRate <= 0f) return float.PositiveInfinity;
        return 1f / legacyRate;
    }

    private float EvaluateLegacySpawnRate(float progress01)
    {
        if (_spawnRateCurve == null || _spawnRateCurve.length == 0) return 0f;
        return Mathf.Max(0f, _spawnRateCurve.Evaluate(Mathf.Clamp01(progress01)));
    }

    public int EvaluateAliveCap(float progress01)
    {
        if (_aliveCapCurve == null || _aliveCapCurve.length == 0) return 0;
        return Mathf.Max(0, Mathf.RoundToInt(_aliveCapCurve.Evaluate(Mathf.Clamp01(progress01))));
    }
}
