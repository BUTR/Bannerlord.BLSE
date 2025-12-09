using System;
using System.Collections.Generic;
using System.Linq;

namespace Bannerlord.LauncherEx.Extensions;

internal static class EnumerableExtensions
{
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        return source.GroupBy(keySelector).Select(x => x.First());
    }
}