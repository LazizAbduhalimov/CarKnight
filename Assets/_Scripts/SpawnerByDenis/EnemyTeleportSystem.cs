using System;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public sealed class EnemyTeleportSettings
{
    public EnemyTeleportSettings(
        float maxDistanceFromHero,
        float teleportDistanceAheadOfHero,
        float checkIntervalSeconds,
        float enemyListRefreshIntervalSeconds,
        float spreadRadius,
        float spreadAngleDegrees,
        float navMeshSampleRadius,
        int teleportCandidateAttempts)
    {
        MaxDistanceFromHero = Mathf.Max(0f, maxDistanceFromHero);
        TeleportDistanceAheadOfHero = Mathf.Max(0f, teleportDistanceAheadOfHero);
        CheckIntervalSeconds = Mathf.Max(0.05f, checkIntervalSeconds);
        EnemyListRefreshIntervalSeconds = Mathf.Max(0.05f, enemyListRefreshIntervalSeconds);
        SpreadRadius = Mathf.Max(0f, spreadRadius);
        SpreadAngleDegrees = Mathf.Clamp(spreadAngleDegrees, 0f, 89f);
        NavMeshSampleRadius = Mathf.Max(0.1f, navMeshSampleRadius);
        TeleportCandidateAttempts = Mathf.Max(1, teleportCandidateAttempts);
    }

    public float MaxDistanceFromHero { get; }
    public float TeleportDistanceAheadOfHero { get; }
    public float CheckIntervalSeconds { get; }
    public float EnemyListRefreshIntervalSeconds { get; }
    public float SpreadRadius { get; }
    public float SpreadAngleDegrees { get; }
    public float NavMeshSampleRadius { get; }
    public int TeleportCandidateAttempts { get; }
}

public sealed class EnemyTeleportSystem : ITickable
{
    private readonly PlayerUnit _playerUnit;
    private readonly EnemyTeleportSettings _settings;

    private Enemy[] _enemies = Array.Empty<Enemy>();
    private float _nextCheckTime;
    private float _nextEnemyListRefreshTime;
    private readonly float _maxDistanceSqr;

    [Inject]
    public EnemyTeleportSystem(PlayerUnit playerUnit, EnemyTeleportSettings settings)
    {
        _playerUnit = playerUnit;
        _settings = settings;
        _maxDistanceSqr = _settings.MaxDistanceFromHero * _settings.MaxDistanceFromHero;
    }

    public void Tick()
    {
        if (_playerUnit == null)
            return;

        if (Time.time < _nextCheckTime)
            return;

        _nextCheckTime = Time.time + _settings.CheckIntervalSeconds;

        if (_enemies.Length == 0 || Time.time >= _nextEnemyListRefreshTime)
        {
            _nextEnemyListRefreshTime = Time.time + _settings.EnemyListRefreshIntervalSeconds;
            _enemies = UnityEngine.Object.FindObjectsOfType<Enemy>(true);
        }

        if (_enemies.Length == 0)
            return;

        Transform heroTransform = _playerUnit.transform;
        Vector3 heroPosition = heroTransform.position;

        Vector3 heroForward = heroTransform.forward;
        heroForward.y = 0f;
        if (heroForward.sqrMagnitude < 0.001f)
            heroForward = Vector3.forward;
        else
            heroForward.Normalize();

        for (int i = 0; i < _enemies.Length; i++)
        {
            Enemy enemy = _enemies[i];
            if (enemy == null || !enemy.isActiveAndEnabled)
                continue;

            if (!SpawnedEnemyMarkerUtility.IsTeleportableSpawnedEnemy(enemy))
                continue;

            Vector3 toEnemy = enemy.transform.position - heroPosition;
            toEnemy.y = 0f;

            if (toEnemy.sqrMagnitude <= _maxDistanceSqr)
                continue;

            TeleportEnemy(enemy, heroPosition, heroForward);
        }
    }

    private void TeleportEnemy(Enemy enemy, Vector3 heroPosition, Vector3 heroForward)
    {
        if (!TryGetTeleportPosition(heroPosition, heroForward, out Vector3 targetPosition))
            return;

        if (enemy.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        if (enemy.TryGetComponent(out NavMeshAgent agent) && agent.enabled)
        {
            bool warpSucceeded = agent.Warp(targetPosition);
            if (!warpSucceeded || !agent.isOnNavMesh)
                return;

            agent.ResetPath();
        }
        else
        {
            enemy.transform.position = targetPosition;
        }

        Vector3 lookDirection = heroPosition - targetPosition;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.001f)
            enemy.transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
    }

    private bool TryGetTeleportPosition(Vector3 heroPosition, Vector3 heroForward, out Vector3 targetPosition)
    {
        float baseDistance = Mathf.Max(0.1f, _settings.TeleportDistanceAheadOfHero);

        for (int attempt = 0; attempt < _settings.TeleportCandidateAttempts; attempt++)
        {
            float angleOffset = UnityEngine.Random.Range(-_settings.SpreadAngleDegrees, _settings.SpreadAngleDegrees);
            float distanceOffset = UnityEngine.Random.Range(-_settings.SpreadRadius, _settings.SpreadRadius);
            float distance = Mathf.Max(0.1f, baseDistance + distanceOffset);

            Vector3 direction = Quaternion.AngleAxis(angleOffset, Vector3.up) * heroForward;
            Vector3 candidatePosition = heroPosition + direction * distance;

            if (NavMesh.SamplePosition(candidatePosition, out NavMeshHit navMeshHit, _settings.NavMeshSampleRadius, NavMesh.AllAreas))
            {
                targetPosition = navMeshHit.position;
                return true;
            }
        }

        targetPosition = default;
        return false;
    }
}
