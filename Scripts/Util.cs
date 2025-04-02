using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ChronosDescent.Scripts;

public class Utils
{
    public static Dictionary<T, U> ToDictionary<[MustBeVariant] T, [MustBeVariant] U>
        (Godot.Collections.Dictionary<T, U> dict)
    {
        var result = new Dictionary<T, U>();

        foreach (var (k, v) in dict) result.Add(k, v);

        return result;
    }

    public static Godot.Collections.Dictionary<T, U> ToDictionary<[MustBeVariant] T, [MustBeVariant] U>
        (Dictionary<T, U> dict)
    {
        var result = new Godot.Collections.Dictionary<T, U>();
        foreach (var (k, v) in dict) result.Add(k, v);

        return result;
    }
}

public static class EnumerableExtensions
{
    // Pick a single random item using System.Random
    public static T PickRandom<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var list = source as IList<T> ?? source.ToList();

        if (list.Count == 0)
            throw new InvalidOperationException("The source sequence is empty.");

        var random = new Random();
        return list[random.Next(0, list.Count)];
    }

    // Pick a single random item using Godot's RandomNumberGenerator
    public static T PickRandom<T>(this IEnumerable<T> source, RandomNumberGenerator rng)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rng);

        var list = source as IList<T> ?? source.ToList();

        if (list.Count == 0)
            throw new InvalidOperationException("The source sequence is empty.");

        return list[rng.RandiRange(0, list.Count - 1)];
    }

    // Pick multiple random items using System.Random
    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");

        var list = source as IList<T> ?? source.ToList();

        if (count > list.Count)
            throw new ArgumentOutOfRangeException(nameof(count),
                "Count cannot be greater than the number of elements in the source.");

        var random = new Random();
        return PickRandomInternal(list, count, index => random.Next(0, index));
    }

    // Pick multiple random items using Godot's RandomNumberGenerator
    public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, RandomNumberGenerator rng, int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(rng);

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");

        var list = source as IList<T> ?? source.ToList();

        if (count > list.Count)
            throw new ArgumentOutOfRangeException(nameof(count),
                "Count cannot be greater than the number of elements in the source.");

        return PickRandomInternal(list, count, index => rng.RandiRange(0, index - 1));
    }

    // Internal helper for more efficient random selection
    private static IEnumerable<T> PickRandomInternal<T>(IList<T> list, int count, Func<int, int> randomIndexGenerator)
    {
        // Create a working copy of the original list
        var workingList = new List<T>(list);
        var result = new List<T>(count);

        // For optimal performance, select items using Fisher-Yates sampling
        var n = workingList.Count;
        for (var i = 0; i < count; i++)
        {
            // Generate random index from the remaining elements
            var randomIndex = randomIndexGenerator(n - i);

            // Add the randomly selected item to the result
            result.Add(workingList[randomIndex]);

            // Move the last non-selected item to the selected position
            // (this avoids the need for removing items which is O(n))
            if (randomIndex < n - i - 1) workingList[randomIndex] = workingList[n - i - 1];
        }

        return result;
    }
}