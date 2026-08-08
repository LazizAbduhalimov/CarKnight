using UnityEngine;

public sealed class LowHealthDamageBonusAttack : AttackTypeBase, IHiddenInUI
{
    public float HealthThresholdNormalized = 0.3f;
    public float BonusDamagePercent = 50f;
    public bool ScaleByMissingHealth = true;
    public float RecheckInterval = 0.1f;

    private HealthComponent _healthComponent;
    private IAttackProgressionService _progressionService;
    private string _multiplierSourceId;
    private float _lastAppliedMultiplier = -1f;
    private float _elapsedSinceRecheck;

    public override void PostInit()
    {
        if (Owner == null)
        {
            return;
        }

        _healthComponent = Owner.GetComponent<HealthComponent>();
        _progressionService = Owner.GetComponent<Attacker>()?.ProgressionService;
        _multiplierSourceId = $"low_hp_bonus_{GetHashCode()}";
    }

    public override void Tick()
    {
        if (_progressionService == null || _healthComponent == null)
        {
            return;
        }

        _elapsedSinceRecheck += Time.deltaTime;
        if (_elapsedSinceRecheck < Mathf.Max(0.01f, RecheckInterval))
        {
            return;
        }

        _elapsedSinceRecheck = 0f;
        var computedMultiplier = ComputeMultiplier();
        if (Mathf.Approximately(_lastAppliedMultiplier, computedMultiplier))
        {
            return;
        }

        _lastAppliedMultiplier = computedMultiplier;
        _progressionService.SetDamageMultiplierSource(_multiplierSourceId, computedMultiplier);
    }

    public override void Dispose()
    {
        if (_progressionService != null && !string.IsNullOrWhiteSpace(_multiplierSourceId))
        {
            _progressionService.RemoveDamageMultiplierSource(_multiplierSourceId);
        }
    }

    private float ComputeMultiplier()
    {
        var maxHealth = Mathf.Max(1, _healthComponent.MaxHealth);
        var healthRatio = Mathf.Clamp01((float)_healthComponent.CurrentHealth / maxHealth);
        var threshold = Mathf.Clamp01(HealthThresholdNormalized);
        if (healthRatio > threshold || BonusDamagePercent <= 0f)
        {
            return 1f;
        }

        if (!ScaleByMissingHealth || threshold <= 0.0001f)
        {
            return 1f + BonusDamagePercent / 100f;
        }

        var normalizedBoost = Mathf.Clamp01((threshold - healthRatio) / threshold);
        var bonusMultiplier = BonusDamagePercent / 100f * normalizedBoost;
        return 1f + bonusMultiplier;
    }
}
