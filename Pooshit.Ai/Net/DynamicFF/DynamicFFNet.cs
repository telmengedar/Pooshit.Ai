using Pooshit.Ai.Extensions;
using NeuronConfig = Pooshit.Ai.Neurons.NeuronConfig;

namespace Pooshit.Ai.Net.DynamicFF;

/// <summary>
/// feed forward net which provides a dynamic set of neurons
/// </summary>
public class DynamicFFNet : INeuronalNet<DynamicFFConfiguration> {
    DynamicFFConfiguration configuration;
    float[] neurons;
    readonly Dictionary<string, int> named = new();

    /// <summary>
    /// creates a new <see cref="DynamicFFNet"/>
    /// </summary>
    /// <param name="configuration">neuronal configuration</param>
    public DynamicFFNet(DynamicFFConfiguration configuration) {
        this.configuration = configuration;
        foreach(NeuronConfig neuron in configuration.Neurons)
            if (!string.IsNullOrEmpty(neuron.Name))
                named[neuron.Name] = neuron.Index;
        neurons = new float[configuration.Neurons.Length];
    }

    /// <inheritdoc />
    public float this[string name] {
        get => neurons[named[name]];
        set => neurons[named[name]] = value;
    }

    /// <inheritdoc />
    public float this[int index] {
        get => neurons[index];
        set => neurons[index] = value;
    }

    /// <inheritdoc />
    public void Compute() {
        foreach (IGrouping<int, FFConnection> group in configuration.GroupedConnections) {
            NeuronConfig targetConfig = configuration[group.Key];

            neurons[group.Key] = group.Select(g => neurons[g.Source] * g.Weight)
                                      .Aggregate(targetConfig.Aggregate)
                                      .Activation(targetConfig.Activation);
        }
    }

    /// <inheritdoc />
    public void SetInputValues(float[] values) {
        if (values.Length != configuration.InputCount)
            throw new ArgumentException("Invalid number of values");
        Array.Copy(values, neurons, values.Length);
    }

    /// <inheritdoc />
    public void Update(DynamicFFConfiguration configuration) {
        this.configuration = configuration;

        int neuronCount = configuration.Neurons.Length;
        if (neurons.Length < neuronCount)
            Array.Resize(ref neurons, neuronCount);
        else
            for (int i = configuration.InputCount; i < configuration.InputCount + configuration.OutputCount; ++i)
                neurons[i] = 0.0f;
    }
}