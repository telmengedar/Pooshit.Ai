namespace Pooshit.Ai.Net.Operations;

/// <summary>
/// group of operations
/// </summary>
public class NeuronalOperationGroup {
    
    /// <summary>
    /// grouped operations
    /// </summary>
    public NeuronalOperation[] Operations { get; set; }

    /// <summary>
    /// neuron to which result is stored
    /// </summary>
    public NeuronIndex Output { get; set; }
    
    /// <summary>
    /// aggregate to apply to operation results
    /// </summary>
    public AggregateType Aggregate { get; set; }
}