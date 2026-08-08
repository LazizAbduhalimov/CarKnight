using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SelectionPositionPointer :SpawnerPointSetter
{
    [SerializeField] private List<Transform> _allPoints = new List<Transform>();
    [SerializeField] private float _cellSize = 20f;
    [SerializeField] private float _maxSpawnRadius = 40f;
    [SerializeField] private float _minSpawnRadius = 40f;

    private Dictionary<Vector2Int, List<Transform>> _grid = new();
    private Transform _player;


    private float minSqr;
    private float maxSqr;

    private int range;

    [Inject]
    private void Initial(PlayerUnit player)
    {
        _player = player.gameObject.GetComponent<Transform>();

        _grid.Clear();
        foreach (Transform t in _allPoints)
        {

            Vector2Int key = GetGridKey(t.position);
            if(!_grid.ContainsKey(key)) _grid[key] = new List<Transform>();

            _grid[key].Add(t);
        }

         minSqr = _minSpawnRadius * _minSpawnRadius;
         maxSqr = _maxSpawnRadius * _maxSpawnRadius;
         range = Mathf.CeilToInt(_maxSpawnRadius / _cellSize);
    }

    private Vector2Int GetGridKey(Vector3 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / _cellSize),
            Mathf.FloorToInt(pos.z / _cellSize)
        );
    }



    public override Transform[] GetSpawnPoint()
    {
        if (_player == null) return new Transform[0];

        List<Transform> points = new List<Transform>();
        Vector3 playerPos = _player.position;
        Vector2Int playerKey = GetGridKey(playerPos);

        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                Vector2Int neighborKey = playerKey + new Vector2Int(x, z);

                if (_grid.TryGetValue(neighborKey, out var pointsInCell))
                {
                    foreach (var p in pointsInCell)
                    {
                        float sqrDist = (p.position - playerPos).sqrMagnitude;

                        if (sqrDist >= minSqr && sqrDist <= maxSqr)
                        {
                            points.Add(p);
                        }
                    }
                }
            }
        }

        return points.ToArray();
    }


}
