using Pooshit.Ai.Extern;

namespace NightlyCode.Ai.Tests;

class SequenceRng : IRng {
    readonly int[] values;
    int index;

    public SequenceRng(params int[] values) => this.values = values;

    public long NextLong() => throw new NotSupportedException();


    public int NextInt() => throw new NotSupportedException();


    public int NextInt(int max) => values[index++];


    public float NextFloat() => throw new NotSupportedException();


    public float NextFloatRange() => throw new NotSupportedException();


    public double NextDouble() => throw new NotSupportedException();
}
