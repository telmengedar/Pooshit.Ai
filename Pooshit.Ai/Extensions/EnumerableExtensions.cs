using Pooshit.Ai.Extern;

namespace Pooshit.Ai.Extensions;

/// <summary>
/// extensions for enumerations
/// </summary>
static class EnumerableExtensions {
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, IRng rng) {
        T[] elements = source as T[] ?? source.ToArray();
        for (int i = elements.Length - 1; i >= 0; i--)
        {
            // Swap element "i" with a random earlier element it (or itself)
            // ... except we don't really need to swap it fully, as we can
            // return it immediately, and afterwards it's irrelevant.
            int swapIndex = rng.NextInt(i + 1);
            yield return elements[swapIndex];
            elements[swapIndex] = elements[i];
        }
    }

    public static T RandomItem<T>(this IEnumerable<T> source, IRng rng) {
        T[] elements = source as T[] ?? source.ToArray();
        if (elements.Length == 0)
            return default;
        return elements[rng.NextInt(elements.Length)];
    }
}