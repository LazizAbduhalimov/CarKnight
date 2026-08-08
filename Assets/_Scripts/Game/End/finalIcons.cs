using UnityEngine;
using UnityEngine.UI;

public class finalIcons : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private Text _name;


    public void ShowIcon(AttackSO config)
    {
        _icon.sprite = config.AbilityIcon;
        _name.text = config.AbilityName;
    }
}
