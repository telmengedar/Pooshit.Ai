using Pooshit.Ai.Extern;

namespace Pooshit.Ai.Genetics;

/// <summary>
/// chromosome which evolves by pure mutation
/// </summary>
/// <typeparam name="T">type of configuration</typeparam>
public interface IMutatingChromosome<T> : IChromosome<T> {
    
    /// <summary>
    /// mutates the chromosome
    /// </summary>
    /// <returns>mutated chromosome</returns>
    T Mutate(IRng rng, float mutationRange);
}