namespace Pooshit.Ai.Genetics;

/// <summary>
/// evaluated entry in <see cref="Population{T}"/>
/// </summary>
/// <typeparam name="T">type of chromosome</typeparam>
public class PopulationEntry<T>
where T : IChromosome<T>
{
    
    /// <summary>
    /// chromosome in population
    /// </summary>
    public T Chromosome { get; set; }

    /// <summary>
    /// fitness of chromosome
    /// </summary>
    public float Fitness { get; set; }
}