using UnityEngine;

public static class LayerAssigner
{
    private static readonly string[] PlayerTargetLayers = { "Enemy", "EnemyGhost" };
    private static readonly string[] EnemyTargetLayers = { "Player", "PlayerGhost" };

    public static void AssignLayer(int ownerLayer, GameObject missile)
    {
        if (LayerMask.LayerToName(ownerLayer) == "Player")
        {
            missile.layer = LayerMask.NameToLayer("Player Missile");
        }
        else if (LayerMask.LayerToName(ownerLayer) == "Enemy")
        {
            missile.layer = LayerMask.NameToLayer("Enemy Missile");
        }
    }

    public static LayerMask ResolveTargetUnitMask(int ownerLayer, LayerMask fallbackMask = default)
    {
        var ownerLayerName = LayerMask.LayerToName(ownerLayer);

        if (ownerLayerName == "Player" && TryBuildMask(PlayerTargetLayers, out var enemyMask))
        {
            return enemyMask;
        }

        if (ownerLayerName == "Enemy" && TryBuildMask(EnemyTargetLayers, out var playerMask))
        {
            return playerMask;
        }

        return fallbackMask.value == 0 ? ~0 : fallbackMask;
    }

    public static int ResolveOwnerLayerFromMissileLayer(int missileLayer)
    {
        var missileLayerName = LayerMask.LayerToName(missileLayer);

        if (missileLayerName == "Player Missile")
        {
            return LayerMask.NameToLayer("Player");
        }

        if (missileLayerName == "Enemy Missile")
        {
            return LayerMask.NameToLayer("Enemy");
        }

        return -1;
    }

    private static bool TryGetMask(string layerName, out LayerMask layerMask)
    {
        var layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            layerMask = default;
            return false;
        }

        layerMask = 1 << layer;
        return true;
    }

    private static bool TryBuildMask(string[] layerNames, out LayerMask layerMask)
    {
        var combinedMask = 0;
        for (var i = 0; i < layerNames.Length; i++)
        {
            if (TryGetMask(layerNames[i], out var currentMask))
            {
                combinedMask |= currentMask.value;
            }
        }

        layerMask = combinedMask;
        return combinedMask != 0;
    }
}
