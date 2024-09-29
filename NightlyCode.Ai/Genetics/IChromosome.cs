using NightlyCode.Ai.Net.Configurations;

namespace NightlyCode.Ai.Genetics;

/// <summary>
/// chromosome used in evolution of data
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
public interface IChromosome<T> {
    
    /// <summary>
    /// randomizes configuration of this chromosome
    /// </summary>
    void Randomize(CrossSetup setup=null);

    /// <summary>
    /// a hash value representing the chromosome structure (without weights)
    /// </summary>
    /// <returns>hash value</returns>
    int StructureHash();
    
    /// <summary>
    /// modifier for fitness to push certain structures
    /// </summary>
    float FitnessModifier { get; }
}