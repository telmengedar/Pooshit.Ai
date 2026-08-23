using Pooshit.Ai.Extern;

namespace NightlyCode.Ai.Tests;

class SequenceRng : IRng {
    readonly int[] values;
    int index;
    int longIndex;
    int intIndex;
    int floatIndex;
    int doubleIndex;

    public SequenceRng(params int[] values) => this.values = values;

    public List<int> Bounds { get; } = new();

    public long[] LongValues { get; init; } = [];

    public int[] IntValues { get; init; } = [];

    public float[] FloatValues { get; init; } = [];

    public double[] DoubleValues { get; init; } = [];

    public long NextLong() {
        if (longIndex >= LongValues.Length)
            throw new NotSupportedException($"{nameof(SequenceRng)}.{nameof(NextLong)} was called more times than scripted ({LongValues.Length} value(s) provided)");
        return LongValues[longIndex++];
    }


    public int NextInt() {
        if (intIndex >= IntValues.Length)
            throw new NotSupportedException($"{nameof(SequenceRng)}.{nameof(NextInt)} was called more times than scripted ({IntValues.Length} value(s) provided)");
        return IntValues[intIndex++];
    }


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
