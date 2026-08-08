using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CarAntiBugSystem : MonoBehaviour
{
    [Header("Dependencies")]
    private Rigidbody _rb;
    private GravitationSetter _gravSetter;
    private MovementFsmController _fsmController;

    [Header("Stuck Settings (Auto)")]
    [SerializeField] private float _stuckSpeedThreshold = 0.6f;
    [SerializeField] private float _timeToAction = 2.0f;

    [Header("History Settings")]
    [SerializeField] private float _saveInterval = 2.0f;
    [SerializeField] private int _historySize = 10;

    [Header("Rotate Settings")]
    [SerializeField] private float _rotateLimit = 70f;
    [SerializeField] private float _rotationCorrectionSpeed = 5f;

    [Header("Manual Reset Settings")]
    [SerializeField] public bool _canPress;
    [SerializeField] private KeyCode _manualResetKey = KeyCode.R;
    [SerializeField] private float _holdTimeToReset = 1.5f;
    private float _manualResetTimer;

    [Header("Visual & Layers")]
    [SerializeField] private GameObject _ghostViev;
    [SerializeField] private float _ghostDuration = 3f;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _checkRadius = 3.5f;

    [Header("Reset UI")]
    [SerializeField] private GameObject _resetCanvas;
    [SerializeField] private UnityEngine.UI.Image _resetFillImage;
    private Camera _mainCamera;

    private Queue<Vector3> _positionHistory = new Queue<Vector3>();
    private float _stuckTimer;
    private bool _isGhost;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _gravSetter = GetComponent<GravitationSetter>();
        _fsmController = GetComponent<MovementFsmController>();
        _mainCamera = Camera.main;

        if (_resetCanvas != null)
            _resetCanvas.SetActive(false);

        InvokeRepeating(nameof(RecordHistory), _saveInterval, _saveInterval);
    }

    void Update()
    {
        HandleStuckDetection();
        HandleManualReset();
        HandleCanvasFaceCamera();
    }

    private void FixedUpdate()
    {
        HandleRotateFix();
    }

    private void HandleCanvasFaceCamera()
    {
        if (_resetCanvas == null || !_resetCanvas.activeSelf || _mainCamera == null) return;

        _resetCanvas.transform.rotation = Quaternion.LookRotation(
            _resetCanvas.transform.position - _mainCamera.transform.position
        );
    }

    private void HandleRotateFix()
    {
        float angleZ = Mathf.Abs(NormalizeAngle(transform.eulerAngles.z));
        float angleX = Mathf.Abs(NormalizeAngle(transform.eulerAngles.x));

        if (angleZ > _rotateLimit || angleX > _rotateLimit)
        {
            Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            _rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * _rotationCorrectionSpeed));
        }
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180) angle -= 360;
        return angle;
    }

    private void RecordHistory()
    {
        if (_gravSetter.GroundCheck() && !_isGhost && _rb.linearVelocity.magnitude > 2f)
        {
            _positionHistory.Enqueue(transform.position);

            if (_positionHistory.Count > _historySize)
                _positionHistory.Dequeue();
        }
    }

    private void HandleStuckDetection()
    {
        if (_isGhost) return;

        bool isTryingToMove = Mathf.Abs(_fsmController.ForwardInput) > 0.1f;
        bool isStopped = _rb.linearVelocity.magnitude < _stuckSpeedThreshold;

        if (isTryingToMove && isStopped)
        {
            _stuckTimer += Time.deltaTime;
            if (_stuckTimer >= _timeToAction) ResolveStuck();
        }
        else
        {
            _stuckTimer = 0;
        }
    }

    private void HandleManualReset()
    {
        if (!_canPress)
        {
            _manualResetTimer = 0;
            if (_resetCanvas != null && _resetCanvas.activeSelf)
                _resetCanvas.SetActive(false);
            return;
        }

        if (Input.GetKey(_manualResetKey))
        {
            // ¬ключаем канвас при первом нажатии
            if (_resetCanvas != null && !_resetCanvas.activeSelf)
            {
                _resetCanvas.SetActive(true);
                if (_resetFillImage != null)
                    _resetFillImage.fillAmount = 0f;
            }

            _manualResetTimer += Time.deltaTime;

            if (_resetFillImage != null)
                _resetFillImage.fillAmount = Mathf.Clamp01(_manualResetTimer / _holdTimeToReset);

            if (_manualResetTimer >= _holdTimeToReset)
            {
                _manualResetTimer = 0;

                if (_resetFillImage != null)
                    _resetFillImage.fillAmount = 0f;

                if (_resetCanvas != null)
                    _resetCanvas.SetActive(false);

                TeleportToSafety();
            }
        }
        else
        {
            //  нопка отпущена Ч выключаем канвас и сбрасываем таймер
            _manualResetTimer = 0;

            if (_resetCanvas != null && _resetCanvas.activeSelf)
                _resetCanvas.SetActive(false);
        }
    }

    private void ResolveStuck()
    {
        _stuckTimer = 0;
        if (AreEnemiesNearby()) StartCoroutine(GhostModeRoutine());
        else TeleportToSafety();
    }

    private void TeleportToSafety()
    {
        if (_positionHistory.Count == 0)
        {
            Debug.LogWarning("CarAntiBugSystem: истори€ позиций пуста€, телепорт невозможен");
            return;
        }

        Vector3 safePos = _positionHistory.Peek();

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;

        transform.position = safePos + Vector3.up * 1.2f;

        _positionHistory.Clear();

        StartCoroutine(EnablePhysicsRoutine());
        StartCoroutine(GhostModeRoutine(2.0f));
    }

    private IEnumerator EnablePhysicsRoutine()
    {
        yield return new WaitForFixedUpdate();
        _rb.isKinematic = false;
    }

    private IEnumerator GhostModeRoutine(float customDuration = 0)
    {
        _isGhost = true;
        float duration = customDuration > 0 ? customDuration : _ghostDuration;

        int pLayer = GetLayerFromMask(_playerLayer);
        int eLayer = GetLayerFromMask(_enemyLayer);

        Physics.IgnoreLayerCollision(pLayer, eLayer, true);
        if (_ghostViev) _ghostViev.SetActive(true);

        yield return new WaitForSeconds(duration);

        while (AreEnemiesNearby()) yield return new WaitForSeconds(0.5f);

        Physics.IgnoreLayerCollision(pLayer, eLayer, false);
        if (_ghostViev) _ghostViev.SetActive(false);
        _isGhost = false;
    }

    private bool AreEnemiesNearby()
    {
        return Physics.OverlapSphere(transform.position, _checkRadius, _enemyLayer).Length > 0;
    }

    private int GetLayerFromMask(LayerMask mask)
    {
        int val = mask.value;
        if (val == 0) return 0;
        int res = 0;
        while ((val >>= 1) != 0) res++;
        return res;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _checkRadius);
    }
}