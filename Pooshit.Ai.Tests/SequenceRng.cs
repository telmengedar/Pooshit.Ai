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

    public long NextLong() => LongValues[longIndex++];


    public int NextInt() => IntValues[intIndex++];


    public int NextInt(int max) {
        Bounds.Add(max);
        int value = values[index++];
        if (value < 0 || value >= max)
            throw new ArgumentOutOfRangeException(nameof(max), $"scripted value {value} is outside [0,{max})");
        return value;
    }


    public float NextFloat() {
        float value = FloatValues[floatIndex++];
        if (value < 0.0f || value >= 1.0f)
            throw new ArgumentOutOfRangeException(nameof(value), $"scripted value {value} is outside [0,1)");
        return value;
    }


    public float NextFloatRange() => throw new NotSupportedException();


    public double NextDouble() {
        double value = DoubleValues[doubleIndex++];
        if (value < 0.0 || value >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), $"scripted value {value} is outside [0,1)");
        return value;
    }
}
