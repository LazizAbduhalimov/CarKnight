using System.Collections.Generic;
using OnlyFun.EventBus;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class AbilityChoiceUsageExample : MonoBehaviour
{
    [FormerlySerializedAs("_abilityChoices")]
    public List<AttackSO> AbilityChoices;
    public SelectionMenuMb SelectionMenu;

    private SelectionMenuPresenter _controller;

    [Inject]
    private void Construct(IAttackProgressionService progressionService, MovementFsmController movementFsm, EventBus eventBus)
    {
        var attackProvider = new AttackCardSelectionProvider(progressionService, AbilityChoices);


        var compositeProvider = new CompositeSelectionProvider(attackProvider);

        _controller = new SelectionMenuPresenter(eventBus, SelectionMenu, compositeProvider);
    }

    private void OnDestroy()
    {
        _controller?.Dispose();
    }
}

