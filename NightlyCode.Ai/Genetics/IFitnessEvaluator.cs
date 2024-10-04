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
    /// <returns>fitness of chromosome</returns>
    float EvaluateFitness(T chromosome);
}