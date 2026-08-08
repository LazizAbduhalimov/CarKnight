using UnityEngine;

public interface IDamageNumberService
{
    void SpawnNumber(DamagesProContainer.DamageNumberPreset preset, Vector3 position, float number);
    void SpawnText(DamagesProContainer.DamageNumberPreset preset, Vector3 position, string text);
}
