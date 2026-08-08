using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CombinariumInfoPanel : MonoBehaviour
{
    [SerializeField] private GamePanelUI _gamePanelUI;
    [SerializeField] private Image _icon;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _hintText;

    [SerializeField] private List<Image> _sprites = new List<Image>();

    [SerializeField] private GameObject _hintPanel;
    [SerializeField] private GameObject _iconPanel;

    private void OnEnable()
    {
        _gamePanelUI.tabChange += CloseInfo;
    }
    private void OnDisable()
    {
        _gamePanelUI.tabChange -= CloseInfo;

    }
    private void CloseInfo(bool isActivedPanel)
    {
        if (!isActivedPanel)
        {
            ClosePanel();
        }

    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
    public void ShowPanel(AttackComboDefinition attackComboDefinition, bool isClosed)
    {
        _icon.sprite = attackComboDefinition.ComboSprite;
        _nameText.text = $"Умение: {attackComboDefinition.DisplayName}";
        
        gameObject.SetActive(true);

        if (!isClosed)
        {
            _hintPanel.SetActive(true);
            _iconPanel.SetActive(false);

            _hintText.text = attackComboDefinition.hintText;
        }
        else
        {
            _hintPanel.SetActive(false);
            _iconPanel.SetActive(true);

            for (int i = 0; i < _sprites.Count; i++)
            {
                if (i < attackComboDefinition.Requirements.Count)
                {
                    _sprites[i].gameObject.SetActive(true);
                    _sprites[i].sprite = attackComboDefinition.Requirements[i].AttackConfig.AbilityIcon;
                }
                else
                {
                    _sprites[i].gameObject.SetActive(false);
                }
            }
        }
      
    }

}
