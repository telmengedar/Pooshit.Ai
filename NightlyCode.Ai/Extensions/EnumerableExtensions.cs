using NightlyCode.Ai.Extern;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Extensions;

/// <summary>
/// extensions for enumerations
/// </summary>
static class EnumerableExtensions {
    
    /// <summary>
    /// generates a forward for loop which generates items based on the iteration variable
    /// </summary>
    /// <param name="start">start of loop</param>
    /// <param name="length">end of loop (exclusive)</param>
    /// <param name="constructor">constructor used to generate result items</param>
    /// <typeparam name="T">type of item to generate</typeparam>
    /// <returns>enumeration of generated items</returns>
    public static IEnumerable<T> For<T>(int start, int length, Func<int, T> constructor) {
        for(int i=start;i<length;++i)
            yield return constructor(i);
    }

    public static int IndexOf<T>(this IEnumerable<T> enumeration, Func<T, bool> predicate) {
        int index = 0;
        foreach (T item in enumeration) {
            if (predicate(item))
                return index;
            ++index;
        }

        return -1;
    }
    
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
}