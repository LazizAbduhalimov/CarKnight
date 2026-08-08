using System.Collections.Generic;
using UnityEngine;

public sealed class AttackCardSelectionProvider : ICardSelectionProvider
{
    private readonly IAttackProgressionService _progressionService;
    private readonly IReadOnlyList<AttackSO> _availableAttacks;

    public AttackCardSelectionProvider(IAttackProgressionService progressionService, IReadOnlyList<AttackSO> availableAttacks)
    {
        _progressionService = progressionService;
        _availableAttacks = availableAttacks;
    }

    public IReadOnlyList<ICardSelectionOption> GetOptions()
    {
        if (_availableAttacks == null || _availableAttacks.Count == 0)
        {
            return new List<ICardSelectionOption>();
        }

        var options = new List<ICardSelectionOption>(_availableAttacks.Count * 2);
        for (var i = 0; i < _availableAttacks.Count; i++)
        {
            var attack = _availableAttacks[i];
            if (attack == null)
            {
                continue;
            }

            var unlockOption = new AttackUnlockSelectionOption(attack, _progressionService);
            if (unlockOption.CanSelect())
            {
                options.Add(unlockOption);
            }

            var upgradeOption = new AttackUpgradeSelectionOption(attack, _progressionService);
            if (upgradeOption.CanSelect())
            {
                options.Add(upgradeOption);
            }
        }

        Shuffle(options);
        return options;
    }

    private static void Shuffle(List<ICardSelectionOption> options)
    {
        if (options == null || options.Count < 2)
        {
            return;
        }

        for (var i = options.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1);
            (options[i], options[j]) = (options[j], options[i]);
        }
    }
}
