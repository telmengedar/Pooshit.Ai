using System.Text;
using NightlyCode.Ai.Extern;
using NightlyCode.Ai.Genetics;
using NightlyCode.Ai.Genetics.Mutation;
using NightlyCode.Ai.Net.Configurations;
using NightlyCode.Ai.Net.Operations;

namespace NightlyCode.Ai.Net.DynamicBinOp;

/// <summary>
/// configuration for a dynamic binary operation net
/// </summary>
public class DynamicBinOpConfiguration : IMutatingChromosome<DynamicBinOpConfiguration> {
    static readonly OperationTypeOptions operationTypes = new(new MutationEntry<OperationType>(OperationType.Multiply, 1.0),
                                                              new MutationEntry<OperationType>(OperationType.Add, 1.0),
                                                              new MutationEntry<OperationType>(OperationType.Div, 1.0),
                                                              new MutationEntry<OperationType>(OperationType.Sub, 1.0));

    readonly AggregateTypeOptions aggregateTypes = new(new MutationEntry<AggregateType>(AggregateType.Sum, 1.0),
                                                       new MutationEntry<AggregateType>(AggregateType.Average, 1.0),
                                                       new MutationEntry<AggregateType>(AggregateType.Median, 0.1),
                                                       new MutationEntry<AggregateType>(AggregateType.Min, 1.0),
                                                       new MutationEntry<AggregateType>(AggregateType.Max, 1.0));

