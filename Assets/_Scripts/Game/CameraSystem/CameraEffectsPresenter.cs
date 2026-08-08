using System;
using UnityEngine;
using Zenject;

public class CameraEffectsPresenter : MonoBehaviour
{
    private CameraBox _cameraBox;
    private HealthComponent _healthComponent;

    [Inject]
    private void Construct(
        CameraBox cameraBox,
        HealthComponent healthComponent
        )
    {
        _cameraBox = cameraBox;
        _healthComponent = healthComponent;
    }

    private void OnEnable()
    {
        _healthComponent.OnDeath += OnDeath;
        _healthComponent.OnHealthChanged += OnHit;
    }

    private void OnDisable()
    {
        _healthComponent.OnDeath -= OnDeath;
        _healthComponent.OnHealthChanged -= OnHit;
    }

    private void OnHit(int oldHealth, int currentHealth, int maxHealth)
    {
        int damage = oldHealth - currentHealth;
        if (damage > 0)
        {
            _cameraBox.ImpulseMM?.PlayFeedbacks();
        }
    }

    private void OnDeath()
    {
        _cameraBox.ToGrayMM?.PlayFeedbacks();
    }
}