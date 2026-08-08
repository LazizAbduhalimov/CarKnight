using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementStateMachineConfig", menuName = "Movement/Movement State Machine Config")]
public class MovementStateMachineConfig : ScriptableObject
{
    public MovementMode DefaultMode = MovementMode.WheelCar;

    [Header("Hover Car")]
    public HoverCarSettings HoverCar = new HoverCarSettings();

    [Header("Levitating Car")]
    public LevitatingCarSettings LevitatingCar = new LevitatingCarSettings();

    [Header("Racing Car")]
    public RacingCarSettings RacingCar = new RacingCarSettings();

    [Header("Tank Style")]
    public TankStyleSettings TankStyle = new TankStyleSettings();

    [Header("Wheel Car")]
    public WheelCarSettings WheelCar = new WheelCarSettings();
}

[Serializable]
public class HoverCarSettings
{
    public float Speed = 50f;
    public float TurnDistance = 10f;
    public float TurnSpeed = 10f;
    public float HoverHeight = 2f;
    public float HoverForce = 50f;
    public float HoverDamp = 5f;
    public LayerMask GroundMask;
    public float TiltAmount = 4f;
    public float TiltSpeed = 6f;
}

[Serializable]
public class LevitatingCarSettings
{
    public float Speed = 50f;
    public float TurnSpeed = 10f;
    public float HoverHeight = 2f;
    public float HoverForce = 50f;
    public float HoverDamp = 5f;
    public LayerMask GroundMask;
    public float TiltAmount = 4f;
    public float TiltSpeed = 6f;
}

[Serializable]
public class RacingCarSettings
{
    public float MaxSpeed = 20f;
    public float BackwardSpeed = 10f;
    public float Acceleration = 15f;
    public float Deceleration = 12f;
    public float RotateSpeed = 140f;
}

[Serializable]
public class TankStyleSettings
{
    public float MaxSpeed = 8f;
    public float Acceleration = 4f;
    public float Deceleration = 2f;
    public float TurnSpeed = 50f;
    public float TurnSmooth = 3f;
    public float CenterOfMassY = 0f;
}

[Serializable]
public class WheelCarSettings
{
    public float MaxSpeed = 60f;
    public float BackwardSpeed = 20f;
    public float Acceleration = 8f;
    public float GroundDistance;
    public float RotationSpeed = 270f;
    public float DragCoefficient = 3f;
    public float CenterOfMassY = -0.5f;
}
