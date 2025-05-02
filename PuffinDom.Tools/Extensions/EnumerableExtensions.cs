using System.Collections;

namespace PuffinDom.Tools.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> DistinctExcept<T>(this IEnumerable<T> source, T exclusionValue)
    {
        ArgumentNullException.ThrowIfNull(source);

        return DistinctExceptIterator(source, exclusionValue, EqualityComparer<T>.Default);
    }

    private static IEnumerable<T> DistinctExceptIterator<T>(IEnumerable<T> source, T exclusionValue, IEqualityComparer<T> comparer)
    {
        var seenElements = new HashSet<T>(comparer);

        foreach (var element in source)
            if (comparer.Equals(element, exclusionValue) || seenElements.Add(element))
                yield return element;
    }
    
    public static bool IsEmpty<T>(this IEnumerable<T> items)
    {
        if (items is ICollection collection)
            return collection.Count == 0;

        return !items.Any();
    }
    
    public static bool IsNotEmpty<T>(this IEnumerable<T> items)
    {
        return !items.IsEmpty();
    }
}