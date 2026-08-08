using System;
using Zenject;

public sealed class AttackComboCoordinator : IInitializable, IDisposable
{
    private readonly Attacker _attacker;
    private readonly IAttackProgressionService _progressionService;
    private readonly HealthComponent _healthComponent;
    private readonly AttackComboTableSO _comboTable;
    public AttackComboService _comboService;

    public AttackComboCoordinator(
        Attacker attacker,
        IAttackProgressionService progressionService,
        HealthComponent healthComponent,
        [InjectOptional] AttackComboTableSO comboTable)
    {
        _attacker = attacker;
        _progressionService = progressionService;
        _healthComponent = healthComponent;
        _comboTable = comboTable;
    }

    public void Initialize()
    {
        if (_comboTable == null || _progressionService == null || _attacker == null)
        {
            return;
        }

        _comboService = new AttackComboService(_progressionService, _healthComponent, _comboTable);
        _attacker.OwnedAttacksChanged += HandleOwnedAttacksChanged;
        _comboService.EvaluateCombos();
    }

    public void Dispose()
    {
        if (_attacker != null)
        {
            _attacker.OwnedAttacksChanged -= HandleOwnedAttacksChanged;
        }

        if (_comboService == null)
        {
            return;
        }

        _comboService.Dispose();
        _comboService = null;
    }

    private void HandleOwnedAttacksChanged()
    {
        _comboService?.EvaluateCombos();
    }
}
