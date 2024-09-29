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
    
    /// <summary>
    /// retrieves fitness value for a series of fitness test results
    /// </summary>
    /// <param name="input">fitness test results</param>
    /// <param name="aggregate">aggregate function to use</param>
    /// <returns>fitness value</returns>
    public static float Fitness(this IEnumerable<float> input, AggregateType aggregate) {
        /*double[] values = input.ToArray();
        if (values.Length == 0)
            return -1.0;

        if (values.Any(v => double.IsNaN(v) || double.IsInfinity(v) || double.IsNegativeInfinity(v)))
            return -1.0;*/

        float value;
        switch (aggregate) {
            default:
            case AggregateType.Sum:
                value = input.Sum();
                break;
            case AggregateType.Average:
                value = input.Average();
                break;
            case AggregateType.Median:
                float[] values = input.ToArray();
                Array.Sort(values);
                int middle = values.Length / 2;
                value = values[middle];
                break;
            case AggregateType.Min:
                value = input.Min();
                break;
            case AggregateType.Max:
                value = input.Max();
                break;
        }

        if (double.IsNaN(value) || double.IsInfinity(value) || double.IsNegativeInfinity(value))
            return -1.0f;
        return value;
    }
}