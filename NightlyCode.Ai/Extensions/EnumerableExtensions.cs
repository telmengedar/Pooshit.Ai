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
}