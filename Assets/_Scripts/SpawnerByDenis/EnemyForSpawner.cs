using UnityEngine;

[System.Serializable]
public class EnemyForSpawner
{
    [Tooltip("Enemy prefab to spawn")] public MonoBehaviour enemyPrefab;
    [Range(0f, 100f), Tooltip("Relative weight for random pick")] public float spawnChance = 1f;
    [Min(0), Tooltip("Spawner level required to unlock this enemy")] public int levelForSpawn;
    [Min(1), Tooltip("Max alive enemies of this type at the same time")] public int maxEnemyCount = 5;
    [Tooltip("Spawn only exactly on levelForSpawn")] public bool onlyForOneLvl;

    [Min(1)] public int poolSize = 10;
    public bool autoExpand = true;
}
