public abstract class FsmState
{
    protected Fsm Fsm { get; private set; }
    
    public FsmState(Fsm fsm)
    {
        Fsm = fsm;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
}
