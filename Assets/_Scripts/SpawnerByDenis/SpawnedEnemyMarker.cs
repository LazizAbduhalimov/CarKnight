using UnityEngine;

[DisallowMultipleComponent]
public sealed class SpawnedEnemyMarker : MonoBehaviour
{
}

public static class SpawnedEnemyMarkerUtility
{
    public static bool IsTeleportableSpawnedEnemy(Enemy enemy)
    {
        return enemy != null && enemy.TryGetComponent<SpawnedEnemyMarker>(out _);
    }

    public static void MarkIfEnemy(Component instance)
    {
        if (instance == null)
            return;

        if (!instance.TryGetComponent<Enemy>(out _))
            return;

        if (!instance.TryGetComponent<SpawnedEnemyMarker>(out _))
            instance.gameObject.AddComponent<SpawnedEnemyMarker>();
    }
}