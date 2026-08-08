using PoolSystem.Alternative;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArcMissile : MissileBase
{
    [HideInInspector] public Vector3 TargetPosition;
    public float ArcHeight = 5f;
    
    private Rigidbody _rb;
    public float Diameter;
    private Vector3 _startPosition;
    private float _travelTime;
    private float _currentTime;
    private Vector3 _initialVelocity;
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
        
        // Вычисляем время полета на основе расстояния и скорости
        var distance = Vector3.Distance(_startPosition, TargetPosition);
        _travelTime = distance / Speed;
        var destinationPool = LayerMask.LayerToName(gameObject.layer) == "Player Missile"
            ? VFXPools.Instance.PlayerMissileDestinationPool
            : VFXPools.Instance.MissileDestinationPool;
        _destinationVFX = destinationPool.GetFromPool<MissileDestination>(TargetPosition.WithY(0));
        _destinationVFX.SetDiameter(Diameter);
        _destinationVFX.SetFillAmount(0f);
        MissileColisionProvider?.SetExplosionDiameter(Diameter);
        // Вычисляем начальную скорость для параболической траектории
        CalculateArcVelocity();
    }

    private void CalculateArcVelocity()
    {
        var toTarget = TargetPosition - _startPosition;
        var horizontalDistance = new Vector3(toTarget.x, 0f, toTarget.z);
        var verticalDistance = toTarget.y;
        
        // Горизонтальная скорость
        var horizontalVelocity = horizontalDistance / _travelTime;
        
        // Вертикальная скорость с учетом дуги
        // v_y = (h_peak - y_start) / (t/2) - учитываем пик дуги
        var peakTime = _travelTime / 2f;
        var verticalVelocity = (ArcHeight + verticalDistance) / peakTime;
        
        _initialVelocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
    }

    protected void FixedUpdate()
    {
        if (_rb == null) return;
        
        _currentTime += Time.fixedDeltaTime;
        
        // Прогресс от 0 до 1
        var t = Mathf.Clamp01(_currentTime / _travelTime);
        
        // Вычисляем текущую позицию по параболе
        var horizontalPos = Vector3.Lerp(_startPosition, TargetPosition, t);
        
        // Добавляем вертикальную составляющую (парабола)
        var arcProgress = 4f * t * (1f - t); // Парабола: максимум в середине (t=0.5)
        var verticalOffset = arcProgress * ArcHeight;
        
        var targetPos = new Vector3(horizontalPos.x, 
            Mathf.Lerp(_startPosition.y, TargetPosition.y, t) + verticalOffset, 
            horizontalPos.z);
        
        // Поворачиваем снаряд по направлению движения
        var direction = targetPos - _rb.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            _rb.MoveRotation(Quaternion.LookRotation(direction.normalized));
        }
        
        _rb.MovePosition(targetPos);
        
        // Уничтожаем снаряд когда достигнем цели
        if (t >= 1f)
        {
            MissileColisionProvider.Boom(true);
        }
    }

    private void Update()
    {
        _destinationVFX.SetFillAmount(Mathf.Clamp01(_currentTime / _travelTime));
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

