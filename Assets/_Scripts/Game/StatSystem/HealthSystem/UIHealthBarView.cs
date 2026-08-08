using MoreMountains.Feedbacks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIHealthBarView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image HealthBar;
    [SerializeField] private Image DelayedHealthBar;
    [SerializeField] private TMP_Text _healthText;

    [Header("Main Fill Animation")]
    [SerializeField] private float _smoothDuration = 0.2f;
    [SerializeField] private Ease _smoothEase = Ease.OutCubic;

    [Header("Delayed Fill Animation")]
    [SerializeField] private float _delayedFillStartDelay = 0.35f;
    [SerializeField] private float _delayedSmoothDuration = 0.25f;
    [SerializeField] private Ease _delayedSmoothEase = Ease.OutCubic;

    [Header("Feedback")]
    [SerializeField] private MMF_Player _damageFeedback;
    
    private HealthComponent _healthComponent;
    private Tween _healthFillTween;
    private Tween _delayedHealthFillTween;
    private Tween _delayedHealthDelayTween;

    [Inject]
    private void Construct(HealthComponent healthComponent)
    {
        _healthComponent = healthComponent;
        SetFillInstant(_healthComponent.CurrentHealth, _healthComponent.MaxHealth);
    }

    private void OnEnable()
    {
        _healthComponent.OnHealthChanged += UpdateHealthBar;
        SetFillInstant(_healthComponent.CurrentHealth, _healthComponent.MaxHealth);
    }

    private void OnDisable()
    {
        _healthComponent.OnHealthChanged -= UpdateHealthBar;
        _healthFillTween.Stop();
        _delayedHealthFillTween.Stop();
        _delayedHealthDelayTween.Stop();
    }

    private void UpdateHealthBar(int oldHealth, int currentHealth, int maxHealth)
    {
        if (HealthBar == null)
        {
            return;
        }

        if (oldHealth > currentHealth)
        {
            _damageFeedback?.PlayFeedbacks();
        }

        float targetFill = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        _healthText.text = $"{currentHealth}/{maxHealth}";
        _healthFillTween.Stop();
        _healthFillTween = Tween.Custom(
            this,
            HealthBar.fillAmount,
            targetFill,
            _smoothDuration,
            ease: _smoothEase,
            onValueChange: (target, value) => target.HealthBar.fillAmount = value);

        UpdateDelayedFill(oldHealth, currentHealth, targetFill);
    }

    private void UpdateDelayedFill(int oldHealth, int currentHealth, float targetFill)
    {
        if (DelayedHealthBar == null)
        {
            return;
        }

        _delayedHealthDelayTween.Stop();
        _delayedHealthFillTween.Stop();

        bool isDamage = currentHealth < oldHealth;
        if (isDamage && _delayedFillStartDelay > 0f)
        {
            _delayedHealthDelayTween = Tween.Delay(_delayedFillStartDelay, () => StartDelayedFillTween(targetFill));
            return;
        }

        StartDelayedFillTween(targetFill);
    }

    private void StartDelayedFillTween(float targetFill)
    {
        if (DelayedHealthBar == null)
        {
            return;
        }

        _delayedHealthFillTween.Stop();
        _delayedHealthFillTween = Tween.Custom(
            this,
            DelayedHealthBar.fillAmount,
            targetFill,
            _delayedSmoothDuration,
            ease: _delayedSmoothEase,
            onValueChange: (target, value) => target.DelayedHealthBar.fillAmount = value);
    }

    private void SetFillInstant(int currentHealth, int maxHealth)
    {
        float fill = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        if (HealthBar != null)
        {
            HealthBar.fillAmount = fill;
        }

        if (DelayedHealthBar != null)
        {
            DelayedHealthBar.fillAmount = fill;
        }

        if (_healthText != null)
        {
            _healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

}
