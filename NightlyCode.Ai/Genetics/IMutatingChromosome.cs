using NightlyCode.Ai.Net.Configurations;

namespace NightlyCode.Ai.Genetics;

/// <summary>
/// chromosome which evolves by pure mutation
/// </summary>
/// <typeparam name="T">type of configuration</typeparam>
public interface IMutatingChromosome<T> : IChromosome<T> {

    /// <summary>
    /// mutates the chromosome
    /// </summary>
    /// <param name="setup">setup to apply</param>
    /// <returns>mutated chromosome</returns>
    T Mutate(CrossSetup setup);
}