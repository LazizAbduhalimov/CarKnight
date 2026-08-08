using UnityEngine;

public class AbilitySlotItemPresenter
{
    private readonly AbilitySlotItem _view;
    private readonly OwnedAttack _ownedAttack;

    public AbilitySlotItemPresenter(AbilitySlotItem view, OwnedAttack ownedAttack)
    {
        _view = view;
        _ownedAttack = ownedAttack;

        if (_ownedAttack?.Config != null)
        {
            _view.SetIcon(_ownedAttack.Config.AbilityIcon);
        }

        Tick();
    }

    public void Tick()
    {
        if (_ownedAttack?.RuntimeAttack == null)
        {
            _view.SetCooldownFill(0f);
            _view.SetCooldownText(string.Empty);
            return;
        }

        var attack = _ownedAttack.RuntimeAttack;
        if (attack is IHiddenInUI || _ownedAttack.Config is IHiddenInUI || _ownedAttack.Config?.Spec is IHiddenInUI)
        {
            _view.SetCooldownFill(0f);
            _view.SetCooldownText(string.Empty);
            return;
        }

        var text = attack.CooldownRemaining > 0f ? Mathf.Ceil(attack.CooldownRemaining).ToString() : "";
        _view.SetCooldownText(text);
        _view.SetCooldownFill(attack.CooldownRemainingNormalized);
    }
}
