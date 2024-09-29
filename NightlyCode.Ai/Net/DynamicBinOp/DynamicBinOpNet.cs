using NightlyCode.Ai.Neurons;

namespace NightlyCode.Ai.Net.DynamicBinOp;

public class DynamicBinOpNet : INeuronalNet<DynamicBinOpConfiguration> {
    DynamicBinOpConfiguration configuration;

    public DynamicBinOpNet(DynamicBinOpConfiguration configuration) {
        this.configuration = configuration;
        Input = new(configuration.Inputs.Select(i => new NamedNeuron { Name = i.Name }).ToArray());
        Output = new(configuration.Outputs.Select(n => new NamedNeuron { Name = n.Name }).ToArray());
        Neurons = this.configuration.Neurons.Select(n => new Neuron()).ToArray();
        for (int i = 0; i < Neurons.Length; ++i)
            Neurons[i] = new();
    }
    
    Neuron[] Neurons { get; set; }

    public NamedNeurons Input { get; }
    
    public NamedNeurons Output { get; }
    
    public void Compute() {
        foreach (IGrouping<int, BinOpConnection> group in configuration.Connections.GroupBy(c => c.Target)) {
            BinOpNeuronData targetConfig=configuration.GetNeuron(group.Key);
            Neuron target = GetNeuron(group.Key);
            float order = targetConfig.OrderNumber;

            foreach (BinOpConnection connection in group) {
                if(connection.Rhs==-1)
                    GetNeuron(connection.Target).Value = GetNeuron(connection.Lhs).Value * connection.Weight;
                else {
                    target.Value = group.Select(g => {
                                                    if (g.Rhs == -1)
                                                        return GetNeuron(connection.Lhs).Value * connection.Weight;

                                                    return NMath.Compute(GetNeuron(connection.Lhs).Value, GetNeuron(connection.Rhs).Value, connection.Operation) * connection.Weight;
                                                }).Aggregate(targetConfig.Aggregate)
                                        .Activation(targetConfig.Activation);
                }
            }
        }
    }

    Neuron GetNeuron(int index) {
        if (index < Input.Length)
            return Input.GetNeuron(index);
        index -= Input.Length;
        if (index < Output.Length)
            return Output.GetNeuron(index);
        index -= Output.Length;
        return Neurons[index];
    }
    
    /// <inheritdoc />
    public void Update(DynamicBinOpConfiguration configuration) {
        this.configuration = configuration;
        foreach (NamedNeuron output in Output)
            output.Value = 0.0f;
        Neurons = this.configuration.Neurons.Select(n => new Neuron()).ToArray();
    }
}