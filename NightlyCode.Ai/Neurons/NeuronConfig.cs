using System.Diagnostics;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Neurons;

/// <summary>
/// config for a neuron
/// </summary>
public class NeuronConfig {

    /// <summary>
    /// name of neuron (optional)
    /// </summary>
    /// <remarks>
    /// used for input and output neurons
    /// </remarks>
    public string Name { get; set; }
    
    /// <summary>
    /// ordernumber for neuronal flow
    /// </summary>
    public float OrderNumber { get; set; }

    /// <summary>
    /// index of neuron in net
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// aggregate to use for input values of this neuron
    /// </summary>
    public AggregateType Aggregate { get; set; }

    /// <summary>
    /// activation function to use
    /// </summary>
    public ActivationFunc Activation { get; set; }

    /// <summary>
    /// clones this neuron
    /// </summary>
    /// <returns>cloned neuron config</returns>
    public NeuronConfig Clone() {
        return new() {
                         Name = Name,
                         OrderNumber = OrderNumber,
                         Index = Index,
                         Aggregate = Aggregate,
                         Activation = Activation
                     };
    }

    public override string ToString() {
        return $"{Index}{(!string.IsNullOrEmpty(Name) ? $"({Name})" : "")} - {(Activation == ActivationFunc.None ? (Aggregate) : $"{Activation}({Aggregate})")}";
    }
}