using UnityEngine;

public class SelectionMenuMb : MonoBehaviour
{
    public AbilityChoiceButton[] AbilityChoiceButtons;

    public int SelectedAbilityIndex { get; private set; } = -1;

    private void OnValidate()
    {
        if (AbilityChoiceButtons == null || AbilityChoiceButtons.Length == 0)
        {
            AbilityChoiceButtons = GetComponentsInChildren<AbilityChoiceButton>();
        }
    }

    private void OnEnable() {
        Time.timeScale = 0f; 
    }

    private void OnDisable() {
        Time.timeScale = 1f; 
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectedAbilityIndex--;
            SelectAbility();
        }    
        else if (Input.GetKeyDown(KeyCode.D ) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            SelectedAbilityIndex++;
            SelectAbility();
        }
        else if (Input.GetKeyDown(KeyCode.Return) && SelectedAbilityIndex != -1)
        {
            Debug.Log($"Ability at index {SelectedAbilityIndex} confirmed.");
            gameObject.SetActive(false);
        }
    }

    private void SelectAbility()
    {
        SelectedAbilityIndex = Mathf.Clamp(SelectedAbilityIndex, 0, AbilityChoiceButtons.Length - 1);
        Debug.Log($"Selected ability index: {SelectedAbilityIndex}");

        for (int i = 0; i < AbilityChoiceButtons.Length; i++)
        {
            var button = AbilityChoiceButtons[i];
            var isSelected = i == SelectedAbilityIndex;
            
            button.transform.localScale = Vector3.one; 
            if (isSelected)
                button.transform.localScale *= 1.2f; 
        }
    }
}