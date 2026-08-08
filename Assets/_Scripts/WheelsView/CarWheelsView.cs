using System;
using System.Collections.Generic;
using UnityEngine;

public class CarWheelsView : MonoBehaviour
{
    [Serializable]
    public struct WheelMapping
    {
        public MovementMode Mode;
        public WheelsSet Set;
    }

    [SerializeField] private List<WheelMapping> _mappings;

    private Dictionary<MovementMode, WheelMapping> _wheels = new();

    private WheelsSet _activeWheels;

    public void SwitchVisualMode(MovementMode mode)
    {
        if (_wheels.Count == 0 && _mappings.Count > 0)
        {
            foreach (var mappings in _mappings)
                _wheels[mappings.Mode] = mappings;
        }


        if (_activeWheels != null)
            _activeWheels.gameObject.SetActive(false);

        if (_wheels.TryGetValue(mode, out WheelMapping mapping))
        {
            _activeWheels = mapping.Set;
            if (_activeWheels != null)
                _activeWheels.gameObject.SetActive(true);
        }
    }

    public void UpdateRotation(float turnInput)
    {
        if (_activeWheels != null)
            _activeWheels.Turn(turnInput);
    }
}