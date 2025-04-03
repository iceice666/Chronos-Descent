using System;
using System.Collections.Generic;
using System.Linq;
using ChronosDescent.Scripts.Core;
using Godot;

namespace ChronosDescent.Scripts;

public static class Extensions
{
    public static Dictionary<T, U> ToDictionary<[MustBeVariant] T, [MustBeVariant] U>
        (this Godot.Collections.Dictionary<T, U> dict)
    {
        var result = new Dictionary<T, U>();

        foreach (var (k, v) in dict) result.Add(k, v);

        return result;
    }

    public static Godot.Collections.Dictionary<T, U> ToDictionary<[MustBeVariant] T, [MustBeVariant] U>
        (this Dictionary<T, U> dict)
    {
        var result = new Godot.Collections.Dictionary<T, U>();
        foreach (var (k, v) in dict) result.Add(k, v);

        return result;
    }

    /// <summary>
    ///     Set the text of a Label with translation
    /// </summary>
    public static void SetTextTr(this Label label, string key)
    {
        label.Text = TranslationManager.Tr(key);
    }

    /// <summary>
    ///     Set the text of a Label with translation and formatting
    /// </summary>
    public static void SetTextTrFormat(this Label label, string key, params object[] args)
    {
        label.Text = TranslationManager.TrFormat(key, args);
    }

    /// <summary>
    ///     Set the text of a Button with translation
    /// </summary>
    public static void SetTextTr(this Button button, string key)
    {
        button.Text = TranslationManager.Tr(key);
    }

    /// <summary>
    ///     Set the tooltip of a Control with translation
    /// </summary>
    public static void SetTooltipTextTr(this Control control, string key)
    {
        control.TooltipText = TranslationManager.Tr(key);
    }

    /// <summary>
    ///     Set the tooltip of a Control with translation and formatting
    /// </summary>
    public static void SetTooltipTextTrFormat(this Control control, string key, params object[] args)
    {
        control.TooltipText = TranslationManager.TrFormat(key, args);
    }

    /// <summary>
    ///     Set an item's text in an OptionButton with translation
    /// </summary>
    public static void SetItemTextTr(this OptionButton optionButton, int index, string key)
    {
        optionButton.SetItemText(index, TranslationManager.Tr(key));
    }
}

public static class ListExtensions
{
    public static bool TryGet<T>(this List<T> list, int index, out T value)
    {
        if (list != null && index >= 0 && index < list.Count)
        {
            value = list[index];
            return true;
        }

        value = default;
        return false;
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