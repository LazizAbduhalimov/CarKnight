using System;
using UnityEngine;

[Serializable]public struct MovementModeDisplayData
{
    public MovementMode movementMode;

    public Sprite icon;

    public string Title;

    [TextArea] public string Description;
}
