using UnityEngine;

public class PlayerUnit : Unit
{
    [SerializeField] private PlayerUnitConfig _config;
    public PlayerUnitConfig Config => _config;

    protected override void Awake()
    {
        base.Awake();
        ApplyConfig(applyRuntimeState: false);
    }

    protected override void OnEnable()
    {
        ApplyConfig(applyRuntimeState: true);
        base.OnEnable();
    }

    public void SetConfig(PlayerUnitConfig config, bool applyNow = true)
    {
        _config = config;
        if (applyNow)
        {
            ApplyConfig(applyRuntimeState: true);
        }
    }

    public void ApplyConfig(bool applyRuntimeState = true)
    {
        if (_config == null)
        {
            return;
        }

        _config.ApplyTo(this, applyRuntimeState);
    }
}
