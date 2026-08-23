using Pooshit.Ai.Extern;

namespace Pooshit.Ai.Extensions;

/// <summary>
/// extensions for enumerations
/// </summary>
static class EnumerableExtensions {
    public static T RandomItem<T>(this IEnumerable<T> source, IRng rng) {
        T[] elements = source as T[] ?? source.ToArray();
        if (elements.Length == 0)
            return default;
        return elements[rng.NextInt(elements.Length)];
    }


    /// <summary>
    /// draws distinct elements at random from a list without replacement
    /// </summary>
    /// <param name="source">list to draw from</param>
    /// <param name="rng">random number generator to use</param>
    /// <param name="count">number of elements to draw</param>
    /// <returns>randomly drawn elements, clamped to <paramref name="source"/>'s length</returns>
    public static T[] RandomSample<T>(this IReadOnlyList<T> source, IRng rng, int count) {
        int tail = source.Count;
        T[] result = new T[Math.Clamp(count, 0, tail)];
        Dictionary<int, T> displaced = new();

        for (int i = 0; i < result.Length; i++) {
            tail--;
            int pick = rng.NextInt(tail + 1);
            result[i] = displaced.TryGetValue(pick, out T value) ? value : source[pick];
            if (pick != tail)
                displaced[pick] = displaced.TryGetValue(tail, out T tailValue) ? tailValue : source[tail];
        }

        return result;
    }
}
