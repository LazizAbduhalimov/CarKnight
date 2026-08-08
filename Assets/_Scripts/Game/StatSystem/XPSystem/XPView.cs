using TMPro;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;

public class XPView : MonoBehaviour 
{
    [SerializeField] private Image _xpBarImage; // just fill it for now
    // [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _xpText;
    [SerializeField] private float _fillAnimationDuration = 0.25f;
    [SerializeField] private Ease _fillAnimationEase = Ease.OutCubic;

    [Header("Feedback")]
    [SerializeField] private MMF_Player _xpGainFeedback;

    private Tween _xpFillTween;

    public void UpdateXP(int currentXP, int xpToNextLevel, int level)
    {
        _xpGainFeedback?.PlayFeedbacks();
        Debug.Log($"Updating XP View: Level {level}, XP {currentXP}/{xpToNextLevel}");

        float targetFillAmount = xpToNextLevel > 0
            ? Mathf.Clamp01((float)currentXP / xpToNextLevel)
            : 0f;

        _xpFillTween.Stop();
        _xpFillTween = Tween.Custom(
            this,
            _xpBarImage.fillAmount,
            targetFillAmount,
            _fillAnimationDuration,
            ease: _fillAnimationEase,
            onValueChange: (target, fillAmount) => target._xpBarImage.fillAmount = fillAmount);

        _xpText.text = $"<size=16>XP</size>\n{currentXP}/{xpToNextLevel}";
    }

    private void OnDisable()
    {
        _xpFillTween.Stop();
    }
}
