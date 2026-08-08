using UnityEngine;

public class UIShiftAwayFromMouse : MonoBehaviour
{
    [SerializeField] private RectTransform _visualTarget;
    [SerializeField] private float _maxOffset = 6f;
    [SerializeField] private float _followSpeed = 12f;
    [SerializeField] private bool _useUnscaledTime = true;

    private RectTransform _rectTransform;
    private Canvas _canvas;
    private Vector2 _defaultAnchoredPosition;

    private void Awake()
    {
        _rectTransform = transform as RectTransform;

        if (_visualTarget == null)
        {
            _visualTarget = _rectTransform;
        }

        if (_visualTarget != null)
        {
            _defaultAnchoredPosition = _visualTarget.anchoredPosition;
        }

        _canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        if (_visualTarget != null)
        {
            _defaultAnchoredPosition = _visualTarget.anchoredPosition;
        }
    }

    private void OnDisable()
    {
        if (_visualTarget != null)
        {
            _visualTarget.anchoredPosition = _defaultAnchoredPosition;
        }
    }

    private void Update()
    {
        if (_visualTarget == null)
        {
            return;
        }

        var deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        var targetPosition = _defaultAnchoredPosition + GetOffsetAwayFromMouse();
        _visualTarget.anchoredPosition = Vector2.Lerp(_visualTarget.anchoredPosition, targetPosition, deltaTime * _followSpeed);
    }

    private Vector2 GetOffsetAwayFromMouse()
    {
        var camera = GetCanvasCamera();
        var screenPosition = RectTransformUtility.WorldToScreenPoint(camera, _visualTarget.position);
        var direction = screenPosition - (Vector2)Input.mousePosition;

        if (direction.sqrMagnitude <= 0.0001f)
        {
            return Vector2.zero;
        }

        return direction.normalized * _maxOffset;
    }

    private Camera GetCanvasCamera()
    {
        if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return null;
        }

        return _canvas.worldCamera;
    }
}
