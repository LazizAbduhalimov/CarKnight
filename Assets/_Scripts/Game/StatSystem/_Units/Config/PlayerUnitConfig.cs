using UnityEngine;

[CreateAssetMenu(fileName = "PlayerUnitConfig", menuName = "Game/Units/Player Unit Config")]
public class PlayerUnitConfig : ScriptableObject
{
    [SerializeField] private UnitComponentConfigData _components = new();
    [SerializeField] private bool _overrideStartingMovementMode;
    [SerializeField] private MovementMode _startingMovementMode = MovementMode.WheelCar;
    [SerializeField, Min(0)] private int[] _xpLevels = { 0, 30, 60, 90, 120, 150, 180, 210, 240, 270 };

    public bool HasXpLevels => _xpLevels != null && _xpLevels.Length > 0;
    public int MaxLevel => HasXpLevels ? _xpLevels.Length : 1;

    public int EvaluateXpToNextLevel(int currentLevel)
    {
        if (!HasXpLevels)
        {
            return 0;
        }

        var safeLevel = Mathf.Max(0, currentLevel);
        if (safeLevel >= MaxLevel)
        {
            return 0;
        }

        // xpLevels[i] stores the XP cost to reach level i + 1 from the previous level.
        var targetLevel = safeLevel + 1;
        var targetLevelIndex = targetLevel - 1;
        return Mathf.Max(1, _xpLevels[targetLevelIndex]);
    }

    public void ApplyTo(PlayerUnit playerUnit, bool applyRuntimeState)
    {
        if (playerUnit == null)
        {
            return;
        }

        _components?.Apply(playerUnit.gameObject);

        if (!applyRuntimeState || !_overrideStartingMovementMode)
        {
            return;
        }

        if (playerUnit.TryGetComponent(out MovementFsmController movementController))
        {
            movementController.SwitchTo(_startingMovementMode);
        }
    }
}
