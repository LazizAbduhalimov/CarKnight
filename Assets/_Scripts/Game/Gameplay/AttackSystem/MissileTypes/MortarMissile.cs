using PoolSystem.Alternative;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MortarMissile : MissileBase
{
    private const float RiseDuration = 0.35f;
    private const float FallSpeed = 35f;

    [Header("Mortar Settings")]
    public Vector3 TargetPosition;
    [HideInInspector] public bool UseMissileExplosionPool;
    [Tooltip("Height above launch point where the shell waits before dropping.")]
    public float MaxHeight = 10f;
    [Tooltip("Delay in seconds before the shell starts falling down.")]
    public float MinTravelTime = 0.5f;
    [Tooltip("If greater than 0, shell will explode after this many seconds since spawn.")]
    public float TimeToImpact = -1f;

    private Rigidbody _rb;
    public float Diameter;
    private Vector3 _startPosition;
    private float _fallStartHeight;
    private float _delayBeforeFall;
    private float _fallDuration;
    private float _totalTravelTime;
    private float _currentTime;
    private bool _startedFalling;
    private MissileDestination _destinationVFX;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    public override void Init(float speed, float lifeTime = 5f)
    {
        base.Init(speed, lifeTime);
        _startPosition = transform.position;
        _currentTime = 0f;
        _startedFalling = false;

        _fallStartHeight = Mathf.Max(
            _startPosition.y + Mathf.Max(0f, MaxHeight),
            TargetPosition.y + 0.5f);
        var fallDistance = Mathf.Max(0.01f, _fallStartHeight - TargetPosition.y);
        _fallDuration = Mathf.Max(0.3f, fallDistance / FallSpeed);

        if (TimeToImpact > 0f)
        {
            var minPossibleTravelTime = RiseDuration + _fallDuration;
            _totalTravelTime = Mathf.Max(minPossibleTravelTime, TimeToImpact);
            _delayBeforeFall = Mathf.Max(RiseDuration, _totalTravelTime - _fallDuration);
        }
        else
        {
            _delayBeforeFall = Mathf.Max(Mathf.Max(0f, MinTravelTime), RiseDuration);
            _totalTravelTime = _delayBeforeFall + _fallDuration;
        }

        var destinationPool = LayerMask.LayerToName(gameObject.layer) == "Player Missile"
            ? VFXPools.Instance.PlayerMissileDestinationPool
            : VFXPools.Instance.MissileDestinationPool;
        _destinationVFX = destinationPool.GetFromPool<MissileDestination>(TargetPosition.WithY(0));
        _destinationVFX.SetDiameter(Diameter);
        _destinationVFX.SetFillAmount(0f);
        MissileColisionProvider?.SetExplosionDiameter(Diameter);
        MissileColisionProvider?.SetExplodeOnTriggerEnter(false);
    }

    protected void FixedUpdate()
    {
        if (_rb == null)
        {
            return;
        }

        _currentTime += Time.fixedDeltaTime;

        if (!_startedFalling && _currentTime >= _delayBeforeFall)
        {
            _startedFalling = true;
            var fallStartPos = new Vector3(TargetPosition.x, _fallStartHeight, TargetPosition.z);
            _rb.position = fallStartPos;
            transform.position = fallStartPos;
        }

        Vector3 targetPos;
        if (!_startedFalling)
        {
            var riseProgress = Mathf.Clamp01(_currentTime / RiseDuration);
            var currentHeight = Mathf.Lerp(_startPosition.y, _fallStartHeight, riseProgress);
            targetPos = new Vector3(_startPosition.x, currentHeight, _startPosition.z);
        }
        else
        {
            var fallProgress = Mathf.Clamp01((_currentTime - _delayBeforeFall) / _fallDuration);
            var currentHeight = Mathf.Lerp(_fallStartHeight, TargetPosition.y, fallProgress);
            targetPos = new Vector3(TargetPosition.x, currentHeight, TargetPosition.z);

            if (fallProgress >= 1f)
            {
                _rb.MovePosition(targetPos);
                MissileColisionProvider.Boom(UseMissileExplosionPool);
                return;
            }
        }

        var direction = targetPos - _rb.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            _rb.MoveRotation(Quaternion.LookRotation(direction.normalized));
        }

        _rb.MovePosition(targetPos);
    }

    private void Update()
    {
        if (_destinationVFX != null)
        {
            _destinationVFX.SetFillAmount(Mathf.Clamp01(_currentTime / _totalTravelTime));
        }
    }

    private void OnDisable()
    {
        if (_destinationVFX != null)
        {
            _destinationVFX.gameObject.SetActive(false);
            _destinationVFX = null;
        }
    }
}

