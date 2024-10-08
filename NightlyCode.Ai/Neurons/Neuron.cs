using System.Globalization;

namespace NightlyCode.Ai.Neurons;

/// <summary>
/// a neuron in a neuronal net
/// </summary>
public class Neuron {
    
    /// <summary>
    /// value of neuron
    /// </summary>
    public float Value { get; set; }
    
    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}