using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/Units/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [SerializeField] private UnitComponentConfigData _components = new();
    [SerializeField] private bool _overrideEnemyTypeId;
    [SerializeField] private long _enemyTypeId;
    [SerializeField] private bool _overrideEnemyId;
    [SerializeField] private long _enemyId;

    public void ApplyTo(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        _components?.Apply(enemy.gameObject);

        if (_overrideEnemyTypeId)
        {
            enemy.enemyTypeId = _enemyTypeId;
        }

        if (_overrideEnemyId)
        {
            enemy.enemyId = _enemyId;
        }
    }
}
