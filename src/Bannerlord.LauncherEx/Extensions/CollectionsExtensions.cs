using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Bannerlord.ModuleManager;

internal static class CollectionsExtensions
{
    public static int IndexOf<T>(this IReadOnlyList<T> self, T elementToFind)
    {
        var i = 0;
        foreach (T element in self)
        {
            if (Equals(element, elementToFind))
                return i;
            i++;
        }
        return -1;
    }
}