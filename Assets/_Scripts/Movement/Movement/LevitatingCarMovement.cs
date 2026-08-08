using UnityEngine;

public class LevitatingCarMovement : MonoBehaviour, IMovementType
{
    [Header("Movement")]
    [SerializeField] private float speed = 50f;
    [SerializeField] private float turnSpeed = 10f;

    [Header("Hower")]
    [SerializeField] private float hoverHeight = 2f;
    [SerializeField] private float hoverForce = 50f;
    [SerializeField] private float hoverDamp = 5f;
    [SerializeField] private LayerMask groundMask;

    [Header("Tilt")]
    [SerializeField] private float tiltAmount = 4f;
    [SerializeField] private float tiltSpeed = 6f;

    [Header("Camera")]
    [SerializeField] private Camera inputCamera;


    private Rigidbody rb;
    private Vector3 input;
    private Vector3 lookTarget;

    private void Update()
    {
        SetLookTarget(GetMouseWorld());
        ReadInput();
    }
    void FixedUpdate()
    {
        Hover();
        MoveInternal();
        RotateToTarget();
        Tilt();
    }

    private void ReadInput()
    {
        input = new Vector3(Input.GetAxisRaw("Vertical"), 0f, Input.GetAxisRaw("Horizontal"));
    }
    void Hover()
    {
        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, hoverHeight * 2f, groundMask);
        float error = hoverHeight - hit.distance;
        float lift = error * hoverForce - rb.linearVelocity.y * hoverDamp;
        rb.AddForce(Vector3.up * lift, ForceMode.Acceleration);

    }
    Vector3 GetMouseWorld()
    {
        Ray r = inputCamera.ScreenPointToRay(Input.mousePosition);
        Plane p = new Plane(Vector3.up, transform.position);
        if (p.Raycast(r, out float d)) return r.GetPoint(d);
        return transform.position + transform.forward * 10f;
    }

    void MoveInternal()
    {
        Vector3 forward = inputCamera.transform.forward;
        Vector3 right = inputCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * input.x + right * input.z;

        Vector3 targetVel = moveDir * speed;
        targetVel.y = rb.linearVelocity.y; 

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVel, Time.fixedDeltaTime * 6f);
    }

    void RotateToTarget()
    {
        Vector3 dir = lookTarget - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion rot = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, turnSpeed * Time.fixedDeltaTime);
    }

    void Tilt()
    {
        Vector3 tilt = new Vector3(input.x * tiltAmount, 0f, -input.z * tiltAmount);
        Quaternion target = Quaternion.Euler(tilt);
        transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * target, tiltSpeed * Time.fixedDeltaTime);
    }

    public void SetLookTarget(Vector3 worldPos)
    {
        lookTarget = worldPos;
    }
    public void Active()
    {
        rb = GetComponent<Rigidbody>();
        if (inputCamera == null) inputCamera = Camera.main;

        this.enabled = true;
        rb.useGravity = false;

    }

    public void Deactive()
    {
        this.enabled = false;
        rb.useGravity = true;

        Vector3 vel = rb.linearVelocity;
        if (vel.y > 0) vel.y = 0;
        rb.linearVelocity = vel;

        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }
}
