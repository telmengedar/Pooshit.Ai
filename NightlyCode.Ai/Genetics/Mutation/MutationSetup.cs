namespace NightlyCode.Ai.Genetics.Mutation;

/// <summary>
/// setup data for mutation rates
/// </summary>
public class MutationSetup {

    /// <summary>
    /// chance for chromosome to mutate
    /// </summary>
    public float Chance { get; set; } = 0.1f;
    
    /// <summary>
    /// rate of mutation when population evolves
    /// </summary>
    public float Rate { get; set; } = 0.07f;

    /// <summary>
    /// range of value mutation on evolution
    /// </summary>
    public float Range { get; set; } = 0.25f;

    /// <summary>
    /// factor by which mutation rates are multiplied when chromosomes match
    /// </summary>
    public float IncestFactor { get; set; } = 7.5f;

}