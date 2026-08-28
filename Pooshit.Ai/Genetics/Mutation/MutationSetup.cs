namespace Pooshit.Ai.Genetics.Mutation;

/// <summary>
/// setup data for mutation rates
/// </summary>
public class MutationSetup {

    /// <summary>
    /// chance for chromosome to mutate, read only where the population binds the cross reproduction strategy
    /// </summary>
    public float Chance { get; set; } = 0.1f;
    
    /// <summary>
    /// rate of mutation when population evolves, read only where the population binds the cross reproduction strategy
    /// </summary>
    public float Rate { get; set; } = 0.07f;

    /// <summary>
    /// range of value mutation on evolution, read by both reproduction strategies
    /// </summary>
    public float Range { get; set; } = 0.25f;

    /// <summary>
    /// factor by which mutation rates are multiplied when chromosomes match, read only where the population binds the cross reproduction strategy
    /// </summary>
    public float IncestFactor { get; set; } = 7.5f;

    /// <summary>
    /// number of mutation runs, read only where the population binds the mutate reproduction strategy
    /// </summary>
    public int Runs { get; set; } = 1;
}