    readonly ActivationFuncOptions activationFuncs = new(new MutationEntry<ActivationFunc>(ActivationFunc.None, 1.0),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.BinaryStep, 0.4),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Sigmoid, 0.25),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Sin, 0.2),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.ReLU, 0.08),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.LeakyReLU, 0.08),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Tanh, 0.2),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Reciprocal, 0.4),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Swish, 0.05));

    NeuronConfig[] neuronsByIndex;

    ILookup<int, BinOpConnection> groupedConnections;
    
    /// <summary>
    /// creates a new <see cref="DynamicBinOpConfiguration"/>
    /// </summary>
    public DynamicBinOpConfiguration() { }

    /// <summary>
    /// creates a new <see cref="DynamicBinOpConfiguration"/>
    /// </summary>
    /// <param name="inputs">name of input nodes</param>
    /// <param name="outputs">name of output nodes</param>
    /// <param name="rng">randomizer</param>
    public DynamicBinOpConfiguration(string[] inputs, string[] outputs, IRng rng=null) {
        rng ??= new Rng();
        int index = 0;
        Inputs = inputs.Select(i => new NamedNeuronConfig {
                                                              Name = i,
                                                              Index = index++,
                                                              OrderNumber = 0.0f,
                                                          }).ToArray();
        Outputs = outputs.Select(o => new NamedTargetNeuronConfig {
                                                                   Name = o,
                                                                   Index = index++,
                                                                   OrderNumber = 1.0f,
                                                                   Aggregate = aggregateTypes.SelectItem(rng),
                                                                   Activation = activationFuncs.SelectItem(rng)
                                                               }).ToArray();
        Neurons = [];
        Connections = [];
    }

    /// <summary>
    /// creates a new <see cref="DynamicBinOpConfiguration"/>
    /// </summary>
    /// <param name="inputs">input neurons</param>
    /// <param name="outputs">output neurons</param>
    /// <param name="neurons">neurons</param>
    /// <param name="connections"></param>
    public DynamicBinOpConfiguration(NamedNeuronConfig[] inputs, NamedTargetNeuronConfig[] outputs, TargetNeuronConfig[] neurons, BinOpConnection[] connections) {
        Inputs = inputs;
        Outputs = outputs;
        Neurons = neurons;
        Connections = connections;
    }

    /// <summary>
    /// indexer for neurons
    /// </summary>
    /// <param name="index">index of neuron</param>
    public NeuronConfig this[int index] => NeuronsByIndex[index];

    NeuronConfig[] NeuronsByIndex {
        get {
            return neuronsByIndex ??= Inputs.Concat(Outputs.Cast<NeuronConfig>()).Concat(Neurons).ToArray();
        }
    }

    /// <summary>
    /// input neurons
    /// </summary>
    public NamedNeuronConfig[] Inputs { get; set; }

    /// <summary>
    /// output neurons
    /// </summary>
    public NamedTargetNeuronConfig[] Outputs { get; set; }

    /// <summary>
    /// free neurons
    /// </summary>
    public TargetNeuronConfig[] Neurons { get; set; }

    /// <summary>
    /// connection weights
    /// </summary>
    public BinOpConnection[] Connections { get; set; }

    public ILookup<int, BinOpConnection> GroupedConnections {
        get { return groupedConnections ??= Connections.ToLookup(c => c.Target); }
    }
    
    BinOpConnection CreateConnection(IRng rng, float mutationRange) {
        NeuronConfig[] unconnectedSources = Inputs.Cast<NeuronConfig>()
                                                  .Concat(Neurons)
                                                  .Where(n => Connections.All(c => c.Lhs != n.Index && c.Rhs != n.Index)).ToArray();

        NeuronConfig[] candidates = unconnectedSources.Length > 0 ? unconnectedSources : NeuronsByIndex.Except(Outputs).ToArray();
        if (candidates.Length == 0)
            return null;
        
        NeuronConfig lhs = candidates[rng.NextInt(candidates.Length)];

        unconnectedSources = unconnectedSources.Where(s => s != lhs).ToArray();
        candidates = unconnectedSources.Length > 0 ? unconnectedSources : NeuronsByIndex.Except(Outputs).Where(n => n != lhs).ToArray();

        NeuronConfig rhs = null;
        if(candidates.Length>0)
            rhs = candidates[rng.NextInt(candidates.Length)];

        float maxOrder = Math.Max(lhs.OrderNumber, rhs?.OrderNumber??0.0f);
        candidates = NeuronsByIndex.Where(n => n.OrderNumber > maxOrder).ToArray();
        if (candidates.Length == 0)
            return null;

        NeuronConfig target = candidates[rng.NextInt(candidates.Length)];
        if (target == null)
            return null;

        BinOpConnection connection = new() {
                                               Lhs = lhs.Index,
                                               Rhs = rhs?.Index ?? -1,
                                               Target = target.Index,
                                               Weight = rng.NextFloatRange(),
                                               Operation = operationTypes.SelectItem(rng)
                                           };

        if (Connections.Any(c => c.Lhs == connection.Lhs && c.Rhs == connection.Rhs && c.Target == connection.Target))
            return null;

        return connection;
    }


    bool AddConnection(List<BinOpConnection> connections, IRng rng, float mutationRange) {
        BinOpConnection connection = CreateConnection(rng, mutationRange);
        if (connection != null) {
            connections.Add(connection);
            connections.Sort((lhs, rhs) => Comparer<float>.Default.Compare(this[lhs.Target].OrderNumber, this[rhs.Target].OrderNumber));
            return true;
        }

        return false;
    }

    void ChangeConnectionWeight(List<BinOpConnection> connections, IRng rng, float mutationRange) {
        if (connections.Count == 0)
            return;
        connections[rng.NextInt(connections.Count)].Weight += rng.NextFloatRange() * mutationRange;
    }

    void ChangeConnectionType(List<BinOpConnection> connections, IRng rng, float mutationRange) {
        BinOpConnection connection = connections[rng.NextInt(connections.Count)];
        connection.Operation = operationTypes.SelectItem(rng);
        connection.Weight = rng.NextFloatRange() * mutationRange;
    }

    void ChangeNeuron(List<TargetNeuronConfig> neurons, List<NamedTargetNeuronConfig> outputs, IRng rng) {
        int neuronIndex = rng.NextInt(outputs.Count + neurons.Count);
        if (neuronIndex < Outputs.Length) {
            outputs[neuronIndex].Aggregate = aggregateTypes.SelectItem(rng);
            outputs[neuronIndex].Activation = activationFuncs.SelectItem(rng);
        }
        else {
            neurons[neuronIndex - outputs.Count].Aggregate = aggregateTypes.SelectItem(rng);
            neurons[neuronIndex - outputs.Count].Activation = activationFuncs.SelectItem(rng);
        }
    }
    
    /// <inheritdoc />
    public DynamicBinOpConfiguration Mutate(IRng rng, float mutationRange) {
        List<TargetNeuronConfig> neurons = [..Neurons.Select(n => n.Clone())];
        List<NamedTargetNeuronConfig> outputs = [..Outputs.Select(n => n.Clone())];
        List<BinOpConnection> connections = [..Connections.Select(c => c.Clone())];

        float dice = rng.NextFloat();

        if (connections.Count == 0 || dice < 0.5) {
            if (connections.Any())
                ChangeConnectionWeight(connections, rng, mutationRange);
            else
                AddConnection(connections, rng, mutationRange);
        }
        else if (dice < 0.65) {
            if (connections.Any())
                ChangeConnectionType(connections, rng, mutationRange);
            else AddConnection(connections, rng, mutationRange);
        }
        else if (dice < 0.8) {
            ChangeNeuron(neurons, outputs, rng);
        }
        else {
            BinOpConnection[] unconnectedCandidates = connections.Where(c => c.Rhs == -1).ToArray();
            if (unconnectedCandidates.Length > 0 && rng.NextFloat()<0.85f) {
                BinOpConnection connection = unconnectedCandidates[rng.NextInt(unconnectedCandidates.Length)];
                NeuronConfig lhs = this[connection.Lhs];
                NeuronConfig target = this[connection.Target];
                NeuronConfig[] candidates = NeuronsByIndex.Where(t => t != lhs && t.OrderNumber < target.OrderNumber).ToArray();
                if (candidates.Length > 0) {
                    NeuronConfig rhs = candidates[rng.NextInt(candidates.Length)];
                    connection.Rhs = rhs.Index;
                }
            }
            else {
                int count = neurons.Count(n => connections.All(c => c.Lhs != n.Index && c.Rhs != n.Index)) + Inputs.Count(n => connections.All(c => c.Lhs != n.Index && c.Rhs != n.Index));
                if (count > 0) {
                    if (!AddConnection(connections, rng, mutationRange))
                        ChangeConnectionWeight(connections, rng, mutationRange);
                }
                else if (connections.Count > 0) {
                    BinOpConnection[] candidates = connections.Where(c => c.Lhs < Inputs.Length && c.Rhs < Inputs.Length && c.Target < Inputs.Length + Outputs.Length).ToArray();
                    if (candidates.Length > 0) {
                        BinOpConnection connection = candidates[rng.NextInt(candidates.Length)];
                        TargetNeuronConfig neuron = new() {
                                                              OrderNumber = rng.NextFloat(),
                                                              Index = Inputs.Length + outputs.Count + neurons.Count,
                                                              Aggregate = aggregateTypes.SelectItem(rng),
                                                              Activation = activationFuncs.SelectItem(rng)
                                                          };
                        BinOpConnection newConnection = new() {
                                                                  Lhs = neuron.Index,
                                                                  Rhs = -1,
                                                                  Target = connection.Target,
                                                                  Operation = operationTypes.SelectItem(rng),
                                                                  Weight = rng.NextFloatRange()
                                                              };
                        connection.Target = newConnection.Lhs;
                        neurons.Add(neuron);
                        connections.Add(newConnection);
                        connections.Sort((lhs, rhs) => Comparer<float>.Default.Compare(GetNeuron(neurons, lhs.Target).OrderNumber, GetNeuron(neurons, rhs.Target).OrderNumber));
                    }
                    else {
                        if (!AddConnection(connections, rng, mutationRange))
                            ChangeConnectionWeight(connections, rng, mutationRange);
                    }
                }
                else {
                    if (!AddConnection(connections, rng, mutationRange))
                        ChangeConnectionWeight(connections, rng, mutationRange);
                }
            }
        }

        return new(Inputs, outputs.ToArray(), neurons.ToArray(), connections.ToArray());
    }
    
    NeuronConfig GetNeuron(List<TargetNeuronConfig> neurons, int index) {
        if (index < NeuronsByIndex.Length)
            return NeuronsByIndex[index];
        return neurons.Last();
    }
    
    /// <summary>
    /// get neuron by index
    /// </summary>
    /// <param name="index">index of neuron to get</param>
    /// <returns>neuron</returns>
    public TargetNeuronConfig GetTargetNeuron(int index) {
        return this[index] as TargetNeuronConfig;
    }

    /// <inheritdoc />
    public void Randomize(CrossSetup setup = null) {
        setup ??= new();
        setup.Rng ??= new Rng();

        foreach (NamedTargetNeuronConfig output in Outputs) {
            output.Aggregate = aggregateTypes.SelectItem(setup.Rng);
            output.Activation = activationFuncs.SelectItem(setup.Rng);
        }

        foreach (TargetNeuronConfig neuron in Neurons) {
            neuron.Aggregate = aggregateTypes.SelectItem(setup.Rng);
            neuron.Activation = activationFuncs.SelectItem(setup.Rng);
        }
    }

    public int StructureHash() {
        int hash = 0;
        foreach (BinOpConnection connection in Connections) {
            hash *= 397;
            hash ^= connection.StructureHash;
        }
        
        return hash;
    }

    /// <inheritdoc />
    public float FitnessModifier => 1.0f + (Connections.Length * 0.01f + Neurons.Length * 0.008f);

    /// <inheritdoc />
    public DynamicBinOpConfiguration Optimize(Func<DynamicBinOpConfiguration, bool> test) {
        List<TargetNeuronConfig> neurons = [..Neurons.Select(n => n.Clone())];
        List<NamedTargetNeuronConfig> outputs = [..Outputs.Select(n => n.Clone())];
        List<BinOpConnection> connections = [..Connections.Select(c => c.Clone())];

        float threshold = 0.1f;
        foreach (BinOpConnection connection in connections) {
            float original = connection.Weight;
            if (Math.Abs(connection.Weight - Math.Floor(connection.Weight)) <= threshold) {
                connection.Weight = (float)Math.Floor(connection.Weight);
                if (!test(new(Inputs, outputs.ToArray(), neurons.ToArray(), connections.ToArray())))
                    connection.Weight = original;
            }
            else if (Math.Abs(connection.Weight - Math.Ceiling(connection.Weight)) <= threshold) {
                connection.Weight = (float)Math.Ceiling(connection.Weight);
                if (!test(new(Inputs, outputs.ToArray(), neurons.ToArray(), connections.ToArray())))
                    connection.Weight = original;
            }
        }

        return new(Inputs, outputs.ToArray(), neurons.ToArray(), connections.ToArray());
    }

    /// <inheritdoc />
    public override string ToString() {
        StringBuilder sb = new();
        sb.AppendLine("Inputs:");
        foreach (NamedNeuronConfig input in Inputs)
            sb.Append("\t").AppendLine(input.ToString());
        sb.AppendLine("Outputs:");
        foreach (NamedTargetNeuronConfig output in Outputs)
            sb.Append("\t").AppendLine(output.ToString());
        sb.AppendLine("Neurons:");
        foreach (TargetNeuronConfig neuron in Neurons)
            sb.Append("\t").AppendLine(neuron.ToString());
        sb.AppendLine("Connections:");
        foreach (BinOpConnection connection in Connections)
            sb.Append("\t").AppendLine(connection.ToString());
        return sb.ToString();
    }
}