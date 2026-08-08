using UnityEngine;

public sealed class DamageNumberService : IDamageNumberService
{
    private readonly DamagesProContainer _container;

    public DamageNumberService(DamagesProContainer container)
    {
        _container = container;
    }

    public void SpawnNumber(DamagesProContainer.DamageNumberPreset preset, Vector3 position, float number)
    {
        var prefab = _container?.GetPrefab(preset);
        if (prefab == null)
        {
            Debug.LogWarning($"DamageNumber prefab for preset '{preset}' is not assigned.", _container);
            return;
        }

        prefab.Spawn(position, number);
    }

    public void SpawnText(DamagesProContainer.DamageNumberPreset preset, Vector3 position, string text)
    {
        var prefab = _container?.GetPrefab(preset);
        if (prefab == null)
        {
            Debug.LogWarning($"DamageNumber prefab for preset '{preset}' is not assigned.", _container);
            return;
        }

        prefab.Spawn(position, text);
    }
}
