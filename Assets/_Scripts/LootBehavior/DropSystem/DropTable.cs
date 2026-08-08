using System.Collections.Generic;
using UnityEngine;

public class DropTable : MonoBehaviour
{
    public List<DropItem> drops = new();
    private HealthComponent _health;

    public void SetDrops(IReadOnlyList<DropItem> configuredDrops)
    {
        if (configuredDrops == null)
        {
            return;
        }

        drops ??= new List<DropItem>();
        drops.Clear();

        for (var i = 0; i < configuredDrops.Count; i++)
        {
            var configuredDrop = configuredDrops[i];
            if (configuredDrop != null)
            {
                drops.Add(CloneDropItem(configuredDrop));
            }
        }
    }

    private void Start()
    {
        gameObject.TryGetComponent(out HealthComponent healthComponent);
        _health = healthComponent;

        if (_health != null)
        {
            _health.OnDeath += OnDeath;
        }
    }

    private void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnDeath -= OnDeath;
        }
    }

    private void OnDeath()
    {
        DropSystem.Instance.Drop(this, transform.position);

        //if (_health != null)
        //{
        //    _health.OnDeath -= OnDeath;
        //}
    }

    private static DropItem CloneDropItem(DropItem source)
    {
        return new DropItem
        {
            prefab = source.prefab,
            dropChance = source.dropChance,
            minAmount = source.minAmount,
            maxAmount = source.maxAmount
        };
    }
}
