using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class MovementController : MonoBehaviour
{
    private GameObject _car;
    private IMovementType _currentMovement;
    private Dictionary<Type, IMovementType> _movementTypes = new Dictionary<Type, IMovementType>();



    [Inject]
    private void Initial(PlayerUnit player)
    {

        _car = player.gameObject;
        var movements = _car.GetComponents<IMovementType>();

        foreach (var movement in movements)
        {
            _movementTypes.Add(movement.GetType(), movement);
            movement.Deactive();
        }

        SetMovementType(typeof(WheelCarMovement));
    }
    

    public void SetMovementType(System.Type type)
    {
        if (_movementTypes.TryGetValue(type, out IMovementType newMovement))
        {

            _currentMovement?.Deactive();

            var rb = _car.GetComponent<Rigidbody>();
            rb.angularVelocity = Vector3.zero;

            _currentMovement = newMovement;
            _currentMovement.Active();

        }
        else
        {
            Debug.Log("Theres is no such " + type);
        }
    }

    public void SwitchToTank() => SetMovementType(typeof(TankStyleMovement));

    public void SwitchToHover() => SetMovementType(typeof(HoverCarMovement));

    public void SwitchToRacing() => SetMovementType(typeof(RacingCarMovement));

    public void SwitchToLevitating() => SetMovementType(typeof(LevitatingCarMovement));

    public void SwitchToWheelCar() => SetMovementType(typeof(WheelCarMovement));
}
