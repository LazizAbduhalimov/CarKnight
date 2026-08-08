using UnityEngine;

public readonly struct CardChoiceViewData
{
    public readonly Sprite Icon;
    public readonly string Title;
    public readonly string Description;

    public CardChoiceViewData(Sprite icon, string title, string description)
    {
        Icon = icon;
        Title = title;
        Description = description;
    }
}
