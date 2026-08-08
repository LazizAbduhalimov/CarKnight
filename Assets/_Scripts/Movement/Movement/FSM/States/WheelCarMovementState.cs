using UnityEngine;

public sealed class WheelCarMovementState : MovementFsmState
{
    public WheelCarMovementState(Fsm fsm, MovementFsmController controller) : base(fsm, controller) { }

    public override void Enter()
    {
        Rb.centerOfMass = new Vector3(0f, Config.WheelCar.CenterOfMassY, 0f);
        Rb.useGravity = true;
    }

    public override void FixedUpdate()
    {
        var settings = Config.WheelCar;
        var speed = Controller.ForwardInput < -0.01f ? settings.BackwardSpeed : settings.MaxSpeed;

        var targetVelocity = CachedTransform.forward * Controller.ForwardInput * speed;
        targetVelocity.y = Rb.linearVelocity.y;
        Rb.linearVelocity = Vector3.Lerp(Rb.linearVelocity, targetVelocity, settings.Acceleration * Time.fixedDeltaTime);

        var isGrounded = Controller.IsGrounded(settings.GroundDistance);
        var canTurn = Mathf.Abs(Controller.TurnInput) > 0.01f && (Mathf.Abs(Controller.ForwardInput) > 0.01f || Rb.linearVelocity.magnitude > 7f);

        if (canTurn && isGrounded)
        {
            float direction = Mathf.Sign(Controller.ForwardInput);

            var turn = Controller.TurnInput * direction * settings.RotationSpeed * Mathf.Deg2Rad;
            Rb.angularVelocity = new Vector3(0f, turn, 0f);
        }
        else
        {
            Rb.angularVelocity = Vector3.Lerp(Rb.angularVelocity, Vector3.zero, 5f * Time.fixedDeltaTime);
        }

        if (Mathf.Abs(Controller.ForwardInput) < 0.01f)
        {
            Rb.linearVelocity = Vector3.Lerp(
                Rb.linearVelocity,
                new Vector3(0f, Rb.linearVelocity.y, 0f),
                settings.DragCoefficient * Time.fixedDeltaTime);
        }
    }

    public override void Exit()
    {
        Rb.angularVelocity = Vector3.zero;
    }
}
