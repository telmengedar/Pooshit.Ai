namespace NightlyCode.Ai.Extern;

public interface IRng {
    /// <summary>
    /// generates a random int64 number
    /// </summary>
    /// <returns>random int64</returns>
    long NextLong();

    /// <summary>
    /// generates a random int32 number
    /// </summary>
    /// <returns>random int32</returns>
    int NextInt();

    /// <summary>
    /// generates a random int32 number in the range of [0..max[
    /// </summary>
    /// <returns>random int32</returns>
    int NextInt(int max);

    /// <summary>
    /// generates a random single precision float number
    /// </summary>
    /// <returns>random float</returns>
    float NextFloat();

    /// <summary>
    /// generates a random single precision float number in the range of [-1..1]
    /// </summary>
    /// <returns>random single precision float</returns>
    float NextFloatRange();

    /// <summary>
    /// generates a random double precision float
    /// </summary>
    /// <returns>random double precision float</returns>
    double NextDouble();
}