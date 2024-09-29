using NightlyCode.Ai.Net.Configurations;
using NightlyCode.Ai.Neurons;

namespace NightlyCode.Ai.Net;

/// <summary>
/// classic feed forward implementation of a neuronal net
/// </summary>
public class FeedForwardNet : INeuronalNet<FeedForwardConfiguration> {
    FeedForwardConfiguration configuration;

    /// <summary>
    /// creates a new <see cref="FeedForwardNet"/>
    /// </summary>
    /// <param name="configuration">access to configuration</param>
    public FeedForwardNet(FeedForwardConfiguration configuration) {
        this.configuration = configuration;
        Input = new(configuration.Inputs.Select(i => new NamedNeuron { Name = i }).ToArray());
        Output = new(configuration.Outputs.Select(n => new NamedNeuron { Name = n }).ToArray());
        Neurons = new Neuron[configuration.Layers * configuration.LayerSize];
        for (int i = 0; i < Neurons.Length; ++i)
            Neurons[i] = new();
    }

    /// <inheritdoc />
    public NamedNeurons Input { get; }

    /// <inheritdoc />
    public NamedNeurons Output { get; }
    
    Neuron[] Neurons { get; set; }

    /// <inheritdoc />
    public void Compute() {
        int offset = 0;
        for (int i = 0; i < configuration.LayerSize; ++i) {
            Neurons[i].Value = Input.Select(n => n.Value * configuration.Weights[offset++])
                                    .Aggregate(configuration.Aggregates[i])
                                    .Activation(configuration.ActivationFunctions[i]);
        }

        for (int layer = 1; layer < configuration.Layers; ++layer) {
            for (int i = 0; i < configuration.LayerSize; ++i) {
                int sourceIndex = (layer - 1) * configuration.LayerSize;
                int neuronIndex = layer * configuration.LayerSize + i;
                Neurons[neuronIndex].Value = Neurons.Skip(sourceIndex + i)
                                                    .Take(configuration.LayerSize)
                                                    .Select(n => n.Value * configuration.Weights[offset++])
                                                    .Aggregate(configuration.Aggregates[neuronIndex])
                                                    .Activation(configuration.ActivationFunctions[neuronIndex]);
            }
        }

        int lastRow = (configuration.Layers - 1) * configuration.LayerSize;
        for (int i = 0; i < Output.Length; ++i) {
            int neuronIndex = configuration.Layers * configuration.LayerSize;
            Output[i] = Neurons.Skip(lastRow)
                               .Select(n => n.Value * configuration.Weights[offset++])
                               .Aggregate(configuration.Aggregates[neuronIndex + i])
                               .Activation(configuration.ActivationFunctions[neuronIndex + i]);
        }
    }

    public void Update(FeedForwardConfiguration configuration) {
        this.configuration = configuration;
    }
}