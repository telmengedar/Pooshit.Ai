using NightlyCode.Ai.Extern;
using NightlyCode.Ai.Genetics;
using NightlyCode.Ai.Genetics.Mutation;
using NightlyCode.Ai.Net.Operations;
using NeuronConfig = NightlyCode.Ai.Neurons.NeuronConfig;

namespace NightlyCode.Ai.Net.Dynamic;

/// <summary>
/// configuration for a dynamic feed forward net
/// </summary>
public class DynamicFFConfiguration : IMutatingChromosome<DynamicFFConfiguration> {
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
                                                         new MutationEntry<ActivationFunc>(ActivationFunc.Ceiling, 0.3));

    int? inputCount;
    int? outputCount;

    ILookup<int, FFConnection> groupedConnections;

    /// <summary>
    /// creates a new <see cref="DynamicFFConfiguration"/>
    /// </summary>
    public DynamicFFConfiguration() { }
    
    /// <summary>
    /// creates a new <see cref="DynamicFFConfiguration"/>
    /// </summary>
    /// <param name="inputs">name of input nodes</param>
    /// <param name="outputs">name of output nodes</param>
    /// <param name="rng">randomizer</param>
    public DynamicFFConfiguration(string[] inputs, string[] outputs, IRng rng=null) {
        rng ??= new Rng();
        int index = 0;

        Neurons = new NeuronConfig[inputs.Length + outputs.Length];
        foreach (string input in inputs)
            Neurons[index] = new() {
                                         Name = input,
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
    /// creates a new <see cref="DynamicFFConfiguration"/>
    /// </summary>
    /// <param name="inputs">number of input nodes</param>
    /// <param name="outputs">name of output nodes</param>
    /// <param name="rng">randomizer</param>
    public DynamicFFConfiguration(int inputs, string[] outputs, IRng rng=null) {
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
    /// creates a new <see cref="DynamicFFConfiguration"/>
    /// </summary>
    /// <param name="neurons">neurons</param>
    /// <param name="connections"></param>
    public DynamicFFConfiguration(NeuronConfig[] neurons, FFConnection[] connections) {
        Neurons = neurons;
        inputCount = neurons.Count(n => n.OrderNumber <= 0.0f);
        outputCount = neurons.Count(n => n.OrderNumber >= 1.0f);
        Connections = connections;
    }

    /// <summary>
    /// contained neuron configs
    /// </summary>
    public NeuronConfig[] Neurons { get; set; }

    /// <summary>
    /// number of input neurons
    /// </summary>
    public int InputCount => inputCount ??= Neurons.Count(n => n.OrderNumber <= 0.0f);

    /// <summary>
    /// number of input neurons
    /// </summary>
    public int OutputCount => outputCount ??= Neurons.Count(n => n.OrderNumber >= 1.0f);

    /// <summary>
    /// neuronal connections
    /// </summary>
    public FFConnection[] Connections { get; set; }
    
    /// <summary>
    /// indexer for neurons
    /// </summary>
    /// <param name="index">index of neuron</param>
    public NeuronConfig this[int index] => Neurons[index];

    /// <summary>
    /// connections grouped by target neuron
    /// </summary>
    public ILookup<int, FFConnection> GroupedConnections {
        get { return groupedConnections ??= Connections.ToLookup(c => c.Target); }
    }

    FFConnection CreateConnection(IRng rng) {
        NeuronConfig[] candidates = Neurons.Where(n => n.OrderNumber < 1.0f 
                                                               && Connections.All(c => c.Source != n.Index))
                                                   .ToArray();

        if (candidates.Length == 0)
            candidates = Neurons.Where(n => n.OrderNumber < 1.0f
                                            && Connections.All(c => c.Source != n.Index))
                                .ToArray();
        
        if (candidates.Length == 0)
            return null;
        
        NeuronConfig source = candidates[rng.NextInt(candidates.Length)];
        
        candidates = Neurons.Where(n => n.OrderNumber > source.OrderNumber).ToArray();
        if (candidates.Length == 0)
            return null;

        NeuronConfig target = candidates[rng.NextInt(candidates.Length)];

        FFConnection connection = new() {
                                            Source = source.Index,
                                            Target = target.Index,
                                            Weight = rng.NextFloatRange(),
                                        };

        if (Connections.Any(c => c.Source == connection.Source && c.Target == connection.Target))
            return null;

        return connection;
    }

    void AddConnection(List<FFConnection> connections, int source, int target, float weight) {
        FFConnection connection = new() {
                                            Source = source,
                                            Target = target,
                                            Weight = weight
                                        };
        connections.Add(connection);
        connections.Sort((lhs, rhs) => Comparer<float>.Default.Compare(this[lhs.Target].OrderNumber, this[rhs.Target].OrderNumber));
    }
    
    bool AddConnection(List<FFConnection> connections, IRng rng) {
        FFConnection connection = CreateConnection(rng);
        if (connection != null) {
            connections.Add(connection);
            connections.Sort((lhs, rhs) => Comparer<float>.Default.Compare(this[lhs.Target].OrderNumber, this[rhs.Target].OrderNumber));
            return true;
        }

        return false;
    }

    void ChangeConnectionWeight(List<FFConnection> connections, IRng rng, float mutationRange) {
        if (connections.Count == 0)
            return;
        connections[rng.NextInt(connections.Count)].Weight += rng.NextFloatRange() * mutationRange;
    }

    void ChangeNeuron(List<NeuronConfig> newNeurons, IRng rng) {
        int neuronIndex = rng.NextInt(newNeurons.Count);
        newNeurons[neuronIndex].Aggregate = aggregateTypes.SelectItem(rng);
        newNeurons[neuronIndex].Activation = activationFuncs.SelectItem(rng);
    }

    Tuple<int,int> GetRandomConnection(IRng rng) {
        NeuronConfig[] candidates = Neurons.Where(n=>n.OrderNumber<1.0f).ToArray();
        int sourceIndex = candidates[rng.NextInt(candidates.Length)].Index;

        float sourceOrder = Neurons[sourceIndex].OrderNumber;
        candidates = Neurons.Where(n => n.OrderNumber > sourceOrder).ToArray();

        return new(sourceIndex, candidates[rng.NextInt(candidates.Length)].Index);
    }
    
    /// <inheritdoc />
    public DynamicFFConfiguration Mutate(IRng rng, float mutationRange) {
        List<NeuronConfig> newNeurons = [..Neurons.Select(n => n.Clone())];
        List<FFConnection> connections = [..Connections.Select(c => c.Clone())];

        float dice = rng.NextFloat();

        if (connections.Count == 0 || dice < 0.7) {
            if (connections.Count != 0)
                ChangeConnectionWeight(connections, rng, mutationRange);
            else
                AddConnection(connections, rng);
        }
        else if (dice < 0.92) {
            ChangeNeuron(newNeurons, rng);
        }
        else {
            Tuple<int, int> connectionIndices = GetRandomConnection(rng);
            FFConnection candidate = connections.FirstOrDefault(c => c.Source == connectionIndices.Item1 && c.Target == connectionIndices.Item2);
            if (candidate == null) {
                AddConnection(connections, connectionIndices.Item1, connectionIndices.Item2, rng.NextFloatRange() * mutationRange);
            }
            else {
                NeuronConfig neuron = new() {
                                                OrderNumber = rng.NextFloat(),
                                                Index = newNeurons.Count,
                                                Aggregate = aggregateTypes.SelectItem(rng),
                                                Activation = activationFuncs.SelectItem(rng)
                                            };
                FFConnection newConnection = new() {
                                                       Source = neuron.Index,
                                                       Target = candidate.Target,
                                                       Weight = rng.NextFloatRange()
                                                   };
                candidate.Target = newConnection.Source;
                newNeurons.Add(neuron);
                connections.Add(newConnection);
                connections.Sort((lhs, rhs) => Comparer<float>.Default.Compare(newNeurons[lhs.Target].OrderNumber, newNeurons[rhs.Target].OrderNumber));
            }
        }

        return new(newNeurons.ToArray(), connections.ToArray());
    }


    public void Randomize(CrossSetup setup = null) {
        setup ??= new();
        setup.Rng ??= new Rng();

        foreach (NeuronConfig neuron in Neurons.Skip(InputCount)) {
            neuron.Aggregate = aggregateTypes.SelectItem(setup.Rng);
            neuron.Activation = activationFuncs.SelectItem(setup.Rng);
        }
    }

    /// <inheritdoc />
    public int StructureHash() {
        int hash = 0;
        foreach (FFConnection connection in Connections) {
            hash *= 397;
            hash ^= connection.StructureHash;
        }
        
        return hash;
    }

    /// <inheritdoc />
    public float FitnessModifier => 1.0f + (Connections.Length * 0.01f + (Neurons.Length - InputCount - OutputCount) * 0.008f);
    
    public DynamicFFConfiguration Optimize(Func<DynamicFFConfiguration, bool> test) => throw new NotImplementedException();

    public override string ToString() {
        return $"Neurons:\n{string.Join<NeuronConfig>("\n", Neurons)}\nConnections:\n{string.Join<FFConnection>("\n", Connections)}";
    }
}