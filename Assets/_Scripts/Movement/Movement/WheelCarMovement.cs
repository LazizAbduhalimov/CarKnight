using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WheelCarMovement : MonoBehaviour, IMovementType
{
    [Header("Movement")]
    [SerializeField] private float _maxSpeed = 60f;
    [SerializeField] private float _backwardSpeed = 20f;
    [SerializeField] private float _acceleration = 8f;

    [Header("Rotation")]
    [SerializeField] private float _groundDistance;
    [SerializeField] private float _rotationSpeed = 270f;
    [SerializeField] private float _dragCoefficient = 3f;
    [SerializeField] private float _centerOfMassY = -0.5f;

    private Rigidbody rb;
    private float motor;
    private float steer;


    private GravitationSetter gravitationSetter;

    void Update()
    {
        ReadInput();
    }
    void FixedUpdate()
    {
        ReadInput();
        Move();
        Rotate();
        Drag();

    }
    private void ReadInput()
    {
        motor = Mathf.Clamp(Input.GetAxisRaw("Vertical"), -1f, 1f);
        steer = Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1f, 1f);
    }

    private void Move()
    {
        float speed = motor < -0.01f ? _backwardSpeed : _maxSpeed;

        Vector3 targetVel = transform.forward * motor * speed;
        targetVel.y = rb.linearVelocity.y;
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVel, _acceleration * Time.fixedDeltaTime);
    }
    private void Rotate()
    {
        bool isGrounded = gravitationSetter.GroundCheck(_groundDistance);
        bool canTurn = Mathf.Abs(steer) > 0.01f && Mathf.Abs(motor) > 0.01f;

        if (canTurn && isGrounded)
        {
            float turn = steer * _rotationSpeed * Mathf.Deg2Rad;
            rb.angularVelocity = new Vector3(0f, turn, 0f);
        }
        else
        {
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, 5f * Time.fixedDeltaTime);
        }
    }

    private void Drag()
    {
        if (Mathf.Abs(motor) < 0.01f)
        {
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(0f, rb.linearVelocity.y, 0f), _dragCoefficient * Time.fixedDeltaTime);
        }
    }

   


    public void Active()
    {
        gravitationSetter = GetComponent<GravitationSetter>();
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, _centerOfMassY, 0f);

        rb.useGravity = true;
        this.enabled = true;
    }

    public void Deactive()
    {
        this.enabled = false;

    }
}
