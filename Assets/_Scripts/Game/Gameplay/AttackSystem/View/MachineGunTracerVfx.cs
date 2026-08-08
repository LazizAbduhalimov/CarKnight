using System.Collections;
using UnityEngine;

public sealed class MachineGunTracerVfx : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _animatedTransform;
    [SerializeField] private bool _animateTransform = true;

    private Coroutine _playCoroutine;
    private TrailRenderer[] _trailRenderers;

    private void Awake()
    {
        if (_lineRenderer == null)
        {
            _lineRenderer = GetComponentInChildren<LineRenderer>();
        }

        _trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
    }

    private void OnDisable()
    {
        if (_playCoroutine != null)
        {
            StopCoroutine(_playCoroutine);
            _playCoroutine = null;
        }

        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = 0;
        }
    }

    public void Play(Vector3 start, Vector3 end, float travelDuration, float lifetime)
    {
        if (_playCoroutine != null)
        {
            StopCoroutine(_playCoroutine);
        }

        ClearTrails();
        _playCoroutine = StartCoroutine(PlayRoutine(start, end, travelDuration, lifetime));
    }

    private IEnumerator PlayRoutine(Vector3 start, Vector3 end, float travelDuration, float lifetime)
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, start);
            _lineRenderer.SetPosition(1, end);
        }

        var mover = _animatedTransform != null ? _animatedTransform : transform;
        mover.position = start;

        var direction = end - start;
        if (direction.sqrMagnitude > 0.0001f)
        {
            mover.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        if (_animateTransform)
        {
            var duration = Mathf.Max(0.001f, travelDuration);
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                mover.position = Vector3.Lerp(start, end, t);
                yield return null;
            }
        }
        else
        {
            mover.position = end;
        }

        var remainingLifetime = Mathf.Max(0f, lifetime - travelDuration);
        if (remainingLifetime > 0f)
        {
            yield return new WaitForSeconds(remainingLifetime);
        }

        gameObject.SetActive(false);
        _playCoroutine = null;
    }

    private void ClearTrails()
    {
        if (_trailRenderers == null)
        {
            return;
        }

        for (var i = 0; i < _trailRenderers.Length; i++)
        {
            var trail = _trailRenderers[i];
            if (trail != null)
            {
                trail.Clear();
            }
        }
    }
}
