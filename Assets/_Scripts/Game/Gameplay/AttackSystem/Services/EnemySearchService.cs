using UnityEngine;

public sealed class EnemySearchService
{
    private readonly float _updateInterval;
    private readonly float _detectionRange;
    private readonly EnemiesNearbyFinder _finder;
    private float _timeSinceUpdate;

    public EnemySearchService(LayerMask enemyLayerMask, float updateInterval, float detectionRange)
    {
        _finder = new EnemiesNearbyFinder(enemyLayerMask);
        _updateInterval = Mathf.Max(0.01f, updateInterval);
        _detectionRange = Mathf.Max(0f, detectionRange);
    }

    public EnemiesNearbyFinder Finder => _finder;
    public float DetectionRange => _detectionRange;

    public void Tick(Vector3 ownerPosition, float deltaTime)
    {
        _timeSinceUpdate += deltaTime;
        if (_timeSinceUpdate < _updateInterval)
        {
            return;
        }

        _timeSinceUpdate = 0f;
        _finder.UpdateEnemiesInRange(ownerPosition, _detectionRange);
        _finder.GetNearestEnemy(ownerPosition, true);
    }
}

