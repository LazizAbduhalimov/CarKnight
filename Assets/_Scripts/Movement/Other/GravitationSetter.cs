using UnityEngine;

public class GravitationSetter : MonoBehaviour
{
    [Header("Gravity Settings")]
    [SerializeField] private float _gravitationForce = 15f;      
    [SerializeField] private float _airGravitationForce = 30f;   
    [SerializeField] private float _airThreshold = 2.0f;         
    [SerializeField] private float _targetYPosition = 0.5f;
    [SerializeField] private float _yCenterOffset = 0.5f;


    [Header("Raycast Settings")]
    [SerializeField] private float _raycastDownLength = 1.5f;
    [SerializeField] private LayerMask _groundLayer;

    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    void FixedUpdate()
    {
        float currentGravity = _gravitationForce;

        if (!GroundCheck() && transform.position.y > _airThreshold)
        {
            currentGravity = _airGravitationForce;
        }

        _rb.AddForce(Vector3.down * currentGravity, ForceMode.Acceleration);

        if (!GroundCheck())
        {
            if (transform.position.y < _targetYPosition - 0.2f)
            {
                ApplyPositionCorrection();
            }
        }

    }

    private void ApplyPositionCorrection()
    {
        Vector3 pos = _rb.position;
        pos.y = _targetYPosition;
        _rb.MovePosition(pos);

        Vector3 vel = _rb.linearVelocity;
        vel.y = 0;
        _rb.linearVelocity = vel;
    }

    public bool GroundCheck(float customLength = 0)
    {
        Vector3 posCheck = transform.position + Vector3.up * _yCenterOffset;
        float length = customLength == 0 ? _raycastDownLength : customLength;

        return Physics.Raycast(posCheck, Vector3.down, length, _groundLayer);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 posCheck = transform.position + Vector3.up * _yCenterOffset;
        Gizmos.DrawRay(posCheck, Vector3.down * _raycastDownLength);

        Gizmos.color = Color.yellow;
        Vector3 thresholdLine = transform.position;
        thresholdLine.y = _airThreshold;
        Gizmos.DrawWireCube(thresholdLine, new Vector3(1, 0.1f, 1));
    }
}