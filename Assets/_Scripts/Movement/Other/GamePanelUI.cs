using UnityEngine;
using System.Collections;
using System;

public class GamePanelUI : MonoBehaviour
{
    [Header("Tab function")]
    [SerializeField] private RectTransform _panelRect;
    [SerializeField] private Vector2 _activePos;
    [SerializeField] private Vector2 _inactivePos;
    [SerializeField] private float _panelMoveDuration = 0.35f;
    private bool _isActive = false;
    private Coroutine _moveCoroutine;

    [Header("Switch menu")]
    [SerializeField] private GameObject[] _panels;
    [SerializeField] private float _panelFadeDuration = 0.2f;
    private GameObject _activePanel;
    private bool _isSwitching = false;

    [Header("Panel Versions")]
    [SerializeField] private GameObject[] _panelsVersion;
    private GameObject _activeVersion;
    private bool _isSwitchingVersion = false;

    private float _timeScale;
    public Action<bool> tabChange;

    [Header("Easing")]
    [SerializeField]
    private AnimationCurve _easeCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    private void Start()
    {
        _timeScale = Time.timeScale;
        _panelRect.anchoredPosition = _inactivePos;

        foreach (var panel in _panels)
        {
            EnsureCanvasGroup(panel);
            panel.SetActive(false);
        }
        if (_panels.Length > 0)
        {
            _panels[0].SetActive(true);
            _panels[0].GetComponent<CanvasGroup>().alpha = 1f;
            _activePanel = _panels[0];
        }

        foreach (var version in _panelsVersion)
        {
            EnsureCanvasGroup(version);
            version.SetActive(false);
        }
        if (_panelsVersion.Length > 0)
        {
            _panelsVersion[0].SetActive(true);
            _panelsVersion[0].GetComponent<CanvasGroup>().alpha = 1f;
            _activeVersion = _panelsVersion[0];
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            TogglePanel();
    }

    public void TogglePanel()
    {
        _isActive = !_isActive;
        tabChange?.Invoke(_isActive);

        Time.timeScale = _isActive ? 0f : _timeScale;

        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveRoutine(_isActive ? _activePos : _inactivePos));
    }

    private IEnumerator MoveRoutine(Vector2 targetPos)
    {
        Vector2 startPos = _panelRect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < _panelMoveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / _panelMoveDuration);
            float curved = _easeCurve.Evaluate(t);
            _panelRect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, curved);
            yield return null;
        }

        _panelRect.anchoredPosition = targetPos;
        _moveCoroutine = null;
    }

    public void SetActivePanel(int panelsNumber)
    {
        if (panelsNumber >= _panels.Length || _panels[panelsNumber] == null)
        {
            Debug.LogError($"There isnt such panel {panelsNumber}");
            return;
        }

        // ”же активна или идЄт переключение Ч игнорируем
        if (_panels[panelsNumber] == _activePanel) return;
        if (_isSwitching) return;

        StartCoroutine(SwitchPanelRoutine(
            _panels, _activePanel, panelsNumber,
            result => _activePanel = result,
            () => _isSwitching = false));
        _isSwitching = true;
    }

    public void SetPanelVersion(int versionNumber)
    {
        if (versionNumber >= _panelsVersion.Length || _panelsVersion[versionNumber] == null)
        {
            Debug.LogError($"There isnt that panel version {versionNumber}");
            return;
        }

        if (_panelsVersion[versionNumber] == _activeVersion) return;
        if (_isSwitchingVersion) return;

        StartCoroutine(SwitchPanelRoutine(
            _panelsVersion, _activeVersion, versionNumber,
            result => _activeVersion = result,
            () => _isSwitchingVersion = false));
        _isSwitchingVersion = true;
    }

    private IEnumerator SwitchPanelRoutine(GameObject[] panels, GameObject currentPanel, int targetIndex, Action<GameObject> onComplete, Action onFinished)
    {
        GameObject nextPanel = panels[targetIndex];

        // √асим все лишние панели мгновенно
        foreach (var panel in panels)
        {
            if (panel != currentPanel && panel != nextPanel)
            {
                panel.GetComponent<CanvasGroup>().alpha = 0f;
                panel.SetActive(false);
            }
        }

        // Fade out текущей
        if (currentPanel != null)
        {
            var cg = currentPanel.GetComponent<CanvasGroup>();
            yield return StartCoroutine(FadeCanvasGroup(cg, 1f, 0f, _panelFadeDuration));
            currentPanel.SetActive(false);
        }

        // Fade in новой
        nextPanel.SetActive(true);
        var nextCg = nextPanel.GetComponent<CanvasGroup>();
        nextCg.alpha = 0f;
        yield return StartCoroutine(FadeCanvasGroup(nextCg, 0f, 1f, _panelFadeDuration));

        onComplete?.Invoke(nextPanel);
        onFinished?.Invoke();
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curved = _easeCurve.Evaluate(t);
            cg.alpha = Mathf.LerpUnclamped(from, to, curved);
            yield return null;
        }

        cg.alpha = to;
    }

    private void EnsureCanvasGroup(GameObject obj)
    {
        if (obj != null && obj.GetComponent<CanvasGroup>() == null)
            obj.AddComponent<CanvasGroup>();
    }
}