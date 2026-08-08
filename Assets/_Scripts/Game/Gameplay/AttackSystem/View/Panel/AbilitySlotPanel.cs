using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class AbilitySlotPanel : MonoBehaviour
{
    [SerializeField] private AbilitySlotItem SlotItemPrefab;
    [SerializeField] private List<AbilitySlotItem> SlotItems;

    private readonly List<AbilitySlotItemPresenter> _presenters = new();
    private Attacker _attacker;

    public int SlotCount => SlotItems.Count;

    [Inject]
    private void Construct(Attacker attacker)
    {
        _attacker = attacker;
        if (_attacker != null)
        {
            _attacker.OwnedAttacksChanged += RebuildPresenters;
        }

        RebuildPresenters();
    }

    private void OnEnable()
    {
        RebuildPresenters();
    }

    private void Update()
    {
        for (var i = 0; i < _presenters.Count; i++)
        {
            _presenters[i].Tick();
        }
    }

    private void OnDestroy()
    {
        if (_attacker != null)
        {
            _attacker.OwnedAttacksChanged -= RebuildPresenters;
        }
    }

    private void OnValidate()
    {
        if (SlotItems == null)
        {
            SlotItems = new List<AbilitySlotItem>();
        }

        if (SlotItems.Count == 0)
        {
            SlotItems = GetComponentsInChildren<AbilitySlotItem>().ToList();
        }
    }

    public void RebuildPresenters()
    {
        _presenters.Clear();

        if (_attacker == null)
        {
            return;
        }

        var ownedAttacks = _attacker.OwnedAttacks;
        if (ownedAttacks == null)
        {
            return;
        }

        EnsureSlotListInitialized();
        EnsureSlotCapacity(ownedAttacks.Count);
        ResetSlots();

        var count = Mathf.Min(SlotItems.Count, ownedAttacks.Count);
        for (var i = 0; i < count; i++)
        {
            var slot = SlotItems[i];
            var ownedAttack = ownedAttacks[i];
            if (slot == null)
            {
                continue;
            }

            _presenters.Add(new AbilitySlotItemPresenter(slot, ownedAttack));
        }
    }

    private void ResetSlots()
    {
        for (var i = 0; i < SlotItems.Count; i++)
        {
            var slot = SlotItems[i];
            if (slot == null)
            {
                continue;
            }

            slot.SetIcon(null);
            slot.SetCooldownFill(0f);
            slot.SetCooldownText(string.Empty);
        }
    }

    private void EnsureSlotListInitialized()
    {
        if (SlotItems == null)
        {
            SlotItems = new List<AbilitySlotItem>();
        }
    }

    private void EnsureSlotCapacity(int requiredCount)
    {
        if (requiredCount <= 0)
        {
            return;
        }

        SlotItems.RemoveAll(item => item == null);

        if (SlotItems.Count >= requiredCount)
        {
            return;
        }

        if (SlotItemPrefab == null)
        {
            Debug.LogWarning("AbilitySlotPanel cannot create new slots because SlotItemPrefab is null.", this);
            return;
        }

        while (SlotItems.Count < requiredCount)
        {
            var slotInstance = Instantiate(SlotItemPrefab, transform);
            slotInstance.gameObject.name = $"{SlotItemPrefab.name}_{SlotItems.Count}";
            SlotItems.Add(slotInstance);
        }
    }

    private void SetSpriteInSlot(int index, Sprite icon)
    {
        if (index < 0 || index >= SlotItems.Count)
        {
            Debug.LogWarning($"Index {index} is out of range for ability slots.");
            return;
        }
        SlotItems[index].SetIcon(icon);
    }
}
