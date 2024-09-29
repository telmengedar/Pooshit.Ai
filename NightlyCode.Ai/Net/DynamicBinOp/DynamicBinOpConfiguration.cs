using NightlyCode.Ai.Extensions;
using NightlyCode.Ai.Genetics;
using NightlyCode.Ai.Genetics.Mutation;
using NightlyCode.Ai.Net.Configurations;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.DynamicBinOp;

/// <summary>
/// configuration for a dynamic binary operation net
/// </summary>
public class DynamicBinOpConfiguration : IMutatingChromosome<DynamicBinOpConfiguration> {
    
    /// <summary>
    /// creates a new <see cref="DynamicBinOpConfiguration"/>
    /// </summary>
    public DynamicBinOpConfiguration() { }

    /// <summary>
    /// creates a new <see cref="DynamicBinOpConfiguration"/>
    /// </summary>
    /// <param name="inputs">name of input nodes</param>
    /// <param name="outputs">name of output nodes</param>
    /// <param name="setup">initialization parameters</param>
    public DynamicBinOpConfiguration(string[] inputs, string[] outputs, CrossSetup setup=null) {
        setup ??= new();
        setup.Rng ??= new();
        setup.OperationTypes ??= new(new MutationEntry<OperationType>(OperationType.Add, 1.0));
        setup.AggregateTypes ??= new(new MutationEntry<AggregateType>(AggregateType.Sum, 1.0));
        setup.ActivationFuncs ??= new(new MutationEntry<ActivationFunc>(ActivationFunc.None, 1.0));

        int index = 0;
        Inputs = inputs.Select(i => new NamedBinOpNeuronData {
                                                                 Name = i,
                                                                 Index = index++,
                                                                 OrderNumber = 0.0f,
                                                             }).ToArray();
        Outputs = outputs.Select(o => new NamedBinOpNeuronData {
                                                                   Name = o,
                                                                   Index = index++,
                                                                   OrderNumber = 1.0f,
                                                                   Aggregate = setup.NextAggregate(),
                                                                   Activation = setup.NextFunc()
                                                               }).ToArray();
        Neurons = [];
        Connections = [];
    }

    /// <summary>
    /// input neurons
    /// </summary>
    public NamedBinOpNeuronData[] Inputs { get; private init; }

    /// <summary>
    /// output neurons
    /// </summary>
    public NamedBinOpNeuronData[] Outputs { get; private init; }

    public BinOpNeuronData[] Neurons { get; private init; }

    /// <summary>
    /// connection weights
    /// </summary>
    public BinOpConnection[] Connections { get; private init; }
    
    BinOpConnection CreateConnection(CrossSetup setup) {
        BinOpNeuronData[] unconnectedSources = Inputs.Concat(Neurons).Where(n => Connections.All(c => c.Lhs != n.Index && c.Rhs != n.Index)).ToArray();

        BinOpNeuronData[] candidates = unconnectedSources.Length > 0 ? unconnectedSources : All.Except(Outputs).ToArray();
        if (candidates.Length == 0)
            return null;
        
        BinOpNeuronData lhs = candidates[setup.Rng.NextInt(candidates.Length)];

        unconnectedSources = unconnectedSources.Where(s => s != lhs).ToArray();
        candidates = unconnectedSources.Length > 0 ? unconnectedSources : All.Except(Outputs).Where(n => n != lhs).ToArray();
        if (candidates.Length == 0)
            return null;
        
        BinOpNeuronData rhs = candidates[setup.Rng.NextInt(candidates.Length)];

        float maxOrder = Math.Max(lhs.OrderNumber, rhs.OrderNumber);
        candidates = All.Where(n => n.OrderNumber > maxOrder).ToArray();
        if (candidates.Length == 0)
            return null;

        BinOpNeuronData target = candidates[setup.Rng.NextInt(candidates.Length)];
        if (target == null)
            return null;

        BinOpConnection connection = new() {
                                               Lhs = lhs.Index,
                                               Rhs = rhs.Index,
                                               Target = target.Index,
                                               Weight = setup.NextWeight(),
                                               Operation = setup.NextOperation()
                                           };

        if (Connections.Any(c => c.Lhs == connection.Lhs && c.Rhs == connection.Rhs && c.Target == connection.Target))
            return null;

        return connection;
    }

