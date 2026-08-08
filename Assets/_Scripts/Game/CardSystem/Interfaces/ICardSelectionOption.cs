public interface ICardSelectionOption
{
    CardChoiceViewData ViewData { get; }
    bool CanSelect();
    void Select();
}
