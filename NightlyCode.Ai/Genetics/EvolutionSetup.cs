using NightlyCode.Ai.Genetics.Mutation;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Genetics;

/// <summary>
/// configuration values used for evolution
/// </summary>
/// <typeparam name="T">type of chromosome to evolve</typeparam>
public class EvolutionSetup<T> 
where T : IChromosome<T>
{
    
    /// <summary>
    /// functions to use to run the chromosomes
    /// </summary>
    public Func<T, float>[] TrainingSet { get; set; }

    /// <summary>
    /// fitness threshold when training is complete
    /// </summary>
    /// <remarks>
    /// when this fitness value is reached by a chromosome, the training stops
    /// </remarks>
    public float TargetFitness { get; set; } = float.Epsilon;
    
    /// <summary>
    /// maximum number of generations to train
    /// </summary>
    public int Runs { get; set; } = 10000;
    
    /// <summary>
    /// rate of chromosomes which is copied to next evolution
    /// </summary>
    public double Elitism { get; set; } = 0.3;

    /// <summary>
    /// rates for mutation
    /// </summary>
    public MutationSetup Mutation { get; } = new();
    
    /// <summary>
    /// action executed after training run
    /// </summary>
    public Action<int, double> AfterRun { get; set; }
    
    /// <summary>
    /// threads to use
    /// </summary>
    public int Threads { get; set; } = 1;
}