using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PositionPoiner : SpawnerPointSetter
{
    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();


    public override Transform[] GetSpawnPoint()
    {
        return _spawnPoints.ToArray();
    }
}
