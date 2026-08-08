using PrimeTween;
using UnityEngine;
using Zenject;

public class FullScreenDamageEffect : MonoBehaviour
{
    [SerializeField] private CanvasGroup _damageOverlay;
    [SerializeField] private float _fadeDuration = 0.5f;
    private Sequence? _damageEffectSequence;
    private HealthComponent _healthComponent;

    [Inject]
    private void Construct(HealthComponent healthComponent)
    {
        _healthComponent = healthComponent;
    }

    private void OnEnable()
    {
        _healthComponent.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        _healthComponent.OnHealthChanged -= OnHealthChanged;
    }

    private void Start()
    {
        if (_damageOverlay == null)
        {
            Debug.LogError("Damage Overlay reference is missing in FullScreenDamageEffect");
            return;
        }
        
        _damageOverlay.alpha = 0f;
    }

    public void TriggerDamageEffect()
    {
        if (_damageOverlay == null)
        {
            return;
        }

        float halfDuration = _fadeDuration * 0.5f;
        float currentAlpha = Mathf.Clamp01(_damageOverlay.alpha);
        float fadeInDuration = halfDuration * (1f - currentAlpha);
        float fadeOutDuration = halfDuration * 1f;

        _damageEffectSequence?.Stop();

        var sequence = Sequence.Create();

        if (fadeInDuration > 0f)
        {
            sequence.Chain(Tween.Alpha(_damageOverlay, currentAlpha, 1f, fadeInDuration, ease: Ease.OutCubic));
        }
        else
        {
            _damageOverlay.alpha = 1f;
        }

        if (fadeOutDuration > 0f)
        {
            sequence.Chain(Tween.Alpha(_damageOverlay, 1f, 0f, fadeOutDuration, ease: Ease.InCubic));
        }
        else
        {
            _damageOverlay.alpha = 0f;
        }

        _damageEffectSequence = sequence;
    }

    private void OnHealthChanged(int oldHealth, int currentHealth, int maxHealth)
    {
        TriggerDamageEffect();
    }
}
