namespace Pooshit.Ai.Extern;

/// <summary>
/// rng using xor-shift implementation
/// </summary>
public class LockedRng : IRng {
    readonly object valueLock = new();
    long value;

    /// <summary>
    /// creates a new <see cref="Rng"/>
    /// </summary>
    /// <param name="seed">seed to use for rng</param>
    public LockedRng(long seed=0) {
        value = seed;
        if (value == 0)
            value = Environment.TickCount64; // can never be 0 (i guess)
    }

    /// <summary>
    /// generates a random int64 number
    /// </summary>
    /// <returns>random int64</returns>
    public long NextLong() {
        lock (valueLock) {
            value ^= value << 21;
            value ^= (value >> 35) | (value << 29);
            value ^= value << 4;
            return value;
        }
    }

    /// <summary>
    /// generates a random int32 number
    /// </summary>
    /// <returns>random int32</returns>
    public int NextInt() {
        return (int)NextLong();
    }

    /// <summary>
    /// generates a random int32 number in the range of [0..max[
    /// </summary>
    /// <returns>random int32</returns>
    public int NextInt(int max) {
        int value = NextInt() % max;
        if(value < 0) value += max;
        return value;
    }
    
    /// <summary>
    /// generates a random single precision float number
    /// </summary>
    /// <returns>random float</returns>
    public float NextFloat() {
        return (float)Math.Abs(NextLong()) / long.MaxValue;
    }

    /// <summary>
    /// generates a random single precision float number in the range of [-1..1]
    /// </summary>
    /// <returns>random single precision float</returns>
    public float NextFloatRange() {
        return (float)NextLong() / long.MaxValue;
    }

    /// <summary>
    /// generates a random double precision float
    /// </summary>
    /// <returns>random double precision float</returns>
    public double NextDouble() {
        return (double)Math.Abs(NextLong()) / long.MaxValue;
    }
}