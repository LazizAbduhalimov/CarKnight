using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CompositeSelectionProvider : ICardSelectionProvider
{
    private readonly ICardSelectionProvider[] _providers;

    public CompositeSelectionProvider(params ICardSelectionProvider[] providers)
    {
        _providers = providers;
    }

    public IReadOnlyList<ICardSelectionOption> GetOptions()
    {
        var allOptions = new List<ICardSelectionOption>();

        foreach (var provider in _providers)
        {
            allOptions.AddRange(provider.GetOptions());
        }

        return allOptions.OrderBy(x => Random.value).Take(3).ToList();
    }
}
