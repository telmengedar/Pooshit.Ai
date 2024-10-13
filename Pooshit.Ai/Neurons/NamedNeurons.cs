using System.Collections;

namespace Pooshit.Ai.Neurons;

/// <summary>
/// collection of named neurons
/// </summary>
public class NamedNeurons : IEnumerable<NamedNeuron> {
    readonly NamedNeuron[] neurons;

    /// <summary>
    /// collection of named neurons
    /// </summary>
    /// <param name="neurons"></param>
    public NamedNeurons(params NamedNeuron[] neurons) {
        this.neurons = neurons;
    }

    /// <summary>
    /// access to neuron values
    /// </summary>
    /// <param name="name">name of neuron to access</param>
    public float this[string name] {
        get => GetNeuron(name).Value;
        set => GetNeuron(name).Value = value;
    }

    /// <summary>
    /// indexer using index
    /// </summary>
    /// <param name="index">index of neuron</param>
    public float this[int index] {
        get => neurons[index].Value;
        set => neurons[index].Value = value;
    }
    
    /// <summary>
    /// number of neurons in collections
    /// </summary>
    public int Length => neurons.Length;
    
    NamedNeuron GetNeuron(string name) {
        NamedNeuron neuron = neurons.FirstOrDefault(n => n.Name == name);
        if (neuron == null)
            throw new ArgumentException($"Neuron with name '{name}' not found");
        return neuron;
    }

    /// <summary>
    /// get neuron by index
    /// </summary>
    /// <param name="neuronIndex">index of neuron</param>
    /// <returns>neuron at the specified index</returns>
    public NamedNeuron GetNeuron(int neuronIndex) {
        return neurons[neuronIndex];
    }

    /// <inheritdoc />
    public IEnumerator<NamedNeuron> GetEnumerator() => ((IEnumerable<NamedNeuron>)neurons).GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}