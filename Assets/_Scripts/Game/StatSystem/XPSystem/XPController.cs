using System;
using OnlyFun.EventBus;
using UnityEngine;

public class XPController : IDisposable
{
    private EventBus _eventBus;
    private XPView _xpView;
    private XPModel _xpModel;

    public XPController(EventBus eventBus, XPModel xpModel, XPView xpView)
    {
        _eventBus = eventBus;
        _xpModel = xpModel;
        _xpView = xpView;
        _xpView.UpdateXP(_xpModel.CurrentXP, _xpModel.XPToNextLevel, _xpModel.Level);
        Subscribe();
    }

    private void Subscribe()
    {
        _xpModel.OnXPChanged += OnXPChanged;
        _xpModel.OnLevelChanged += OnLevelChanged;
    }

    private void Unsubscribe()
    {
        _xpModel.OnXPChanged -= OnXPChanged;
        _xpModel.OnLevelChanged -= OnLevelChanged;
    }

    public void AddXP(int xp)
    {
        Debug.Log($"Adding {xp} XP");
        _xpModel.AddXP(xp);
    }

    private void OnXPChanged(int currentXP, int xpToNextLevel)
    {
        Debug.Log($"XP Changed: {currentXP}/{xpToNextLevel}");
        _xpView.UpdateXP(currentXP, xpToNextLevel, _xpModel.Level);
    }

    private void OnLevelChanged(int newLevel)
    {
        _xpView.UpdateXP(_xpModel.CurrentXP, _xpModel.XPToNextLevel, newLevel);
        _eventBus.Publish(new ELevelUp(newLevel)); // можно передать игрока если нужно через di 
        Debug.Log($"Level Changed: {newLevel}");
    }

    public void Dispose()
    {
        Unsubscribe();
    }
}