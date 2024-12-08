using System.Text;
using Pooshit.Ai.Extensions;
using Pooshit.Ai.Extern;
using Pooshit.Ai.Genetics;
using Pooshit.Ai.Genetics.Mutation;
using Pooshit.Ai.Net.Operations;
using Pooshit.Ai.Neurons;

namespace Pooshit.Ai.Net.DynamicBO;

/// <summary>
/// configuration for a dynamic binary operation net
/// </summary>
public class DynamicBOConfiguration : IMutatingChromosome<DynamicBOConfiguration> {
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
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Swish, 0.05),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Floor, 0.3),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Ceiling, 0.3),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Sqrt, 0.05),
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Pow2, 0.05));

    ILookup<int, BOConnection> groupedConnections;
    int? inputCount;
    int? outputCount;
    
    /// <summary>
    /// creates a new <see cref="DynamicBOConfiguration"/>
    /// </summary>
    public DynamicBOConfiguration() { }

    /// <summary>
    /// creates a new <see cref="DynamicBOConfiguration"/>
    /// </summary>
    /// <param name="inputs">name of input nodes</param>
    /// <param name="outputs">name of output nodes</param>
    /// <param name="rng">randomizer</param>
    public DynamicBOConfiguration(string[] inputs, string[] outputs, IRng rng = null)
        : this(inputs.Select(i => new NeuronSpec { Name = i }).ToArray(), outputs, rng) { }

    /// <summary>
    /// creates a new <see cref="DynamicBOConfiguration"/>
    /// </summary>
    /// <param name="inputs">specification of input nodes</param>
    /// <param name="outputs">name of output nodes</param>
    /// <param name="rng">randomizer</param>
    public DynamicBOConfiguration(NeuronSpec[] inputs, string[] outputs, IRng rng=null) {
        rng ??= new Rng();
        int index = 0;
        Neurons = new NeuronConfig[inputs.Length + outputs.Length];
        foreach (NeuronSpec input in inputs)
            Neurons[index] = new() {
                Name = input.Name,
                Generator = input.Generator,
                Index = index++,
                OrderNumber = 0.0f,
            };
        inputCount = inputs.Length;
        foreach (string output in outputs) {
            Neurons[index] = new() {
                Name = output,
                Index = index++,
                OrderNumber = 1.0f,
                Aggregate = aggregateTypes.SelectItem(rng),
                Activation = activationFuncs.SelectItem(rng)
            };
        }

        outputCount = outputs.Length;
        Connections = [];
    }

    /// <summary>
    /// creates a new <see cref="DynamicBOConfiguration"/>
    /// </summary>
    /// <param name="inputs">number of input nodes</param>
    /// <param name="outputs">name of output nodes</param>
    /// <param name="rng">randomizer</param>
    public DynamicBOConfiguration(int inputs, string[] outputs, IRng rng=null) {
        rng ??= new Rng();
        Neurons = new NeuronConfig[inputs + outputs.Length];

        for (int i = 0; i < inputs; ++i)
            Neurons[i] = new() { Index = i };
        inputCount = inputs;
        int index = inputs;
        foreach (string output in outputs) {
            Neurons[index] = new() {
                                              Name = output,
                                              Index = index++,
                                              OrderNumber = 1.0f,
                                              Aggregate = aggregateTypes.SelectItem(rng),
                                              Activation = activationFuncs.SelectItem(rng)
                                          };
        }

        outputCount = outputs.Length;
        Connections = [];
    }

    /// <summary>
    /// creates a new <see cref="DynamicBOConfiguration"/>
    /// </summary>
    /// <param name="neurons">neurons</param>
    /// <param name="connections">neuronal connections</param>
    public DynamicBOConfiguration(NeuronConfig[] neurons, BOConnection[] connections) {
        Neurons = neurons;
        inputCount = neurons.Count(n => n.OrderNumber <= 0.0f);
        outputCount = neurons.Count(n => n.OrderNumber >= 1.0f);
        Connections = connections;
    }

    /// <summary>
    /// indexer for neurons
    /// </summary>
    /// <param name="index">index of neuron</param>
    public NeuronConfig this[int index] => Neurons[index];

    /// <summary>
    /// neurons in net
    /// </summary>
    public NeuronConfig[] Neurons { get; set; }

    /// <summary>
    /// connection weights
    /// </summary>
    public BOConnection[] Connections { get; set; }

    /// <summary>
    /// number of input neurons
    /// </summary>
    public int InputCount => inputCount ??= Neurons.Count(n => n.OrderNumber <= 0.0f);

    /// <summary>
    /// number of output neurons
    /// </summary>
    public int OutputCount => outputCount ??= Neurons.Count(n => n.OrderNumber >= 1.0f);
    
    public ILookup<int, BOConnection> GroupedConnections {
        get { return groupedConnections ??= Connections.ToLookup(c => c.Target); }
    }
    
    void ChangeConnectionWeight(List<BOConnection> connections, IRng rng, float mutationRange) {
        if (connections.Count == 0)
            return;
        connections[rng.NextInt(connections.Count)].Weight += rng.NextFloatRange() * mutationRange;
    }

    void ChangeConnectionType(List<BOConnection> connections, IRng rng, float mutationRange) {
        BOConnection connection = connections[rng.NextInt(connections.Count)];
        connection.Operation = operationTypes.SelectItem(rng);
        connection.Weight = rng.NextFloatRange() * mutationRange;
    }

    void ChangeNeuron(List<NeuronConfig> neurons, IRng rng) {
        NeuronConfig neuron = neurons[InputCount + rng.NextInt(neurons.Count - InputCount)];
        neuron.Aggregate = aggregateTypes.SelectItem(rng);
        neuron.Activation = activationFuncs.SelectItem(rng);
    }
    
    Tuple<int,int,int> GetRandomConnection(IRng rng) {
        NeuronConfig[] candidates = Neurons.Where(n=>n.OrderNumber<1.0f).ToArray();
        int lhsIndex = candidates[rng.NextInt(candidates.Length)].Index;

        candidates = Neurons.Where(n=>n.Index!=lhsIndex && n.OrderNumber<1.0f).ToArray();
        int rhsIndex = candidates.Length > 0 ? candidates[rng.NextInt(candidates.Length)].Index : -1;

        float sourceOrder = rhsIndex >= 0 ? Math.Max(Neurons[lhsIndex].OrderNumber, Neurons[rhsIndex].OrderNumber) : Neurons[lhsIndex].OrderNumber;
        candidates = Neurons.Where(n => n.OrderNumber > sourceOrder).ToArray();

        return new(lhsIndex, rhsIndex, candidates[rng.NextInt(candidates.Length)].Index);
    }

    void AddConnection(List<BOConnection> connections, int lhs, int rhs, int target, IRng rng) {
        BOConnection connection = new() {
                                               Lhs = lhs,
                                               Rhs = rhs,
                                               Target = target,
                                               Operation = operationTypes.SelectItem(rng),
                                               Weight = rng.NextFloatRange()
                                           };
        connections.Add(connection);
        connections.Sort((lhsC, rhsC) => Comparer<float>.Default.Compare(this[lhsC.Target].OrderNumber, this[rhsC.Target].OrderNumber));
    }

    void RemoveConnection(BOConnection connectionToRemove, List<BOConnection> connections, List<NeuronConfig> neurons) {
        if (!connections.Remove(connectionToRemove))
            return;
        
        NeuronConfig[] obsolete = neurons.Where(n => n.OrderNumber is > 0.0f and < 1.0f && connections.All(c => c.Lhs != n.Index && c.Rhs != n.Index))
                                         .Concat(neurons.Where(n => n.OrderNumber is > 0.0f and < 1.0f && connections.All(c => c.Target != n.Index)))
                                         .ToArray();

        BOConnection[] obsoleteConnections = connections.Where(c => obsolete.Any(n => n.Index == c.Lhs || n.Index == c.Rhs || n.Index == c.Target))
                                                        .ToArray();
        foreach (BOConnection obsoleteConnection in obsoleteConnections)
            RemoveConnection(obsoleteConnection, connections, neurons);

        foreach (NeuronConfig neuronToRemove in obsolete) {
            if (!neurons.Remove(neuronToRemove))
                continue;

            foreach (BOConnection connection in connections) {
                if (connection.Lhs > neuronToRemove.Index)
                    --connection.Lhs;
                if (connection.Rhs > neuronToRemove.Index)
                    --connection.Rhs;
                if (connection.Target > neuronToRemove.Index)
                    --connection.Target;
            }
            
            foreach(NeuronConfig neuron in neurons)
                if (neuron.Index > neuronToRemove.Index)
                    --neuron.Index;
        }
    }
    
    /// <inheritdoc />
    public DynamicBOConfiguration Mutate(IRng rng, float mutationRange) {
        List<NeuronConfig> newNeurons = [..Neurons.Select(n => n.Clone())];
        List<BOConnection> connections = [..Connections.Select(c => c.Clone())];

        float dice = rng.NextFloat();

        if (connections.Count > 0 && dice < 0.9) {
            if (dice < 0.5) {
                ChangeConnectionWeight(connections, rng, mutationRange);
            }
            else if (dice < 0.75) {
                ChangeConnectionType(connections, rng, mutationRange);
            }
            else 
                ChangeNeuron(newNeurons, rng);
        }
        else if (connections.Count > 0 && dice < 0.94) {
            Tuple<int, int, int> connectionData = GetRandomConnection(rng);
            BOConnection connection = connections.FirstOrDefault(c => c.Lhs == connectionData.Item1 && (c.Rhs == connectionData.Item2 || c.Rhs == -1) && c.Target == connectionData.Item3);
            if (connection != null) {
                RemoveConnection(connection, connections, newNeurons);
            }
        }
        else {
            Tuple<int, int, int> connectionData = GetRandomConnection(rng);
            BOConnection connection = connections.FirstOrDefault(c => c.Lhs == connectionData.Item1 && (c.Rhs == connectionData.Item2 || c.Rhs == -1) && c.Target == connectionData.Item3);

            if (connection == null) {
                AddConnection(connections, connectionData.Item1, connectionData.Item2, connectionData.Item3, rng);
            }
            else {
                if (connection.Rhs == -1 && rng.NextFloat() < 0.8) {
                    HashSet<int> existing = [..connections.Where(c => c.Lhs == connection.Lhs && c.Rhs >= 0 && c.Target == connection.Target).Select(c => c.Rhs)];
                    float targetOrder = Neurons[connection.Target].OrderNumber;
                    NeuronConfig rhsNeuron = Neurons.Where(n => !existing.Contains(n.Index) && n.OrderNumber < targetOrder).RandomItem(rng);
                    if (rhsNeuron != null)
                        connection.Rhs = rhsNeuron.Index;
                }
                else {
                    NeuronConfig neuron = new() {
                                                    OrderNumber = rng.NextFloat(),
                                                    Index = newNeurons.Count,
                                                    Aggregate = aggregateTypes.SelectItem(rng),
                                                    Activation = activationFuncs.SelectItem(rng)
                                                };
                    BOConnection newConnection = new() {
                                                              Lhs = neuron.Index,
                                                              Rhs = -1,
                                                              Target = connection.Target,
                                                              Operation = operationTypes.SelectItem(rng),
                                                              Weight = rng.NextFloatRange()
                                                          };
                    connection.Target = newConnection.Lhs;
                    newNeurons.Add(neuron);
                    connections.Add(newConnection);
                    connections.Sort((lhs, rhs) => Comparer<float>.Default.Compare(GetNeuron(newNeurons, lhs.Target).OrderNumber, GetNeuron(newNeurons, rhs.Target).OrderNumber));
                }
            }
        }

        return new(newNeurons.ToArray(), connections.ToArray());
    }
    
    NeuronConfig GetNeuron(List<NeuronConfig> neurons, int index) {
        if (index < Neurons.Length)
            return Neurons[index];
        return neurons.Last();
    }
    
    /// <summary>
    /// get neuron by index
    /// </summary>
    /// <param name="index">index of neuron to get</param>
    /// <returns>neuron</returns>
    public NeuronConfig GetTargetNeuron(int index) {
        return this[index];
    }

    /// <inheritdoc />
    public void Randomize(CrossSetup setup = null) {
        setup ??= new();
        setup.Rng ??= new Rng();

        foreach (NeuronConfig neuron in Neurons) {
            neuron.Aggregate = aggregateTypes.SelectItem(setup.Rng);
            neuron.Activation = activationFuncs.SelectItem(setup.Rng);
        }
    }

    public int StructureHash() {
        int hash = 0;
        /*foreach (NeuronConfig neuron in Neurons) {
            hash *= 397;
            hash ^= neuron.StructureHash;
        }*/
        
        foreach (BOConnection connection in Connections) {
            hash *= 397;
            hash ^= connection.StructureHash;
        }
        
        return hash;
    }

    /// <inheritdoc />
    public float FitnessModifier => 1.0f + (Connections.Length * 0.01f + (Neurons.Length - InputCount - OutputCount) * 0.008f);

    /// <inheritdoc />
    public DynamicBOConfiguration Optimize(Func<DynamicBOConfiguration, bool> test) {
        List<NeuronConfig> neurons = [..Neurons.Select(n => n.Clone())];
        List<BOConnection> connections = [..Connections.Select(c => c.Clone())];

        float threshold = 0.1f;
        foreach (BOConnection connection in connections) {
            float original = connection.Weight;
            if (Math.Abs(connection.Weight - Math.Floor(connection.Weight)) <= threshold) {
                connection.Weight = (float)Math.Floor(connection.Weight);
                if (!test(new(neurons.ToArray(), connections.ToArray())))
                    connection.Weight = original;
            }
            else if (Math.Abs(connection.Weight - Math.Ceiling(connection.Weight)) <= threshold) {
                connection.Weight = (float)Math.Ceiling(connection.Weight);
                if (!test(new(neurons.ToArray(), connections.ToArray())))
                    connection.Weight = original;
            }
        }

        return new(neurons.ToArray(), connections.ToArray());
    }

    /// <inheritdoc />
    public override string ToString() {
        StringBuilder sb = new();
        sb.AppendLine("Neurons:");
        foreach (NeuronConfig neuron in Neurons)
            sb.Append("\t").AppendLine(neuron.ToString());
        sb.AppendLine("Connections:");
        foreach (BOConnection connection in Connections)
            sb.Append("\t").AppendLine(connection.ToString());
        return sb.ToString();
    }
}