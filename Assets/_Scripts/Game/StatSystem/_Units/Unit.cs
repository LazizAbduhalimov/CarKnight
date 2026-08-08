using Sirenix.OdinInspector;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public HealthComponent HealthComponent;
    [ReadOnly] public TrailRenderer[] WheelsTrails;

    protected virtual void OnValidate()
    {
        HealthComponent ??= GetComponent<HealthComponent>();
    }

    protected virtual void Awake()
    {
        WheelsTrails = GetComponentsInChildren<TrailRenderer>();
    }

    protected virtual void OnEnable()
    {
        foreach (var wheel in WheelsTrails)
        {
            wheel.Clear();
        }
    }
}
