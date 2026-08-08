
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TankStyleMovement : MonoBehaviour, IMovementType
{
    [Header("ƒвижение и “€жесть")]
    [SerializeField] private float _maxSpeed = 8f;
    [SerializeField] private float _acceleration = 4f;
    [SerializeField] private float _deceleration = 2f;
    [SerializeField] private float _turnSpeed = 50f;
    [SerializeField] private float _turnSmooth = 3f;

    private Rigidbody _rb;
    private float _moveInput;
    private float _turnInput;

    void Update()
    {
        _moveInput = Input.GetAxisRaw("Vertical");
        _turnInput = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
    }

    private void ApplyMovement()
    {
        Vector3 targetVelocity = transform.forward * _moveInput * _maxSpeed;
        float currentForce = (Mathf.Abs(_moveInput) > 0.1f) ? _acceleration : _deceleration;

        Vector3 newVel = Vector3.MoveTowards(_rb.linearVelocity, targetVelocity, currentForce * Time.fixedDeltaTime);
        newVel.y = _rb.linearVelocity.y;
        _rb.linearVelocity = newVel;
    }

    private void ApplyRotation()
    {
        float targetAngularY = _turnInput * _turnSpeed * Mathf.Deg2Rad;
        float smoothAngularY = Mathf.MoveTowards(_rb.angularVelocity.y, targetAngularY, _turnSmooth * Time.fixedDeltaTime);
        _rb.angularVelocity = new Vector3(0, smoothAngularY, 0);
    }



    public void Active()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        _rb.useGravity = true;
        this.enabled = true;
    }

    public void Deactive()
    {
        this.enabled = false;

    }
}