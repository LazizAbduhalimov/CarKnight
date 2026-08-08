using UnityEngine;

public sealed class RacingCarMovementState : MovementFsmState
{
    public RacingCarMovementState(Fsm fsm, MovementFsmController controller) : base(fsm, controller) { }

    public override void Enter()
    {
        Rb.interpolation = RigidbodyInterpolation.Interpolate;
        Rb.useGravity = true;
    }

    public override void FixedUpdate()
    {
        var settings = Config.RacingCar;
        var speed = Controller.ForwardInput < -0.01f ? settings.BackwardSpeed : settings.MaxSpeed;
        var targetVelocity = CachedTransform.forward * Controller.ForwardInput * speed;
        var currentStep = Mathf.Abs(Controller.ForwardInput) > 0.05f ? settings.Acceleration : settings.Deceleration;

        var newVelocity = Vector3.MoveTowards(Rb.linearVelocity, targetVelocity, currentStep * Time.fixedDeltaTime);
        newVelocity.y = Rb.linearVelocity.y;
        Rb.linearVelocity = newVelocity;

        var canTurn = Mathf.Abs(Controller.TurnInput) > 0.01f && Mathf.Abs(Controller.ForwardInput) > 0.01f;

        if (canTurn)
        {
            float direction = Controller.ForwardInput < -0.01f ? -1f : 1f;

            var turnAmount = Controller.TurnInput * direction * settings.RotateSpeed * Mathf.Deg2Rad;

            Rb.angularVelocity = new Vector3(0f, turnAmount, 0f);
            return;
        }

        Rb.angularVelocity = Vector3.zero;
    }

    public override void Exit()
    {
        Rb.angularVelocity = Vector3.zero;
    }
}
