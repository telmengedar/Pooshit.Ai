using Pooshit.Ai.Net;
using Pooshit.Ai.Neurons;

namespace Pooshit.Ai.Genetics;

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
    
    /// <summary>
    /// tries to optimize connections of the chromosome
    /// </summary>
    /// <param name="test">function used to test an optimized candidate</param>
    /// <returns>optimized chromosome</returns>
    T Optimize(Func<T, bool> test);
    
    /// <summary>
    /// access to neurons
    /// </summary>
    NeuronConfig[] Neurons { get; }
}