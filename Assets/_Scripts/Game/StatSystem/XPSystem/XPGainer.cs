using System;
using OnlyFun.EventBus;
using UnityEngine;
using Zenject;

public class XPGainer : IDisposable
{
    private XPController _xpController;
    private EventBus _eventBus;
    
    [Inject]
    public void Construct(XPController xpController, EventBus eventBus)
    {
        _xpController = xpController;
        _eventBus = eventBus;
        _eventBus.Subscribe<EEnemyDeath>(GainXP);
    }

    public void Dispose()
    {
        _eventBus.Unsubscribe<EEnemyDeath>(GainXP);
    }

    public void GainXP(EEnemyDeath enemy)
    {
        var xp = UnityEngine.Random.Range(1, 5);
        Debug.Log($"Gaining <color=red>{xp}</color> XP from enemy {enemy.Enemy.name}");
        _xpController.AddXP(xp);
    }
}