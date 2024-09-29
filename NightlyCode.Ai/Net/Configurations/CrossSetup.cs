using NightlyCode.Ai.Extern;
using NightlyCode.Ai.Genetics.Mutation;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.Configurations;

/// <summary>
/// setup for crossing chromosomes
/// </summary>
public class CrossSetup {
    
    /// <summary>
    /// rate of mutated chromosomes
    /// </summary>
    public float MutateChance { get; set; } = 0.1f;
    
    /// <summary>
    /// base chance for weight mutation
    /// </summary>
    /// <returns></returns>
    public float MutateRate { get; set; } = 0.07f;

    /// <summary>
    /// base range for weight mutation
    /// </summary>
    public float MutateRange { get; set; } = 1.0f;
    
    /// <summary>
    /// valid operation types for mutation
    /// </summary>
    public OperationTypeOptions OperationTypes { get; set; }

    /// <summary>
    /// valid aggregate types for mutation
    /// </summary>
    public AggregateTypeOptions AggregateTypes { get; set; }

    /// <summary>
    /// valid activation functions
    /// </summary>
    public ActivationFuncOptions ActivationFuncs { get; set; }
    
    /// <summary>
    /// random number generator to use
    /// </summary>
    public Rng Rng { get; set; }

    public float NextWeight() {
        return -MutateRange + Rng.NextFloat() * MutateRange * 2.0f;
    }

    public OperationType NextOperation() {
        return OperationTypes.SelectItem(Rng);
    }

    public AggregateType NextAggregate() {
        return AggregateTypes.SelectItem(Rng);
    }

    public ActivationFunc NextFunc() {
        return ActivationFuncs.SelectItem(Rng);
    }
}