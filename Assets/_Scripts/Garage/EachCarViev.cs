using System.Collections;
using Unity.InferenceEngine;
using UnityEngine;

public class EachCarViev : MonoBehaviour
{
    [Header("Car Model")]
    [SerializeField] private GameObject _model;

    [Header("Wheels/Movement Visuals")]
    public MovementMode baseMovement;

    [SerializeField] private GameObject _hoverWheels;
    [SerializeField] private GameObject _levitatingWheels; 
    [SerializeField] private GameObject _racerWheels;  
    [SerializeField] private GameObject _tankWheels;   
    [SerializeField] private GameObject _basicWheels;

    [SerializeField] private GameObject _buttons;
    private GameObject _activeWheels;



    public void Activate()
    {
        _model.SetActive(true);
        SetMovementMode(baseMovement);
        _buttons.SetActive(true);
    }


    public void Deactivate()
    {
        _buttons?.SetActive(false);
        _activeWheels?.SetActive(false);
        _model.SetActive(false);
        _activeWheels = null;
    }


    public void SetMovementMode(MovementMode mode)
    {
        _activeWheels?.SetActive(false);

        _activeWheels = mode switch
        {
            MovementMode.HoverCar => _hoverWheels,
            MovementMode.LevitatingCar => _levitatingWheels,
            MovementMode.RacingCar => _racerWheels,
            MovementMode.TankStyle => _tankWheels,
            MovementMode.WheelCar => _basicWheels,
            _ => _basicWheels
        };

        _activeWheels?.SetActive(true);
    }
}
