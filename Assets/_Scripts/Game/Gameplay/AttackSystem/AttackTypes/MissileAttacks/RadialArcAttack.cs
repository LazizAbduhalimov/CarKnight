using PrimeTween;
using UnityEngine;

public class RadialArcAttack : BurstMissileAttackBase<ArcMissile>
{
    [Header("Radial Arc Settings")]
    public float ExplosionDiameter = 2f;
    [Tooltip("Высота дуги в метрах")]
    public float ArcHeight = 5f;
    public int ProjectilesPerCircle = 8;
    public float DelayBetweenCircleProjectiles = 0f;
    public float CircleRadius = 6f;
    [Tooltip("Угол в градусах для поворота всего кольца")]
    public float AngleOffsetDegrees = 0f;
    [Tooltip("Если true, нулевой угол берется от forward владельца")]
    public bool UseOwnerForwardAsBaseAngle = true;

    private Tween _nextCircleProjectileTween;
    private int _circleProjectileIndex;
    private int _circleProjectileCount;
    private float _circleBaseAngle;
    private Vector3 _circleOrigin;
    private Vector3 _circleSpawnPosition;

    protected override bool ExecuteAttack()
    {
        if (Owner == null)
        {
            Debug.LogWarning("Owner is null in RadialArcAttack");
            return false;
        }

        _nextCircleProjectileTween.Stop();
        _circleProjectileCount = Mathf.Max(1, ProjectilesPerCircle);
        _circleProjectileIndex = 0;
        _circleBaseAngle = (UseOwnerForwardAsBaseAngle ? Owner.eulerAngles.y : 0f) + AngleOffsetDegrees;
        _circleOrigin = Owner.position;
        _circleSpawnPosition = Owner.position.AddY(1f);

        if (DelayBetweenCircleProjectiles <= 0f)
        {
            for (var i = 0; i < _circleProjectileCount; i++)
            {
                SpawnCircleProjectile(i);
            }

            return false;
        }

        FireNextCircleProjectile();
        return false;
    }

    public override void Dispose()
    {
        _nextCircleProjectileTween.Stop();
    }

    private void FireNextCircleProjectile()
    {
        if (_circleProjectileIndex >= _circleProjectileCount || Owner == null)
        {
            return;
        }

        SpawnCircleProjectile(_circleProjectileIndex);
        _circleProjectileIndex++;

        if (_circleProjectileIndex >= _circleProjectileCount)
        {
            return;
        }

        _nextCircleProjectileTween.Stop();
        _nextCircleProjectileTween = Tween.Delay(DelayBetweenCircleProjectiles, FireNextCircleProjectile);
    }

    private void SpawnCircleProjectile(int index)
    {
        if (Owner == null)
        {
            return;
        }

        var angleStep = 360f / _circleProjectileCount;
        var angle = _circleBaseAngle + index * angleStep;
        var direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
        var targetPosition = _circleOrigin + direction * CircleRadius;

        var missile = MissilesPool.GetFreeElement(_circleSpawnPosition, Quaternion.LookRotation(direction, Vector3.up), false);
        LayerAssigner.AssignLayer(Owner.gameObject.layer, missile.gameObject);

        missile.TargetPosition = targetPosition;
        missile.ArcHeight = ArcHeight;
        missile.Diameter = ExplosionDiameter;

        missile.Init(MissileSpeed, 5f);
        missile.SetDamage(Damage);
        missile.gameObject.SetActive(true);
        PlayShotSound(_circleSpawnPosition);
    }
}
