using NightlyCode.Ai.Net;

namespace NightlyCode.Ai.Genetics;

/// <summary>
/// chromosome which evolves by crossing it with another
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
public interface ICrossChromosome<T> : IChromosome<T> {
    
    /// <summary>
    /// creates a new chromosome by pairing this chromosome with another
    /// </summary>
    /// <param name="other">chromosome to cross this with</param>
    /// <param name="setup">mutation setup</param>
    /// <returns>generated chromosome</returns>
    T Cross(T other, CrossSetup setup);
}