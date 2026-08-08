using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class MovementFsmController : MonoBehaviour
{
    [SerializeField] private MovementStateMachineConfig _config;
    [SerializeField] private Camera _inputCamera;
    [SerializeField] private CarWheelsView _wheelsView; 

    private readonly Fsm _fsm = new Fsm();
    private Rigidbody _rigidbody;
    private GravitationSetter _gravitationSetter;

    public BoxCollider Collider ;
    public MovementMode CurrentMode { get; private set; }
    public MovementStateMachineConfig Config => _config;
    public Rigidbody Rigidbody => _rigidbody;
    public Camera InputCamera => _inputCamera != null ? _inputCamera : Camera.main;
    public float ForwardInput { get; private set; }
    public float TurnInput { get; private set; }
    public Vector3 LookTarget { get; private set; }

    public bool IsMouseHolding { get; private set; }

    public bool chosenOnce;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _gravitationSetter = GetComponent<GravitationSetter>();

        if (_config == null)
        {
            _config = ScriptableObject.CreateInstance<MovementStateMachineConfig>();
            Debug.LogWarning("MovementFsmController is missing MovementStateMachineConfig.", this);
        }

        if (_inputCamera == null)
        {
            _inputCamera = Camera.main;
        }

        RegisterStates();

        // Устанавливаем начальный режим
        SwitchTo(_config != null ? _config.DefaultMode : MovementMode.WheelCar);
    }

    private void Update()
    {
        ReadInput();
        UpdateLookTarget();
        _fsm.Update();

        // Передаем поворот в визуальную часть каждый кадр
        if (_wheelsView != null)
        {
            _wheelsView.UpdateRotation(TurnInput);
        }
    }

    private void FixedUpdate()
    {
        _fsm.FixedUpdate();
    }

    public void SwitchTo(MovementMode movementMode)
    {
        ResetPhysicsForStateSwap();
        CurrentMode = movementMode;

        if (_wheelsView != null)
        {
            _wheelsView.SwitchVisualMode(movementMode);
        }

        switch (movementMode)
        {
            case MovementMode.HoverCar:
                _fsm.SetState<HoverCarMovementState>();
                break;
            case MovementMode.LevitatingCar:
                _fsm.SetState<LevitatingCarMovementState>();
                break;
            case MovementMode.RacingCar:
                _fsm.SetState<RacingCarMovementState>();
                break;
            case MovementMode.TankStyle:
                _fsm.SetState<TankStyleMovementState>();
                break;
            case MovementMode.WheelCar:
                _fsm.SetState<WheelCarMovementState>();
                break;
            default:
                Debug.LogWarning($"Unsupported movement mode: {movementMode}", this);
                break;
        }

    }

    public void SwitchToHover() => SwitchTo(MovementMode.HoverCar);
    public void SwitchToLevitating() => SwitchTo(MovementMode.LevitatingCar);
    public void SwitchToRacing() => SwitchTo(MovementMode.RacingCar);
    public void SwitchToTank() => SwitchTo(MovementMode.TankStyle);
    public void SwitchToWheelCar() => SwitchTo(MovementMode.WheelCar);

    private void RegisterStates()
    {
        _fsm.AddState(new HoverCarMovementState(_fsm, this));
        _fsm.AddState(new LevitatingCarMovementState(_fsm, this));
        _fsm.AddState(new RacingCarMovementState(_fsm, this));
        _fsm.AddState(new TankStyleMovementState(_fsm, this));
        _fsm.AddState(new WheelCarMovementState(_fsm, this));
    }

    public bool IsGrounded(float groundDistance)
    {
        return _gravitationSetter == null || _gravitationSetter.GroundCheck(groundDistance);
    }

    private void ReadInput()
    {
        IsMouseHolding = Input.GetMouseButton(0);
        ForwardInput = Input.GetAxisRaw("Vertical");
        TurnInput = Input.GetAxisRaw("Horizontal");
    }

    private void UpdateLookTarget()
    {
        var inputCamera = InputCamera;
        if (inputCamera == null)
        {
            LookTarget = transform.position + transform.forward * 10f;
            return;
        }

        var ray = inputCamera.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out var distance))
        {
            LookTarget = ray.GetPoint(distance);
            return;
        }

        LookTarget = transform.position + transform.forward * 10f;
    }

    private void ResetPhysicsForStateSwap()
    {
        if (_rigidbody != null)
        {
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }

    public void SetViev(CarWheelsView wheel)
    {
        _wheelsView = wheel;
    }
}