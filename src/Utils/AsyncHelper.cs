using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MinecraftWebExporter.Utils;

/// <summary>
/// A helper class to add some async methods
/// </summary>
public static class AsyncHelper
{
    /// <summary>
    /// Runs the given <paramref name="source"/> in parallel with the <paramref name="funcAsync"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="funcAsync"></param>
    /// <param name="partitions"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> funcAsync, int partitions = 4)
    {
        async Task AwaitPartition(IEnumerator<T> partition)
        {
            using (partition)
            {
                while (partition.MoveNext())
                {
                    await Task.Yield();
                    await funcAsync(partition.Current);
                }
            }
        }

        return Task.WhenAll(
            Partitioner
                .Create(source)
                .GetPartitions(partitions)
                .AsParallel()
                .Select(AwaitPartition));
    }
}