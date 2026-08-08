using UnityEngine;

public sealed class LevitatingCarMovementState : MovementFsmState
{
    public LevitatingCarMovementState(Fsm fsm, MovementFsmController controller) : base(fsm, controller) { }

    public override void Enter()
    {
        Rb.useGravity = false;
    }

    public override void FixedUpdate()
    {
        var settings = Config.LevitatingCar;
        var inputCamera = Controller.InputCamera;

        ApplyHover(settings.HoverHeight, settings.HoverForce, settings.HoverDamp, settings.GroundMask);

        var forward = inputCamera != null ? inputCamera.transform.forward : CachedTransform.forward;
        var right = inputCamera != null ? inputCamera.transform.right : CachedTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        var moveDirection = forward * Controller.ForwardInput + right * Controller.TurnInput;
        var targetVelocity = moveDirection * settings.Speed;
        targetVelocity.y = Rb.linearVelocity.y;

        Rb.linearVelocity = Vector3.Lerp(
            Rb.linearVelocity,
            targetVelocity,
            Time.fixedDeltaTime * 6f
        );

        var direction = Controller.LookTarget - CachedTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            var targetYaw = Quaternion.LookRotation(direction.normalized);

            var tilt = Quaternion.Euler(
                Controller.ForwardInput * settings.TiltAmount,
                0f,
                -Controller.TurnInput * settings.TiltAmount
            );

            var smoothedYaw = Quaternion.Slerp(
                CachedTransform.rotation,
                targetYaw,
                settings.TurnSpeed * Time.fixedDeltaTime
            );

            CachedTransform.rotation = Quaternion.Slerp(
                CachedTransform.rotation,
                smoothedYaw * tilt,
                settings.TiltSpeed * Time.fixedDeltaTime
            );
        }
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