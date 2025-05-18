using System.Globalization;
using System.Runtime.Serialization;

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
    /// id of origin structure
    /// </summary>
    public Guid AncestryId { get; set; }
    
    /// <summary>
    /// fitness of chromosome
    /// </summary>
    public float Fitness { get; set; }

    /// <summary>
    /// value used for fitness selection
    /// </summary>
    [IgnoreDataMember]
    public float FitnessSelector { get; set; }
    
    public override string ToString() => Fitness.ToString(CultureInfo.InvariantCulture);
}