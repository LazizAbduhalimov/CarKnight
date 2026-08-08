using System;
using Unity.VisualScripting;
using UnityEngine;

public partial class GameEvents 
{
    public event Action<long> EnemyKilled;
    public event Action<int> CoinsCollected;
    public event Action<int> ExpCollected;

    public void OnEnemyKilled(long enemyID) => EnemyKilled?.Invoke(enemyID);
    public void OnCoinsCollected(int amount) => CoinsCollected?.Invoke(amount);
    public void OnExpCollected(int amount) => ExpCollected?.Invoke(amount);


}
