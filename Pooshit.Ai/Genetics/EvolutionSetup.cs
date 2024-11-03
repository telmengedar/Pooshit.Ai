using Pooshit.Ai.Genetics.Mutation;

namespace Pooshit.Ai.Genetics;

/// <summary>
/// configuration values used for evolution
/// </summary>
/// <typeparam name="T">type of chromosome to evolve</typeparam>
public class EvolutionSetup<T> 
where T : IChromosome<T>
{
    
    /// <summary>
    /// evaluator used to determine fitness of chromosomes
    /// </summary>
    public IFitnessEvaluator<T> Evaluator { get; set; }

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
    public double Elitism { get; set; } = 0.1;

    /// <summary>
    /// rates for mutation
    /// </summary>
    public MutationSetup Mutation { get; } = new();
    
    /// <summary>
    /// action executed after training run
    /// </summary>
    public Action<int, float> AfterRun { get; set; }
    
    /// <summary>
    /// threads to use
    /// </summary>
    public int Threads { get; set; } = 1;
}