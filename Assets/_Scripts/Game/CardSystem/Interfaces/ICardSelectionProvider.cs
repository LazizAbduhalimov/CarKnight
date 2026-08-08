using System.Collections.Generic;

public interface ICardSelectionProvider
{
    IReadOnlyList<ICardSelectionOption> GetOptions();
}
