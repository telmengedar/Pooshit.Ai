using NightlyCode.Ai.Net.Configurations;
using NightlyCode.Ai.Net.Operations;
using NightlyCode.Ai.Neurons;
using NeuronalOperation = NightlyCode.Ai.Net.Operations.NeuronalOperation;

namespace NightlyCode.Ai.Net;

/// <summary>
/// net for complex neuronal operations
/// </summary>
public class NeuronalOperationNet : INeuronalNet<NeuronalOperationNetConfiguration> {
    NeuronalOperationNetConfiguration configuration;

    /// <summary>
    /// creates a new <see cref="NeuronalOperationNet"/>
    /// </summary>
    /// <param name="configuration">configuration used to build neuronal net</param>
    public NeuronalOperationNet(NeuronalOperationNetConfiguration configuration) {
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

    Neuron GetNeuron(NeuronIndex index) {
        return GetNeuron(index.Layer, index.Neuron);
    }
    
    Neuron GetNeuron(int layerIndex, int neuronIndex) {
        switch (layerIndex) {
            case NeuronIndex.InputLayer:
                return Input.GetNeuron(neuronIndex);
            case NeuronIndex.OutputLayer:
                return Output.GetNeuron(neuronIndex);
            default:
                return Neurons[layerIndex * configuration.LayerSize + neuronIndex];
        }
    }

    float Compute(NeuronalOperation operation) {
        float operationValue;
        if (operation.Rhs == null)
            operationValue = GetNeuron(operation.Lhs).Value;
        else
            operationValue = NMath.Compute(GetNeuron(operation.Lhs).Value, GetNeuron(operation.Rhs).Value, operation.Operation);
        return operationValue * operation.Weight;
    }
    
    /// <inheritdoc />
    public void Compute() {
        foreach (NeuronalOperationGroup group in configuration.OperationGroups) {
            GetNeuron(group.Output).SetValue(group.Operations.Select(Compute), group.Aggregate);
        }
    }

    /// <inheritdoc />
    public void Update(NeuronalOperationNetConfiguration configuration) {
        this.configuration = configuration;
    }
}