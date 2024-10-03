namespace NightlyCode.Ai.Net.DynamicBinOp;

/// <summary>
/// base class for neuron configs
/// </summary>
public abstract class NeuronConfig {
    
    /// <summary>
    /// ordernumber for neuronal flow
    /// </summary>
    public float OrderNumber { get; set; }

    /// <summary>
    /// index of neuron in net
    /// </summary>
    public int Index { get; set; }

    public TargetNeuronConfig Clone() {
        return new() {
                         OrderNumber = OrderNumber,
                         Index = Index
                     };
    }

    public override int GetHashCode() {
        return HashCode.Combine(OrderNumber, Index);
    }
}