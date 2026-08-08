using PrimeTween;
using UnityEngine;

public abstract class BurstMissileAttackBase<T> : MissileSpawnAttackBase<T> where T : MissileBase
{
    [Header("Burst Attack Settings")]
    public int ProjectilesPerAttack = 1;
    public float DelayBetweenProjectiles = 0f;
    public bool RetargetDuringBurst = true;

    private int _shotsRemainingInBurst;
    private bool _isBurstActive;
    private Transform _lockedBurstTarget;
    private Tween _nextShotTween;

    public override void Tick()
    {
        if (_isBurstActive || Owner == null || EnemiesNearby == null) return;

        PassedTime += Time.deltaTime;
        if (PassedTime < AttackRate)
        {
            return;
        }

        if (!CanStartBurst()) return;

        StartBurst();
    }

    private void StartBurst()
    {
        _shotsRemainingInBurst = Mathf.Max(1, ProjectilesPerAttack);
        _isBurstActive = true;
        _lockedBurstTarget = RetargetDuringBurst ? null : Target;
        OnBurstStarted();
        EventBus?.Publish(new EAttackEvent(Owner, Target, this));
        FireBurstShots();
    }

    private void FireBurstShots()
    {
        if (!_isBurstActive) return;

        if (DelayBetweenProjectiles <= 0f)
        {
            while (_shotsRemainingInBurst > 0 && TryFireSingleShot()) { }
            FinishBurst();
            return;
        }

        if (!TryFireSingleShot() || _shotsRemainingInBurst <= 0)
        {
            FinishBurst();
            return;
        }

        _nextShotTween.Stop();
        _nextShotTween = Tween.Delay(DelayBetweenProjectiles, FireBurstShots);
    }

    private bool TryFireSingleShot()
    {
        if (!TryResolveTargetForNextShot())
        {
            return false;
        }

        Attack();
        _shotsRemainingInBurst--;
        return true;
    }

    private bool TryResolveTargetForNextShot()
    {
        if (RetargetDuringBurst)
        {
            return CanFireSingleShot();
        }

        if (_lockedBurstTarget != null)
        {
            Target = _lockedBurstTarget;
            return true;
        }

        return CanFireSingleShot();
    }

    private bool TryUpdateTarget()
    {
        Target = EnemiesNearby.GetNearestEnemy(Owner.position);
        return Target != null;
    }

    protected virtual bool CanStartBurst()
    {
        return TryUpdateTarget();
    }

    protected virtual bool CanFireSingleShot()
    {
        return TryUpdateTarget();
    }

    protected virtual void OnBurstStarted() {}

    private void FinishBurst()
    {
        _nextShotTween.Stop();
        _shotsRemainingInBurst = 0;
        _isBurstActive = false;
        _lockedBurstTarget = null;
        EventBus?.Publish(new EAfterFullBurstAttackEvent(Owner, Target, this));
        PassedTime = 0f;
    }
}
