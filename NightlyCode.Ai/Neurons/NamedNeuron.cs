using System.Globalization;

namespace NightlyCode.Ai.Neurons;

/// <summary>
/// neuron used as input parameters
/// </summary>
public class NamedNeuron : Neuron {
    
    /// <summary>
    /// name of input
    /// </summary>
    /// <remarks>
    /// can be used as a parametername
    /// </remarks>
    public string Name { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{Name}: {Value.ToString(CultureInfo.InvariantCulture)}";
}