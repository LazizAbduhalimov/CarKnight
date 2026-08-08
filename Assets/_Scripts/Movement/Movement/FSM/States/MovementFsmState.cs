using UnityEngine;

public abstract class MovementFsmState : FsmState
{
    protected MovementFsmController Controller { get; }
    protected Rigidbody Rb => Controller.Rigidbody;
    protected Transform CachedTransform => Controller.transform;
    protected MovementStateMachineConfig Config => Controller.Config;

    protected MovementFsmState(Fsm fsm, MovementFsmController controller) : base(fsm)
    {
        Controller = controller;
    }

    protected void ApplyHover(float hoverHeight, float hoverForce, float hoverDamp, LayerMask groundMask)
    {
        float maxRayDistance = hoverHeight * 5f;
        bool hasHit = Physics.Raycast(CachedTransform.position, Vector3.down, out var hit, maxRayDistance, groundMask);

        if (hasHit)
        {
            var error = hoverHeight - hit.distance;

            var lift = error * hoverForce - Rb.linearVelocity.y * hoverDamp;

            Rb.AddForce(Vector3.up * lift, ForceMode.Acceleration);
        }
    }

    protected void RotateToTarget(Vector3 lookTarget, float turnSpeed)
    {
        var direction = lookTarget - CachedTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            return;
        }

        var targetRotation = Quaternion.LookRotation(direction.normalized);
        CachedTransform.rotation = Quaternion.Slerp(
            CachedTransform.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime);
    }

    protected void Tilt(float forwardInput, float turnInput, float tiltAmount, float tiltSpeed)
    {
        var tiltEuler = new Vector3(forwardInput * tiltAmount, 0f, -turnInput * tiltAmount);
        var tiltRotation = Quaternion.Euler(tiltEuler);
        CachedTransform.rotation = Quaternion.Slerp(
            CachedTransform.rotation,
            CachedTransform.rotation * tiltRotation,
            tiltSpeed * Time.fixedDeltaTime);
    }
}
