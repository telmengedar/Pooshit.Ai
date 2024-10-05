using NightlyCode.Ai.Extern;

namespace NightlyCode.Ai.Genetics;

/// <summary>
/// used to evaluate fitness of a training set
/// </summary>
public interface IFitnessEvaluator<T> 
where T : IChromosome<T>
{
    /// <summary>
    /// evaluates the fitness of the specified chromosomes
    /// </summary>
    /// <param name="chromosome">chromosome to check</param>
    /// <param name="rng">rng to use</param>
    /// <param name="fullSet">flag whether to force to train the full sample set</param>
    /// <returns>fitness of chromosome</returns>
    float EvaluateFitness(T chromosome, IRng rng, bool fullSet);
}