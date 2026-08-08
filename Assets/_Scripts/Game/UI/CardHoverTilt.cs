using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverTilt : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform _hoverArea;
    [SerializeField] private RectTransform _visualTarget;
    [SerializeField] private float _maxTiltAngle = 12f;
    [SerializeField] private float _followSpeed = 10f;
    [SerializeField] private bool _useUnscaledTime = true;

    private bool _isHovered;
    private Quaternion _defaultRotation;
    private Camera _eventCamera;

    private void Awake()
    {
        if (_hoverArea == null)
        {
            _hoverArea = transform as RectTransform;
        }

        if (_visualTarget == null)
        {
            _visualTarget = transform as RectTransform;
        }

        if (_visualTarget != null)
        {
            _defaultRotation = _visualTarget.localRotation;
        }

        if (_hoverArea != null && _visualTarget != null && ReferenceEquals(_hoverArea, _visualTarget))
        {
            Debug.LogWarning(
                $"{nameof(CardHoverTilt)} on '{name}' uses the same RectTransform for hover and tilt. " +
                "Use a stable parent for hover and a child for visual tilt to avoid Enter/Exit flicker on edges.",
                this);
        }
    }

    private void Update()
    {
        if (_visualTarget == null)
        {
            return;
        }

        var deltaTime = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        var targetRotation = _isHovered ? GetHoverRotation() : _defaultRotation;
        _visualTarget.localRotation = Quaternion.Slerp(_visualTarget.localRotation, targetRotation, deltaTime * _followSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovered = true;
        _eventCamera = eventData != null ? eventData.enterEventCamera : null;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
    }

    private Quaternion GetHoverRotation()
    {
        if (_hoverArea == null || _visualTarget == null)
        {
            return _defaultRotation;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_hoverArea, Input.mousePosition, _eventCamera, out var localPoint))
        {
            return _defaultRotation;
        }

        var rect = _hoverArea.rect;
        var normalizedX = rect.width <= 0.0001f ? 0f : Mathf.Clamp(localPoint.x / (rect.width * 0.5f), -1f, 1f);
        var normalizedY = rect.height <= 0.0001f ? 0f : Mathf.Clamp(localPoint.y / (rect.height * 0.5f), -1f, 1f);

        var tiltX = -normalizedY * _maxTiltAngle;
        var tiltY = normalizedX * _maxTiltAngle;
        return _defaultRotation * Quaternion.Euler(tiltX, tiltY, 0f);
    }
}
