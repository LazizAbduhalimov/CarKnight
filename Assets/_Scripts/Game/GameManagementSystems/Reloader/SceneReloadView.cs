using UnityEngine;
using Zenject;

public class SceneReloadView : MonoBehaviour
{
    [SerializeField] GameObject ReloadButton;
    [HideInInspector] public PlayerUnit PlayerUnit;

    [Inject]
    private void Construct(PlayerUnit playerUnit)
    {
        PlayerUnit = playerUnit;
        playerUnit.HealthComponent.OnDeath += ShowReloadButtonOnDeath;
    }

    private void OnDestroy() 
    {
        PlayerUnit.HealthComponent.OnDeath -= ShowReloadButtonOnDeath;
    }

    private void ShowReloadButtonOnDeath()
    {
        ReloadButton.SetActive(true);
    }
}