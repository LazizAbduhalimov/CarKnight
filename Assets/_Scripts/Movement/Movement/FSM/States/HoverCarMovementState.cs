using UnityEngine;

public sealed class HoverCarMovementState : MovementFsmState
{
    public HoverCarMovementState(Fsm fsm, MovementFsmController controller) : base(fsm, controller) { }

    public override void Enter()
    {
        Rb.useGravity = false;
    }

    public override void FixedUpdate()
    {
        var settings = Config.HoverCar;

        ApplyHover(settings.HoverHeight, settings.HoverForce, settings.HoverDamp, settings.GroundMask);

        var toTarget = Controller.LookTarget - CachedTransform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 1f)
            return;

        var moveDir = toTarget.normalized;

        Vector3 targetVelocity;

        if (Controller.IsMouseHolding)
        {
            var distance = toTarget.magnitude;

            var speedFactor = Mathf.Clamp01(distance / settings.TurnDistance);

            targetVelocity = moveDir * settings.Speed * 1.8f;
            targetVelocity *= speedFactor;

            var right = Vector3.Cross(Vector3.up, moveDir);
            targetVelocity += right * Controller.TurnInput * settings.Speed * 0.3f;
        }
        else
        {
            var forwardMove = moveDir * Controller.ForwardInput;
            var right = Vector3.Cross(Vector3.up, moveDir);
            var strafeMove = right * Controller.TurnInput * 0.6f;
            targetVelocity = (forwardMove + strafeMove) * settings.Speed;
        }

        targetVelocity.y = Rb.linearVelocity.y;

        Rb.linearVelocity = Vector3.Lerp(
            Rb.linearVelocity,
            targetVelocity,
            Time.fixedDeltaTime * (Controller.IsMouseHolding ? 8f : 4f)
        );


        var targetYaw = Quaternion.LookRotation(moveDir);

        var tiltEuler = new Vector3(
            Controller.ForwardInput * settings.TiltAmount,
            0f,
            -Controller.TurnInput * settings.TiltAmount
        );
        var tilt = Quaternion.Euler(tiltEuler);

        CachedTransform.rotation = Quaternion.Slerp(
            CachedTransform.rotation,
            targetYaw * tilt,
            settings.TurnSpeed * Time.fixedDeltaTime
        );
    }

    public override void Exit()
    {
        Rb.useGravity = true;

        var velocity = Rb.linearVelocity;
        if (velocity.y > 0f)
            velocity.y = 0f;

        Rb.linearVelocity = velocity;
        Rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
    }
}