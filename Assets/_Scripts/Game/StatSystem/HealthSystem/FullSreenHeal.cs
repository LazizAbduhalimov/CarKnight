using PrimeTween;
using UnityEngine;
using Zenject;

public class FullSreenHeal : MonoBehaviour
{
    [SerializeField] private CanvasGroup _healOverlay;
    [SerializeField] private float _fadeDuration = 0.5f;
    private Sequence? _healEffectSequence;
    private HealthComponent _healthComponent;

    [Inject]
    private void Construct(HealthComponent healthComponent)
    {
        _healthComponent = healthComponent;
    }

    private void OnEnable()
    {
        _healthComponent.OnHealthHeal += HealEffect;
    }

    private void OnDisable()
    {
        _healthComponent.OnHealthHeal -= HealEffect;
    }

    private void Start()
    {
        if (_healOverlay == null)
        {
            Debug.LogError("Missin HealthHealOverlay");
            return;
        }

        _healOverlay.alpha = 0f;
    }

    public void HealEffect()
    {
        if (_healOverlay == null)
        {
            return;
        }

        float halfDuration = _fadeDuration * 0.5f;
        float currentAlpha = Mathf.Clamp01(_healOverlay.alpha);
        float fadeInDuration = halfDuration * (1f - currentAlpha);
        float fadeOutDuration = halfDuration * 1f;

        _healEffectSequence?.Stop();

        var sequence = Sequence.Create();

        if (fadeInDuration > 0f)
        {
            sequence.Chain(Tween.Alpha(_healOverlay, currentAlpha, 1f, fadeInDuration, ease: Ease.OutCubic));
        }
        else
        {
            _healOverlay.alpha = 1f;
        }

        if (fadeOutDuration > 0f)
        {
            sequence.Chain(Tween.Alpha(_healOverlay, 1f, 0f, fadeOutDuration, ease: Ease.InCubic));
        }
        else
        {
            _healOverlay.alpha = 0f;
        }

        _healEffectSequence = sequence;
    }
}
