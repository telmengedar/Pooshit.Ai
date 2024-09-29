namespace NightlyCode.Ai.Neurons;

/// <summary>
/// connects two neurons
/// </summary>
public class Connection {
    
    /// <summary>
    /// neuron from which the original value is pulled
    /// </summary>
    public Neuron Neuron { get; set; }

    /// <summary>
    /// weight of connection
    /// </summary>
    public int WeightIndex { get; set; }
}