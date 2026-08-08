using UnityEngine;

public sealed class TankStyleMovementState : MovementFsmState
{
    public TankStyleMovementState(Fsm fsm, MovementFsmController controller) : base(fsm, controller) { }

    private float saving;
    public override void Enter()
    {
        saving = Controller.Collider.center.y;

        Controller.Collider.center = new Vector3(Controller.Collider.center.x, Config.TankStyle.CenterOfMassY, Controller.Collider.center.z);

        Rb.interpolation = RigidbodyInterpolation.Interpolate;
        Rb.useGravity = true;
    }

    public override void FixedUpdate()
    {
        var settings = Config.TankStyle;

        var targetVelocity = CachedTransform.forward * Controller.ForwardInput * settings.MaxSpeed;
        var currentForce = Mathf.Abs(Controller.ForwardInput) > 0.1f ? settings.Acceleration : settings.Deceleration;

        var newVelocity = Vector3.MoveTowards(Rb.linearVelocity, targetVelocity, currentForce * Time.fixedDeltaTime);
        newVelocity.y = Rb.linearVelocity.y;
        Rb.linearVelocity = newVelocity;


        float direction = Controller.ForwardInput < -0.1f ? -1f : 1f;

        if (Mathf.Abs(Controller.TurnInput) > 0.1f)
        {
            var targetAngularY = Controller.TurnInput * direction * settings.TurnSpeed * Mathf.Deg2Rad;
            var smoothAngularY = Mathf.MoveTowards(Rb.angularVelocity.y, targetAngularY, settings.TurnSmooth * Time.fixedDeltaTime);
            Rb.angularVelocity = new Vector3(0f, smoothAngularY, 0f);
        }
        else
        {
            Rb.angularVelocity = Vector3.zero;
        }

    }

    public override void Exit()
    {
        Controller.Collider.center = new Vector3(Controller.Collider.center.x, saving, Controller.Collider.center.z);
        Rb.angularVelocity = Vector3.zero;
    }
}
