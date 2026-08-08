using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Zenject;

public class MovementSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject _movementHolder;

    private GameObject _car;

    private IMovementType _currentMovement;
    private MonoBehaviour _currentComponent;

    private Dictionary<Type, MonoBehaviour> _movementTypes = new();

    [Inject]
    private void Initial(PlayerUnit player)
    {
        _car = player.gameObject;

        var movements = _movementHolder.GetComponents<MonoBehaviour>();

        foreach (var movement in movements)
        {
            if (movement is IMovementType)
            {
                _movementTypes.Add(movement.GetType(), movement);
                movement.gameObject.SetActive(false);
            }
        }

        SetMovementType(typeof(WheelCarMovement));
    }

    public void SetMovementType(Type type)
    {
        DeactiveCurrent();

        var rb = _car.GetComponent<Rigidbody>();
        if (rb != null)
            rb.angularVelocity = Vector3.zero;

        if (!_movementTypes.TryGetValue(type, out var source))
        {
            Debug.LogError($"Movement type {type} not found in holder");
            return;
        }

        var newComponent = _car.AddComponent(type) as MonoBehaviour;

        CopyFields(source, newComponent);

        _currentComponent = newComponent;
        _currentMovement = newComponent as IMovementType;

        _currentMovement.Active();
    }

    private void DeactiveCurrent()
    {
        if (_currentMovement == null) return;

        _currentMovement.Deactive();
        Destroy(_currentComponent);

        _currentMovement = null;
        _currentComponent = null;
    }

    private void CopyFields(MonoBehaviour source, MonoBehaviour target)
    {
        var fields = source.GetType().GetFields(
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic
        );

        foreach (var field in fields)
        {
            if (field.IsPublic || field.IsDefined(typeof(SerializeField), true))
            {
                field.SetValue(target, field.GetValue(source));
            }
        }
    }


    public void SwitchToTank() => SetMovementType(typeof(TankStyleMovement));
    public void SwitchToHover() => SetMovementType(typeof(HoverCarMovement));
    public void SwitchToRacing() => SetMovementType(typeof(RacingCarMovement));
    public void SwitchToLevitating() => SetMovementType(typeof(LevitatingCarMovement));
    public void SwitchToWheelCar() => SetMovementType(typeof(WheelCarMovement));
}