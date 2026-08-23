using Pooshit.Ai.Extern;

namespace NightlyCode.Ai.Tests;

class RecordingRng : IRng {
    readonly IRng inner;

    public RecordingRng(IRng inner) => this.inner = inner;

    public int CallCount { get; private set; }

    public long NextLong() {
        ++CallCount;
        return inner.NextLong();
    }

    public int NextInt() {
        ++CallCount;
        return inner.NextInt();
    }

    public int NextInt(int max) {
        ++CallCount;
        return inner.NextInt(max);
    }

    public float NextFloat() {
        ++CallCount;
        return inner.NextFloat();
    }

    public float NextFloatRange() {
        ++CallCount;
        return inner.NextFloatRange();
    }

    public double NextDouble() {
        ++CallCount;
        return inner.NextDouble();
    }
}
