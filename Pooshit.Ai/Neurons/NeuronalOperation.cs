namespace Pooshit.Ai.Neurons;

/// <summary>
/// operation in a neuronal net
/// </summary>
public class NeuronalOperation {
    
    /// <summary>
    /// connections used to generate value
    /// </summary>
    public Connection[] Connections { get; set; }
    
    /// <summary>
    /// neuron where result is stored
    /// </summary>
    public Neuron Neuron { get; set; }
}