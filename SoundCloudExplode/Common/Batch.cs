using System.Collections.Generic;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode.Common;

/// <summary>
/// Generic collection of items returned by a single request.
/// </summary>
/// <remarks>
/// Initializes an instance of <see cref="Batch{T}" />.
/// </remarks>
public class Batch<T>(IReadOnlyList<T> items)
    where T : IBatchItem
{
    /// <summary>
    /// Items included in the batch.
    /// </summary>
    public IReadOnlyList<T> Items { get; } = items;
}

internal static class Batch
{
    public static Batch<T> Create<T>(IReadOnlyList<T> items)
        where T : IBatchItem => new(items);
}

internal static class BatchExtensions
{
    public static IAsyncEnumerable<T> FlattenAsync<T>(this IAsyncEnumerable<Batch<T>> source)
        where T : IBatchItem => source.SelectManyAsync(b => b.Items);
}
