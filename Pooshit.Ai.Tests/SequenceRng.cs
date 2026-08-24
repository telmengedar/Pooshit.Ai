using Pooshit.Ai.Extern;

namespace NightlyCode.Ai.Tests;

class SequenceRng : IRng {
    readonly int[] values;
    int index;
    int floatIndex;
    int doubleIndex;

    public SequenceRng(params int[] values) => this.values = values;

    public List<int> Bounds { get; } = new();

    public float[] FloatValues { get; init; } = [];

    public double[] DoubleValues { get; init; } = [];

    /// <summary>
    /// never called by any production code path - <c>Pooshit.Ai</c> draws exclusively through
    /// <see cref="NextInt(int)"/>, <see cref="NextFloat"/> and <see cref="NextDouble"/> (verified
    /// by inventory across the production project). Not scriptable; throwing documents that no
    /// test should ever need to script it (R3, test README) - the unconsumed-double condition
    /// (design #9072 §16 addendum) removed the scripting surface this used to carry.
    /// </summary>
    public long NextLong() => throw new NotSupportedException($"{nameof(SequenceRng)}.{nameof(NextLong)} is never called by any Pooshit.Ai production code path and is not scriptable");


    /// <summary>
    /// never called by any production code path - see <see cref="NextLong"/>.
    /// </summary>
    public int NextInt() => throw new NotSupportedException($"{nameof(SequenceRng)}.{nameof(NextInt)}() is never called by any Pooshit.Ai production code path and is not scriptable");


    public int NextInt(int max) {
        if (index >= values.Length)
            throw new NotSupportedException($"{nameof(SequenceRng)}.{nameof(NextInt)}(max) was called more times than scripted ({values.Length} value(s) provided)");
        Bounds.Add(max);
        int value = values[index++];
        if (value < 0 || value >= max)
            throw new ArgumentOutOfRangeException(nameof(max), $"scripted value {value} is outside [0,{max})");
        return value;
    }


    public float NextFloat() {
        if (floatIndex >= FloatValues.Length)
            throw new NotSupportedException($"{nameof(SequenceRng)}.{nameof(NextFloat)} was called more times than scripted ({FloatValues.Length} value(s) provided)");
        float value = FloatValues[floatIndex++];
        if (value < 0.0f || value >= 1.0f)
            throw new ArgumentOutOfRangeException(nameof(value), $"scripted value {value} is outside [0,1)");
        return value;
    }


    public float NextFloatRange() => throw new NotSupportedException();


    public double NextDouble() {
        if (doubleIndex >= DoubleValues.Length)
            throw new NotSupportedException($"{nameof(SequenceRng)}.{nameof(NextDouble)} was called more times than scripted ({DoubleValues.Length} value(s) provided)");
        double value = DoubleValues[doubleIndex++];
        if (value < 0.0 || value >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), $"scripted value {value} is outside [0,1)");
        return value;
    }
}
