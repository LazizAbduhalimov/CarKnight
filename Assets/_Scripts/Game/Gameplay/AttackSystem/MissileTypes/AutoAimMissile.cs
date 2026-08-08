using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AutoAimMissile : MissileBase
{
    public Transform Target;
    public float TurnRatePerSecond = 180f;
    private Rigidbody _rb;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody>();
    }

    protected void FixedUpdate()
    {
        if (_rb == null) return;

        if (Target != null)
        {
            if (!Target.gameObject.activeInHierarchy)
                MissileColisionProvider.Boom();
            
            var toTarget = Target.position - _rb.position;
            if (toTarget.sqrMagnitude > 0.001f)
            {
                var normalizedDirection = toTarget.normalized;
                var up = Mathf.Abs(Vector3.Dot(normalizedDirection, Vector3.up)) > 0.999f ? Vector3.forward : Vector3.up;
                var desired = Quaternion.LookRotation(normalizedDirection, up);
                var newRotation = Quaternion.RotateTowards(_rb.rotation, desired, TurnRatePerSecond * Time.fixedDeltaTime);
                _rb.MoveRotation(newRotation);
            }
        }

        var nextPos = _rb.position + _rb.rotation * Vector3.forward * Speed * Time.fixedDeltaTime;
        _rb.MovePosition(nextPos);
    }
}
