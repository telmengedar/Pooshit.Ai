using Pooshit.Ai.Extern;

namespace NightlyCode.Ai.Tests;

class SequenceRng : IRng {
    readonly int[] values;
    int index;

    public SequenceRng(params int[] values) => this.values = values;

    public List<int> Bounds { get; } = new();

    public long NextLong() => throw new NotSupportedException();


    public int NextInt() => throw new NotSupportedException();


    public int NextInt(int max) {
        Bounds.Add(max);
        int value = values[index++];
        if (value < 0 || value >= max)
            throw new ArgumentOutOfRangeException(nameof(max), $"scripted value {value} is outside [0,{max})");
        return value;
    }


    public float NextFloat() => throw new NotSupportedException();


    public float NextFloatRange() => throw new NotSupportedException();


    public double NextDouble() => throw new NotSupportedException();
}
