using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpiralHomingMissile : MissileBase
{
    [Header("Flight")]
    public Vector3 TargetPosition;
    [MinValue(0.1f)] public float MinFlightTime = 0.8f;

    private Rigidbody _rb;
    private Vector3 _startPosition;
    private Vector3 _flatForward;
    private Vector3 _flatRight;
    private float _flightTime;
    private float _elapsed;
    private float _arcHeight;
    private float _maxSpiralRadius;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _elapsed = 0f;
    }

    public override void Init(float speed, float lifeTime = 5f)
    {
        base.Init(speed, lifeTime);

        _startPosition = transform.position;
        _elapsed = 0f;

        var toTarget = TargetPosition - _startPosition;
        var distance = toTarget.magnitude;
        _flightTime = Mathf.Max(MinFlightTime, distance / Mathf.Max(0.01f, Speed));

        var flatToTarget = toTarget.WithY(0f);
        if (flatToTarget.sqrMagnitude < 0.0001f)
        {
            flatToTarget = transform.forward.WithY(0f);
            if (flatToTarget.sqrMagnitude < 0.0001f)
            {
                flatToTarget = Vector3.forward;
            }
        }

        _flatForward = flatToTarget.normalized;
        _flatRight = Vector3.Cross(Vector3.up, _flatForward).normalized;

        _arcHeight = Mathf.Clamp(distance * 0.28f, 1f, 9f);
        _maxSpiralRadius = Mathf.Clamp(distance * 0.08f, 0.25f, 2.5f);
    }

    private void FixedUpdate()
    {
        if (_rb == null)
        {
            return;
        }

        _elapsed += Time.fixedDeltaTime;
        var t = Mathf.Clamp01(_elapsed / Mathf.Max(0.01f, _flightTime));

        var position = EvaluatePosition(t);
        var direction = position - _rb.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            _rb.MoveRotation(Quaternion.LookRotation(direction.normalized, Vector3.up));
        }

        _rb.MovePosition(position);

        if (t >= 1f)
        {
            MissileColisionProvider.Boom();
        }
    }

    private Vector3 EvaluatePosition(float t)
    {
        var center = Vector3.Lerp(_startPosition, TargetPosition, t);
        var arcOffset = Vector3.up * (Mathf.Sin(t * Mathf.PI) * _arcHeight);
        var spiralOffset = Vector3.zero;

        if (t <= 0.5f)
        {
            var u = t / 0.5f;
            var angle = u * Mathf.PI * 2f; // full 360 during upward phase
            var radius = Mathf.Lerp(_maxSpiralRadius, 0f, u);
            spiralOffset = (_flatRight * Mathf.Cos(angle) + _flatForward * Mathf.Sin(angle)) * radius;
        }

        return center + arcOffset + spiralOffset;
    }
}
