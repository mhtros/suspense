namespace Suspense.Server.Extensions;

/// <summary>
/// Provides extension methods for working with groupings.
/// </summary>
public static class GroupingExtensions
{
    /// <summary>
    /// Converts an <see cref="IEnumerable{T}"/> of <see cref="IGrouping{TKey, TElement}"/>
    /// to a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TElement">The type of the elements in the groups.</typeparam>
    /// <param name="groupings">The groupings to convert to a dictionary.</param>
    /// <returns>A dictionary where the keys are the group keys and the values are lists of the grouped elements.</returns>
    public static Dictionary<TKey, List<TElement>> ToDictionary<TKey, TElement>(
        this IEnumerable<IGrouping<TKey, TElement>> groupings) where TKey : notnull
    {
        return groupings.ToDictionary(group => group.Key, group => group.ToList());
    }
}