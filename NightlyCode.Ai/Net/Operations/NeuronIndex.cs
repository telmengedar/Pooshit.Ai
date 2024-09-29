namespace NightlyCode.Ai.Net.Operations;

/// <summary>
/// index of a neuron in a neuronal net
/// </summary>
public class NeuronIndex {
    
    /// <summary>
    /// creates a new <see cref="NeuronIndex"/>
    /// </summary>
    /// <param name="layer">layer of neuron</param>
    /// <param name="neuron">index of neuron</param>
    public NeuronIndex(int layer, int neuron) {
        Layer = layer;
        Neuron = neuron;
    }

    /// <summary>
    /// layer of neuron (-1 is input, -2 is output)
    /// </summary>
    public int Layer { get; set; }
    
    /// <summary>
    /// index of neuron
    /// </summary>
    public int Neuron { get; set; }

    /// <summary>
    /// layer index of input layer
    /// </summary>
    public const int InputLayer = -1;

    /// <summary>
    /// layer index of output layer
    /// </summary>
    public const int OutputLayer = int.MaxValue;

    protected bool Equals(NeuronIndex other) => Layer == other.Layer && Neuron == other.Neuron;

    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((NeuronIndex)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Layer, Neuron);
}