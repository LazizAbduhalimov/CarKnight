using System;
using System.Collections.Generic;

public class Fsm
{
    private FsmState StateCurrent { get; set; }
    private readonly Dictionary<Type, FsmState> _states = new();

    public void AddState(FsmState state)
    {
        _states.Add(state.GetType(), state);
    }

    public void SetState<T>() where T : FsmState
    {
        var type = typeof(T);

        if (StateCurrent != null && StateCurrent.GetType() == type)
            return;

        if (_states.TryGetValue(type, out var newState))
            ChangeState(newState);
    }

    public void ChangeState(FsmState newState)
    {
        StateCurrent?.Exit();
        StateCurrent = newState;
        StateCurrent.Enter();
    }

    public void Update()
    {
        StateCurrent?.Update();
    }

    public void FixedUpdate()
    {
        StateCurrent?.FixedUpdate();
    }
}
