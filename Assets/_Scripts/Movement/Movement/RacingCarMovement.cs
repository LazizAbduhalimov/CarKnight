using UnityEngine;

public class RacingCarMovement : MonoBehaviour, IMovementType
{
    [Header("Speed Settings")]
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _backwardSpeed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;

    [Header("Rotation Settings")]
    [SerializeField] private float _rotateSpeed;

    private Rigidbody _rb;
    private float _forwardInput;
    private float _turnInput;

    void Update()
    {
        _forwardInput = Input.GetAxisRaw("Vertical");
        _turnInput = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        Move();
        Rotate();
    }

    private void Move()
    {


        float speed = _forwardInput < -0.01f ? _backwardSpeed : _maxSpeed;
        Vector3 targetVelocity = transform.forward * _forwardInput * speed;

        float currentStep = (Mathf.Abs(_forwardInput) > 0.05f) ? _acceleration : _deceleration;

        Vector3 newVelocity = Vector3.MoveTowards(_rb.linearVelocity, targetVelocity, currentStep * Time.fixedDeltaTime);

        newVelocity.y = _rb.linearVelocity.y;

        _rb.linearVelocity = newVelocity;
    }

    private void Rotate()
    {
        if (Mathf.Abs(_turnInput) > 0.05f)
        {
            float turnAmount = _turnInput * _rotateSpeed * Mathf.Deg2Rad;
            _rb.angularVelocity = new Vector3(0f, turnAmount, 0f);
        }
        else
        {
            _rb.angularVelocity = Vector3.zero;
        }
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
