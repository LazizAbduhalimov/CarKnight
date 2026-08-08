using UnityEngine;
using UnityEngine.UI;

public class CombinariumSlotUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Text _nameText;

    private Button _button;
    private AttackComboDefinition _comboDefinition;
    private CombinariumInfoPanel _infoPanel;
    private bool _openOnStart;
    public void DrawSlot(AttackComboDefinition combo, CombinariumInfoPanel infoPanel, bool isOpen)
    {
        _comboDefinition = combo;
        _button = GetComponent<Button>();
        _infoPanel = infoPanel;

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(InfoShow);



        UpdateSlot(combo, isOpen);
    }

    public void UpdateSlot(AttackComboDefinition combo, bool isOpen)
    {
        _openOnStart = isOpen;

        _comboDefinition = combo;

        _icon.sprite = _comboDefinition.ComboSprite;
        _nameText.text = combo.DisplayName;

    }

    public void InfoShow()
    {
        if (_infoPanel != null) _infoPanel.ShowPanel(_comboDefinition, _openOnStart);
    }
}