    /// <inheritdoc />
    public DynamicBinOpConfiguration Mutate(CrossSetup setup) {
        setup.Rng ??= new();
        setup.OperationTypes ??= new();
        setup.AggregateTypes ??= new();
        setup.ActivationFuncs ??= new();

        List<BinOpNeuronData> neurons = [..Neurons.Select(n => n.Clone())];
        List<NamedBinOpNeuronData> outputs = [..Outputs.Select(n => n.Clone())];
        List<BinOpConnection> connections = [..Connections.Select(c => c.Clone())];


        switch (setup.Rng.NextInt(12)) {
            default:
            case 0:
            case 1:
            case 2:
            case 3: {
                if (connections.Count > 0) {
                    int connectionIndex = setup.Rng.NextInt(connections.Count);
                    connections[connectionIndex].Weight += setup.NextWeight();
                }
                else {
                    BinOpConnection connection = CreateConnection(setup);
                    if (connection != null)
                        connections.Add(connection);
                }

                break;
            }
            case 5: {
                if (connections.Count > 0) {
                    int connectionIndex = setup.Rng.NextInt(connections.Count);
                    connections[connectionIndex].Operation = setup.NextOperation();
                    connections[connectionIndex].Weight = setup.NextWeight();
                }
                else {
                    BinOpConnection connection = CreateConnection(setup);
                    if (connection != null)
                        connections.Add(connection);
                }

                break;
            }
            case 7: {
                int neuronIndex = setup.Rng.NextInt(Outputs.Length + Neurons.Length);
                if (neuronIndex < Outputs.Length)
                    Outputs[neuronIndex].Aggregate = setup.NextAggregate();
                else Neurons[neuronIndex - Outputs.Length].Aggregate = setup.NextAggregate();
                break;
            }
            case 8: {
                int neuronIndex = setup.Rng.NextInt(Outputs.Length + Neurons.Length);
                if (neuronIndex < Outputs.Length)
                    Outputs[neuronIndex].Activation = setup.NextFunc();
                else Neurons[neuronIndex - Outputs.Length].Activation = setup.NextFunc();
                break;
            }
            case 4:
            case 6:
            case 9:
            case 10:
            case 11: {
                int indexOf = connections.IndexOf(c => c.Rhs == -1);
                if (indexOf > -1) {
                    BinOpNeuronData lhs = GetNeuron(Inputs, outputs, neurons, connections[indexOf].Lhs);
                    BinOpNeuronData target = GetNeuron(Inputs, outputs, neurons, connections[indexOf].Target);
                    BinOpNeuronData[] candidates = All.Where(t => t != lhs && t.OrderNumber < target.OrderNumber).ToArray();
                    if (candidates.Length > 0) {
                        BinOpNeuronData rhs = candidates[setup.Rng.NextInt(candidates.Length)];
                        connections[indexOf].Rhs = rhs.Index;
                    }
                }
                else {
                    int count = neurons.Count(n => connections.All(c => c.Lhs != n.Index && c.Rhs != n.Index)) + Inputs.Count(n => connections.All(c => c.Lhs != n.Index && c.Rhs != n.Index));
                    if (count > 0) {
                        BinOpConnection connection = CreateConnection(setup);
                        if (connection != null)
                            connections.Add(connection);
                    }
                    else if (connections.Count > 0) {
                        BinOpConnection connection = connections[setup.Rng.NextInt(connections.Count)];
                        float maxOrder = connection.Rhs == -1 ? GetNeuron(Inputs, outputs, neurons, connection.Lhs).OrderNumber : Math.Max(GetNeuron(Inputs, outputs, neurons, connection.Lhs).OrderNumber, GetNeuron(Inputs, outputs, neurons, connection.Rhs).OrderNumber);

                        BinOpNeuronData neuron = new() {
                                                           OrderNumber = (GetNeuron(Inputs, outputs, neurons, connection.Target).OrderNumber + maxOrder) * 0.5f,
                                                           Index = neurons.Count,
                                                           Aggregate = setup.NextAggregate(),
                                                           Activation = setup.NextFunc()
                                                       };
                        BinOpConnection newConnection = new() {
                                                                  Lhs = Inputs.Length + outputs.Count + neurons.Count,
                                                                  Rhs = -1,
                                                                  Target = connection.Target,
                                                                  Operation = setup.NextOperation(),
                                                                  Weight = setup.NextWeight()
                                                              };
                        connection.Target = newConnection.Lhs;
                        neurons.Add(neuron);
                        connections.Add(newConnection);
                    }
                    else {
                        BinOpConnection connection = CreateConnection(setup);
                        if (connection != null)
                            connections.Add(connection);
                    }
                }
            }
            break;
        }

        return new() {
                         Inputs = Inputs,
                         Outputs = outputs.ToArray(),
                         Neurons = neurons.ToArray(),
                         Connections = connections.OrderBy(c => GetNeuron(Inputs, outputs, neurons, c.Target).OrderNumber).ToArray()
                     };
    }

    IEnumerable<BinOpNeuronData> All {
        get {
            foreach (NamedBinOpNeuronData input in Inputs)
                yield return input;
            foreach (NamedBinOpNeuronData output in Outputs)
                yield return output;
            foreach (BinOpNeuronData neuron in Neurons)
                yield return neuron;
        }
    }
    
    BinOpNeuronData GetNeuron(NamedBinOpNeuronData[] inputs, List<NamedBinOpNeuronData> outputs, List<BinOpNeuronData> neurons, int index) {
        if (index < inputs.Length)
            return inputs[index];
        index -= inputs.Length;
        if (index < outputs.Count)
            return outputs[index];
        index -= outputs.Count;
        return neurons[index];
    }
    
    /// <summary>
    /// get neuron by index
    /// </summary>
    /// <param name="index">index of neuron to get</param>
    /// <returns>neuron</returns>
    public BinOpNeuronData GetNeuron(int index) {
        if (index < Inputs.Length)
            return Inputs[index];
        index -= Inputs.Length;
        if (index < Outputs.Length)
            return Outputs[index];
        index -= Outputs.Length;
        return Neurons[index];
    }

    /// <inheritdoc />
    public void Randomize(CrossSetup setup = null) {
        setup ??= new();
        setup.Rng ??= new();
        setup.OperationTypes ??= new();
        setup.AggregateTypes ??= new();
        setup.ActivationFuncs ??= new();

        foreach (NamedBinOpNeuronData output in Outputs)
            output.Randomize(setup);

        foreach (BinOpNeuronData neuron in Neurons)
            neuron.Randomize(setup);
    }

    public int StructureHash() {
        int hash = 0;
        foreach (BinOpConnection connection in Connections) {
            hash *= 397;
            hash ^= connection.GetHashCode();
        }

        foreach (BinOpNeuronData neuron in Neurons) {
            hash *= 397;
            hash ^= neuron.GetHashCode();
        }

        return hash;
    }

    /// <inheritdoc />
    public float FitnessModifier => 1.0f / (Connections.Length * 0.01f + Neurons.Length * 0.008f);
}