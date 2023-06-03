using System;
using System.Collections.Generic;

namespace Bannerlord.BLSE.Shared.Utils;

public static class Extensions
{
    public static TSource? MaxByOrDefault<TSource, TKey>(this IEnumerable<TSource> enumerable, Func<TSource, TKey> selector, IComparer<TKey> comparer, out TKey? maxKey)
    {
        if (enumerable == null)
            throw new ArgumentNullException(nameof (enumerable));
        if (selector == null)
            throw new ArgumentNullException(nameof (selector));
        if (comparer == null)
            throw new ArgumentNullException(nameof (comparer));

        using var enumerator = enumerable.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            maxKey = default;
            return default;
        }

        var source = enumerator.Current;
        maxKey = selector(source);
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            var x = selector(current);
            if (comparer.Compare(x, maxKey) <= 0) continue;
            source = current;
            maxKey = x;
        }
        return source;
    }
}